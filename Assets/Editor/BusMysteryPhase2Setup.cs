using System.Collections.Generic;
using System.IO;
using BusMystery.Bus;
using BusMystery.Passengers;
using BusMystery.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace BusMystery.Editor
{
    public static class BusMysteryPhase2Setup
    {
        private const string BusScenePath = "Assets/Scenes/Bus.unity";
        private const string PassengerDataFolder = "Assets/Data/Passengers";

        [MenuItem("Tools/BusMystery/Setup Phase 2")]
        public static void SetupPhase2()
        {
            EnsureFolders();
            var passengers = EnsurePassengerData();
            AssetDatabase.SaveAssets();

            if (!File.Exists(BusScenePath))
            {
                Debug.LogError("Assets/Scenes/Bus.unity was not found. Run Tools > BusMystery > Setup Phase 1 before Setup Phase 2.");
                return;
            }

            var scene = EditorSceneManager.OpenScene(BusScenePath);
            var canvas = Object.FindFirstObjectByType<Canvas>();
            var viewController = Object.FindFirstObjectByType<BusViewController>();

            if (canvas == null || viewController == null)
            {
                Debug.LogError("Bus scene is missing Phase 1 Canvas or BusViewController. Run Setup Phase 1, then Setup Phase 2 again.");
                return;
            }

            RemoveGeneratedObject("Phase 2 Passenger Hotspots");
            RemoveGeneratedObject("Passenger Memory Panel");

            var memoryUI = CreateMemoryUI(canvas.transform);
            CreateHotspots(canvas.transform, viewController, memoryUI, passengers);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("BusMystery Phase 2 setup complete.");
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
            {
                Directory.CreateDirectory("Assets/Data");
            }

            if (!AssetDatabase.IsValidFolder(PassengerDataFolder))
            {
                Directory.CreateDirectory(PassengerDataFolder);
            }
        }

        private static List<PassengerMemoryData> EnsurePassengerData()
        {
            return new List<PassengerMemoryData>
            {
                EnsurePassenger(
                    "young_man",
                    "20代男性",
                    BusViewDirection.Right,
                    new Vector2(0.20f, 0.31f),
                    new Vector2(0.41f, 0.63f),
                    new[]
                    {
                        "駅までは、まだ少し余裕があるはずだった。",
                        "母さんの車で、弟と一緒に向かっていた。初めてのデートで、妙に落ち着かなかった。",
                        "少し早く家を出たから、遅れる心配はないと思っていた。",
                        "スマホに『もうすぐ着く』って送った。画面ばかり見ていた。",
                        "母さんが減速したかどうかも、後ろに何がいたのかも、ちゃんとは覚えていない。",
                        "次に顔を上げた時には、音が全部を塗りつぶしていた。"
                    }),
                EnsurePassenger(
                    "elementary_brother",
                    "小学生の弟",
                    BusViewDirection.Right,
                    new Vector2(0.43f, 0.28f),
                    new Vector2(0.64f, 0.60f),
                    new[]
                    {
                        "お兄ちゃんの用事についていくの、ちょっとだけ楽しかった。",
                        "車の中で好きな曲を流してもらって、小さな声で歌っていた。",
                        "窓の外を見たら、後ろに白い車がいた。",
                        "「あの車、ママの車と同じ色だったな。」",
                        "車の名前とか、どれくらい速かったとか、運転していた人までは分からない。",
                        "お兄ちゃんはスマホを見ていて、ぼくの方が外を見ていたと思う。",
                        "大きな音がして、体がぎゅっと押された。",
                        "「お兄ちゃんはいるけど、ママはどこにいるの？」",
                        "そこから先は、思い出せない。"
                    }),
                EnsurePassenger(
                    "commuting_woman",
                    "30代女性",
                    BusViewDirection.Left,
                    new Vector2(0.50f, 0.30f),
                    new Vector2(0.73f, 0.64f),
                    new[]
                    {
                        "仕事帰りだった。いつもは最寄りのひとつ手前で降りて、そこからバスに乗る。",
                        "その日も駅の近くを歩いていた。車に乗っていたわけじゃない。",
                        "人の流れと信号の音だけがあって、いつも通りの帰り道だった。",
                        "突然、大きな衝突音がした。",
                        "振り向くより先に、タイヤの音が近づいてきた。車がこっちへ来る音だった。",
                        "でも、車そのものは見ていない。",
                        "何かが迫ってくると分かった瞬間、記憶はそこで途切れている。"
                    })
            };
        }

        private static PassengerMemoryData EnsurePassenger(string id, string displayName, BusViewDirection viewDirection, Vector2 anchorMin, Vector2 anchorMax, string[] lines)
        {
            var path = $"{PassengerDataFolder}/{id}.asset";
            var data = AssetDatabase.LoadAssetAtPath<PassengerMemoryData>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<PassengerMemoryData>();
                AssetDatabase.CreateAsset(data, path);
            }

            var serialized = new SerializedObject(data);
            serialized.FindProperty("id").stringValue = id;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("viewDirection").enumValueIndex = (int)viewDirection;
            serialized.FindProperty("hotspotAnchorMin").vector2Value = anchorMin;
            serialized.FindProperty("hotspotAnchorMax").vector2Value = anchorMax;

            var memoryLines = serialized.FindProperty("memoryLines");
            memoryLines.arraySize = lines.Length;
            for (var i = 0; i < lines.Length; i++)
            {
                memoryLines.GetArrayElementAtIndex(i).stringValue = lines[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(data);
            return data;
        }

        private static PassengerMemoryUI CreateMemoryUI(Transform canvasTransform)
        {
            var panel = CreateImage("Passenger Memory Panel", canvasTransform, new Color(0.02f, 0.025f, 0.03f, 0.94f));
            SetRect(panel.rectTransform, new Vector2(0.08f, 0.05f), new Vector2(0.92f, 0.34f), new Vector2(0.5f, 0.5f), Vector2.zero);

            var speaker = CreateText("Speaker Label", panel.transform, "乗客", 26f, TextAlignmentOptions.Left);
            SetRect(speaker.rectTransform, new Vector2(0.04f, 0.72f), new Vector2(0.96f, 0.92f), new Vector2(0f, 0.5f), Vector2.zero);
            speaker.color = new Color(1f, 0.92f, 0.62f);

            var body = CreateText("Memory Body Label", panel.transform, "", 28f, TextAlignmentOptions.Left);
            SetRect(body.rectTransform, new Vector2(0.04f, 0.18f), new Vector2(0.96f, 0.70f), new Vector2(0f, 0.5f), Vector2.zero);
            body.textWrappingMode = TextWrappingModes.Normal;

            var hint = CreateText("Memory Hint Label", panel.transform, "クリック / Enter / Space：次へ　ESC：閉じる", 18f, TextAlignmentOptions.Right);
            SetRect(hint.rectTransform, new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.16f), new Vector2(1f, 0.5f), Vector2.zero);
            hint.color = new Color(0.72f, 0.76f, 0.82f);

            var memoryUI = panel.gameObject.AddComponent<PassengerMemoryUI>();
            SetObjectReference(memoryUI, "panelRoot", panel.gameObject);
            SetObjectReference(memoryUI, "speakerLabel", speaker);
            SetObjectReference(memoryUI, "bodyLabel", body);
            return memoryUI;
        }

        private static void CreateHotspots(Transform canvasTransform, BusViewController viewController, PassengerMemoryUI memoryUI, IReadOnlyList<PassengerMemoryData> passengers)
        {
            var root = new GameObject("Phase 2 Passenger Hotspots", typeof(RectTransform));
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.SetParent(canvasTransform, false);
            SetRect(rootRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            for (var i = 0; i < passengers.Count; i++)
            {
                var passenger = passengers[i];
                var button = CreateButton($"{passenger.Id} Hotspot", root.transform, passenger.DisplayName, new Color(0.28f, 0.31f, 0.34f, 0.86f), Color.white);
                SetRect(button.GetComponent<RectTransform>(), passenger.HotspotAnchorMin, passenger.HotspotAnchorMax, new Vector2(0.5f, 0.5f), Vector2.zero);

                var hotspot = button.gameObject.AddComponent<PassengerHotspot>();
                SetObjectReference(hotspot, "passengerData", passenger);
                SetObjectReference(hotspot, "viewController", viewController);
                SetObjectReference(hotspot, "memoryUI", memoryUI);
                SetObjectReference(hotspot, "label", button.GetComponentInChildren<TMP_Text>(true));
            }
        }

        private static void RemoveGeneratedObject(string objectName)
        {
            var target = GameObject.Find(objectName);
            if (target != null)
            {
                Object.DestroyImmediate(target);
            }
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
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = Color.white;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            return label;
        }

        private static Button CreateButton(string name, Transform parent, string text, Color normalColor, Color textColor)
        {
            var image = CreateImage(name, parent, normalColor);
            image.raycastTarget = true;

            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = normalColor * 1.18f;
            colors.pressedColor = normalColor * 0.82f;
            colors.selectedColor = normalColor * 1.08f;
            colors.disabledColor = new Color(0.12f, 0.12f, 0.12f, 0.6f);
            button.colors = colors;

            var label = CreateText("Label", image.transform, text, 22f, TextAlignmentOptions.Center);
            label.color = textColor;
            label.raycastTarget = false;
            SetRect(label.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero);
            return button;
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
    }
}
