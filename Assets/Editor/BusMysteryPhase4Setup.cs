using System.IO;
using BusMystery.UI;
using BusMystery.Words;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace BusMystery.Editor
{
    public static class BusMysteryPhase4Setup
    {
        private const string BusScenePath = "Assets/Scenes/Bus.unity";
        private const string JapaneseFontPath = "Assets/Fonts/NotoSansJP-Regular SDF.asset";
        private const string RootObjectName = "Phase 4 Word Notebook UI";

        [MenuItem("Tools/BusMystery/Setup Phase 4")]
        public static void SetupPhase4()
        {
            if (!File.Exists(BusScenePath))
            {
                Debug.LogError("Assets/Scenes/Bus.unity was not found. Run earlier setup phases before Setup Phase 4.");
                return;
            }

            var scene = EditorSceneManager.OpenScene(BusScenePath);
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Bus scene is missing a Canvas. Run Setup Phase 1 before Setup Phase 4.");
                return;
            }

            RemoveGeneratedObject(RootObjectName);

            var memoryUI = Object.FindFirstObjectByType<PassengerMemoryUI>(FindObjectsInactive.Include);
            var collectionManager = EnsureSharedCollectionManager(memoryUI);
            var font = LoadJapaneseFont();
            if (font == null)
            {
                Debug.LogError("Setup Phase 4 was stopped because the Japanese TMP font asset could not be loaded.");
                return;
            }

            ApplyJapaneseFontToScene(font);
            CreateNotebookUI(canvas.transform, collectionManager, font);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("BusMystery Phase 4 setup complete.");
        }

        private static WordCollectionManager EnsureSharedCollectionManager(PassengerMemoryUI memoryUI)
        {
            var manager = memoryUI != null ? memoryUI.CollectionManager : null;
            if (manager == null)
            {
                manager = Object.FindFirstObjectByType<WordCollectionManager>(FindObjectsInactive.Include);
            }

            if (manager == null)
            {
                manager = new GameObject("Word Collection Manager").AddComponent<WordCollectionManager>();
            }

            if (memoryUI != null && memoryUI.CollectionManager != manager)
            {
                SetObjectReference(memoryUI, "wordCollectionManager", manager);
            }

            return manager;
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

        private static void CreateNotebookUI(Transform canvasTransform, WordCollectionManager collectionManager, TMP_FontAsset font)
        {
            var root = new GameObject(RootObjectName, typeof(RectTransform));
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.SetParent(canvasTransform, false);
            SetRect(rootRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var notebook = root.AddComponent<WordNotebookUI>();

            var openButton = CreateButton("Open Word Notebook Button", root.transform, "推理ノート", new Color(0.72f, 0.58f, 0.36f), new Color(0.11f, 0.08f, 0.05f), font);
            SetRect(openButton.GetComponent<RectTransform>(), new Vector2(0.02f, 0.70f), new Vector2(0.16f, 0.77f), new Vector2(0f, 0.5f), Vector2.zero);
            UnityEventTools.AddPersistentListener(openButton.onClick, notebook.Open);

            var panel = CreateImage("Word Notebook Panel", root.transform, new Color(0.86f, 0.78f, 0.61f, 0.97f));
            SetRect(panel.rectTransform, new Vector2(0.18f, 0.12f), new Vector2(0.82f, 0.88f), new Vector2(0.5f, 0.5f), Vector2.zero);

            var title = CreateText("Title Label", panel.transform, "推理ノート", 34f, TextAlignmentOptions.Center, font);
            title.color = new Color(0.13f, 0.09f, 0.05f);
            SetRect(title.rectTransform, new Vector2(0.05f, 0.90f), new Vector2(0.95f, 0.98f), new Vector2(0.5f, 0.5f), Vector2.zero);

            var closeButton = CreateButton("Close Button", panel.transform, "閉じる", new Color(0.42f, 0.31f, 0.20f), Color.white, font);
            SetRect(closeButton.GetComponent<RectTransform>(), new Vector2(0.78f, 0.90f), new Vector2(0.96f, 0.98f), new Vector2(0.5f, 0.5f), Vector2.zero);
            UnityEventTools.AddPersistentListener(closeButton.onClick, notebook.Close);

            var scrollView = CreateImage("Card Scroll View", panel.transform, new Color(0.76f, 0.67f, 0.50f, 0.55f));
            SetRect(scrollView.rectTransform, new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.86f), new Vector2(0.5f, 0.5f), Vector2.zero);
            var scrollRect = scrollView.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            var viewport = CreateImage("Viewport", scrollView.transform, new Color(1f, 1f, 1f, 0f));
            SetRect(viewport.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero);
            viewport.raycastTarget = false;
            viewport.gameObject.AddComponent<RectMask2D>();

            var content = new GameObject("Content", typeof(RectTransform)).GetComponent<RectTransform>();
            content.SetParent(viewport.transform, false);
            SetRect(content, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(0f, 96f));

            var grid = content.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(210f, 64f);
            grid.spacing = new Vector2(14f, 14f);
            grid.padding = new RectOffset(18, 18, 18, 18);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.childAlignment = TextAnchor.UpperLeft;

            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewport.rectTransform;
            scrollRect.content = content;

            var cardTemplate = CreateImage("Notebook Word Card Template", content, new Color(0.96f, 0.90f, 0.76f));
            cardTemplate.raycastTarget = false;
            SetRect(cardTemplate.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), grid.cellSize);
            var cardLayout = cardTemplate.gameObject.AddComponent<LayoutElement>();
            cardLayout.preferredWidth = 210f;
            cardLayout.preferredHeight = 64f;

            var cardLabel = CreateText("Word Label", cardTemplate.transform, "単語", 22f, TextAlignmentOptions.Center, font);
            cardLabel.color = new Color(0.12f, 0.08f, 0.04f);
            SetRect(cardLabel.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero);

            var card = cardTemplate.gameObject.AddComponent<NotebookWordCardUI>();
            SetObjectReference(card, "wordLabel", cardLabel);
            SetObjectReference(card, "fontAsset", font);
            cardTemplate.gameObject.SetActive(false);

            SetObjectReference(notebook, "panelRoot", panel.gameObject);
            SetObjectReference(notebook, "collectionManager", collectionManager);
            SetObjectReference(notebook, "cardContainer", content);
            SetObjectReference(notebook, "cardPrefab", card);

            panel.gameObject.SetActive(false);
        }

        private static void ApplyJapaneseFontToScene(TMP_FontAsset font)
        {
            var texts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var text in texts)
            {
                text.font = font;
                EditorUtility.SetDirty(text);
            }

            var reservationUis = Object.FindObjectsByType<BusReservationUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var reservationUi in reservationUis)
            {
                SetObjectReference(reservationUi, "fontAsset", font);
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

        private static TMP_Text CreateText(string name, Transform parent, string text, float fontSize, TextAlignmentOptions alignment, TMP_FontAsset font)
        {
            var label = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI)).GetComponent<TMP_Text>();
            label.transform.SetParent(parent, false);
            label.text = text;
            label.font = font;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = Color.white;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.raycastTarget = false;
            EditorUtility.SetDirty(label);
            return label;
        }

        private static Button CreateButton(string name, Transform parent, string text, Color normalColor, Color textColor, TMP_FontAsset font)
        {
            var image = CreateImage(name, parent, normalColor);
            image.raycastTarget = true;

            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = normalColor * 1.12f;
            colors.pressedColor = normalColor * 0.86f;
            colors.selectedColor = normalColor * 1.05f;
            colors.disabledColor = new Color(0.25f, 0.22f, 0.18f, 0.55f);
            button.colors = colors;

            var label = CreateText("Label", image.transform, text, 22f, TextAlignmentOptions.Center, font);
            label.color = textColor;
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
