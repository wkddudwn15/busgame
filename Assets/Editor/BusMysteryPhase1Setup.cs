using System.Collections.Generic;
using System.IO;
using BusMystery.Bus;
using BusMystery.Core;
using BusMystery.DebugTools;
using BusMystery.Input;
using BusMystery.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BusMystery.Editor
{
    public static class BusMysteryPhase1Setup
    {
        private const string RouteAssetPath = "Assets/Data/Phase1BusRoute.asset";
        private const string TitleScenePath = "Assets/Scenes/Title.unity";
        private const string BusScenePath = "Assets/Scenes/Bus.unity";
        private const string JapaneseFontPath = "Assets/Fonts/NotoSansJP-Regular SDF.asset";

        [MenuItem("Tools/BusMystery/Setup Phase 1")]
        public static void SetupPhase1()
        {
            EnsureFolders();
            var routeData = EnsureRouteData();
            AssetDatabase.SaveAssets();
            CreateTitleScene();
            CreateBusScene(routeData);
            EnsureBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("BusMystery Phase 1 setup complete.");
        }

        private static void EnsureFolders()
        {
            var folders = new[]
            {
                "Assets/Art",
                "Assets/Audio",
                "Assets/Data",
                "Assets/Prefabs",
                "Assets/Scenes",
                "Assets/Scripts",
                "Assets/Scripts/Bus",
                "Assets/Scripts/Core",
                "Assets/Scripts/Debug",
                "Assets/Scripts/Input",
                "Assets/Scripts/UI",
                "Assets/UI"
            };

            foreach (var folder in folders)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }
        }

        private static BusRouteData EnsureRouteData()
        {
            var routeData = AssetDatabase.LoadAssetAtPath<BusRouteData>(RouteAssetPath);
            if (routeData == null)
            {
                routeData = ScriptableObject.CreateInstance<BusRouteData>();
                AssetDatabase.CreateAsset(routeData, RouteAssetPath);
            }

            var serialized = new SerializedObject(routeData);
            var stops = serialized.FindProperty("stops");
            stops.arraySize = 8;

            SetStop(stops.GetArrayElementAtIndex(0), "sakuradai_residential", "桜台住宅前", 1, 270f, false);
            SetStop(stops.GetArrayElementAtIndex(1), "minamigaoka_park", "南ヶ丘公園前", 2, 300f, false);
            SetStop(stops.GetArrayElementAtIndex(2), "asahi_shopping_street", "旭通り商店街", 3, 270f, false);
            SetStop(stops.GetArrayElementAtIndex(3), "kirigaoka_station_east_street", "霧ヶ丘駅東口通り", 4, 330f, false);
            SetStop(stops.GetArrayElementAtIndex(4), "kirigaoka_station", "霧ヶ丘駅", 5, 300f, false);
            SetStop(stops.GetArrayElementAtIndex(5), "shirakawa_bridge", "白川橋", 6, 270f, false);
            SetStop(stops.GetArrayElementAtIndex(6), "nishino_town", "西野町", 7, 300f, false);
            SetStop(stops.GetArrayElementAtIndex(7), "unknown_terminal", "■■□#%?", 8, 360f, true);

            serialized.FindProperty("approachNoticeSeconds").floatValue = 30f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(routeData);
            return routeData;
        }

        private static void SetStop(SerializedProperty stop, string id, string displayName, int order, float seconds, bool terminal)
        {
            stop.FindPropertyRelative("id").stringValue = id;
            stop.FindPropertyRelative("displayName").stringValue = displayName;
            stop.FindPropertyRelative("order").intValue = order;
            stop.FindPropertyRelative("travelTimeFromPreviousSeconds").floatValue = seconds;
            stop.FindPropertyRelative("terminal").boolValue = terminal;
        }

        private static void CreateTitleScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Title";
            CreateCamera(new Color(0.06f, 0.07f, 0.09f));

            var canvas = CreateCanvas("Title Canvas");
            var root = CreateRect("Title Root", canvas.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var title = CreateText("Title Label", root.transform, "Bus Mystery", 64, TextAlignmentOptions.Center);
            SetRect(title.rectTransform, new Vector2(0.5f, 0.58f), new Vector2(0.5f, 0.58f), new Vector2(0.5f, 0.5f), new Vector2(720f, 110f));

            var controller = new GameObject("Title Screen Controller").AddComponent<TitleScreenController>();
            var startButton = CreateButton("Start Button", root.transform, "始める", new Color(0.22f, 0.55f, 0.82f), Color.white);
            SetRect(startButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.5f), new Vector2(240f, 64f));
            UnityEventTools.AddPersistentListener(startButton.onClick, controller.StartGame);

            CreateEventSystem();
            EditorSceneManager.SaveScene(scene, TitleScenePath);
        }

        private static void CreateBusScene(BusRouteData routeData)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Bus";
            CreateCamera(new Color(0.08f, 0.085f, 0.09f));

            var systemRoot = new GameObject("Bus Phase 1 Systems");
            var clock = systemRoot.AddComponent<BusClock>();
            var route = systemRoot.AddComponent<BusRouteController>();
            var reservation = systemRoot.AddComponent<StopReservationController>();
            var view = systemRoot.AddComponent<BusViewController>();
            var pause = systemRoot.AddComponent<PauseController>();
            var input = systemRoot.AddComponent<BusInputController>();
            var debug = systemRoot.AddComponent<DebugBusControls>();

            SetObjectReference(route, "routeData", routeData);
            SetObjectReference(route, "busClock", clock);
            SetObjectReference(reservation, "routeController", route);
            SetObjectReference(pause, "busClock", clock);
            SetObjectReference(input, "viewController", view);
            SetObjectReference(input, "pauseController", pause);
            SetObjectReference(debug, "busClock", clock);
            SetObjectReference(debug, "routeController", route);

            var canvas = CreateCanvas("Bus Canvas");
            var root = CreateRect("Bus UI Root", canvas.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var backdrop = CreateImage("View Backdrop", root.transform, new Color(0.12f, 0.13f, 0.14f));
            SetRect(backdrop.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var viewLabel = CreateText("View Label", root.transform, "FRONT VIEW", 56, TextAlignmentOptions.Center);
            SetRect(viewLabel.rectTransform, new Vector2(0.1f, 0.52f), new Vector2(0.62f, 0.78f), new Vector2(0.5f, 0.5f), Vector2.zero);

            var approachLabel = CreateText("Approach Label", root.transform, "", 34, TextAlignmentOptions.Center);
            SetRect(approachLabel.rectTransform, new Vector2(0.18f, 0.84f), new Vector2(0.82f, 0.96f), new Vector2(0.5f, 0.5f), Vector2.zero);
            approachLabel.color = new Color(1f, 0.9f, 0.45f);

            var previousLabel = CreateText("Previous Stop Label", root.transform, "直前：なし", 22, TextAlignmentOptions.Left);
            SetRect(previousLabel.rectTransform, new Vector2(0.02f, 0.89f), new Vector2(0.45f, 0.96f), new Vector2(0f, 0.5f), Vector2.zero);

            var reservationStatus = CreateText("Reservation Status Label", root.transform, "予約：なし", 22, TextAlignmentOptions.Left);
            SetRect(reservationStatus.rectTransform, new Vector2(0.02f, 0.82f), new Vector2(0.45f, 0.89f), new Vector2(0f, 0.5f), Vector2.zero);

            var eventLog = CreateText("Event Log Label", root.transform, "", 20, TextAlignmentOptions.Left);
            SetRect(eventLog.rectTransform, new Vector2(0.02f, 0.05f), new Vector2(0.55f, 0.11f), new Vector2(0f, 0.5f), Vector2.zero);

            var pauseLabel = CreateText("Pause Label", root.transform, "PAUSE", 52, TextAlignmentOptions.Center);
            SetRect(pauseLabel.rectTransform, new Vector2(0.34f, 0.43f), new Vector2(0.66f, 0.57f), new Vector2(0.5f, 0.5f), Vector2.zero);
            pauseLabel.gameObject.SetActive(false);

            CreateViewButton(root.transform, "Left View Button", "LEFT", new Vector2(0.08f, 0.12f), view.ShowLeft);
            CreateViewButton(root.transform, "Front View Button", "FRONT", new Vector2(0.24f, 0.12f), view.ShowFront);
            CreateViewButton(root.transform, "Right View Button", "RIGHT", new Vector2(0.40f, 0.12f), view.ShowRight);

            var panel = CreateImage("Reservation Panel", root.transform, new Color(0.04f, 0.045f, 0.05f, 0.92f));
            SetRect(panel.rectTransform, new Vector2(0.70f, 0.08f), new Vector2(0.98f, 0.88f), new Vector2(0.5f, 0.5f), Vector2.zero);

            var panelTitle = CreateText("Reservation Title", panel.transform, "降車停留所予約", 24, TextAlignmentOptions.Center);
            SetRect(panelTitle.rectTransform, new Vector2(0.04f, 0.90f), new Vector2(0.96f, 0.98f), new Vector2(0.5f, 0.5f), Vector2.zero);

            var container = CreateRect("Reservation Options", panel.transform, new Vector2(0.06f, 0.04f), new Vector2(0.94f, 0.88f), new Vector2(0.5f, 0.5f), Vector2.zero);
            var layout = container.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 7f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            var reservationUi = panel.gameObject.AddComponent<BusReservationUI>();
            SetObjectReference(reservationUi, "routeController", route);
            SetObjectReference(reservationUi, "reservationController", reservation);
            SetObjectReference(reservationUi, "optionContainer", container);
            SetObjectReference(reservationUi, "titleLabel", panelTitle);

            var hud = root.gameObject.AddComponent<BusHUD>();
            SetObjectReference(hud, "viewController", view);
            SetObjectReference(hud, "routeController", route);
            SetObjectReference(hud, "reservationController", reservation);
            SetObjectReference(hud, "pauseController", pause);
            SetObjectReference(hud, "viewLabel", viewLabel);
            SetObjectReference(hud, "approachLabel", approachLabel);
            SetObjectReference(hud, "previousStopLabel", previousLabel);
            SetObjectReference(hud, "reservationStatusLabel", reservationStatus);
            SetObjectReference(hud, "pauseLabel", pauseLabel);
            SetObjectReference(hud, "eventLogLabel", eventLog);

            CreateEventSystem();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, BusScenePath);
        }

        private static Camera CreateCamera(Color backgroundColor)
        {
            var cameraObject = new GameObject("Main Camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = backgroundColor;
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            return camera;
        }

        private static Canvas CreateCanvas(string name)
        {
            var canvasObject = new GameObject(name);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static void CreateEventSystem()
        {
            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            var inputModule = eventSystemObject.AddComponent<InputSystemUIInputModule>();
            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/Settings/InputSystem_Actions.inputactions");
            if (actions != null)
            {
                inputModule.actionsAsset = actions;
            }
        }

        private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            SetRect(rect, anchorMin, anchorMax, pivot, sizeDelta);
            return rect;
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            var image = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
            image.transform.SetParent(parent, false);
            image.color = color;
            return image;
        }

        private static TMP_Text CreateText(string name, Transform parent, string text, float fontSize, TextAlignmentOptions alignment)
        {
            var label = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI)).GetComponent<TMP_Text>();
            label.transform.SetParent(parent, false);
            label.text = text;
            label.font = LoadJapaneseFont();
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = Color.white;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            return label;
        }

        private static TMP_FontAsset LoadJapaneseFont()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(JapaneseFontPath);
            if (font == null)
            {
                Debug.LogError($"Japanese TMP font asset was not found: {JapaneseFontPath}");
            }

            return font;
        }

        private static Button CreateButton(string name, Transform parent, string text, Color normalColor, Color textColor)
        {
            var image = CreateImage(name, parent, normalColor);
            var button = image.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = normalColor * 1.18f;
            colors.pressedColor = normalColor * 0.82f;
            colors.selectedColor = normalColor * 1.08f;
            colors.disabledColor = new Color(0.12f, 0.12f, 0.12f, 0.6f);
            button.colors = colors;

            var label = CreateText("Label", image.transform, text, 22, TextAlignmentOptions.Center);
            label.color = textColor;
            SetRect(label.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero);
            return button;
        }

        private static void CreateViewButton(Transform parent, string name, string label, Vector2 normalizedCenter, UnityEngine.Events.UnityAction action)
        {
            var button = CreateButton(name, parent, label, new Color(0.23f, 0.25f, 0.28f), Color.white);
            SetRect(button.GetComponent<RectTransform>(), normalizedCenter, normalizedCenter, new Vector2(0.5f, 0.5f), new Vector2(180f, 56f));
            UnityEventTools.AddPersistentListener(button.onClick, action);
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = sizeDelta;
        }

        private static void SetObjectReference(Object target, string propertyName, Object reference)
        {
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogError($"Serialized property not found: {target.name}.{propertyName}");
                return;
            }

            property.objectReferenceValue = reference;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void EnsureBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>
            {
                new(TitleScenePath, true),
                new(BusScenePath, true)
            };

            foreach (var existing in EditorBuildSettings.scenes)
            {
                if (existing.path != TitleScenePath && existing.path != BusScenePath)
                {
                    scenes.Add(existing);
                }
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
