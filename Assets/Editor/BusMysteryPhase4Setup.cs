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

            var listPage = new GameObject("Word List Page", typeof(RectTransform)).GetComponent<RectTransform>();
            listPage.SetParent(panel.transform, false);
            SetRect(listPage, new Vector2(0.04f, 0.07f), new Vector2(0.96f, 0.88f), new Vector2(0.5f, 0.5f), Vector2.zero);
            var listPageCanvasGroup = listPage.gameObject.AddComponent<CanvasGroup>();

            var categoryButton = CreateButton("Open Category Page Button", listPage, "分類ページへ", new Color(0.42f, 0.31f, 0.20f), Color.white, font);
            SetRect(categoryButton.GetComponent<RectTransform>(), new Vector2(0.79f, 0.91f), new Vector2(0.99f, 0.99f), new Vector2(0.5f, 0.5f), Vector2.zero);
            UnityEventTools.AddPersistentListener(categoryButton.onClick, notebook.ShowCategoryPage);

            var scrollView = CreateImage("Card Scroll View", listPage, new Color(0.76f, 0.67f, 0.50f, 0.55f));
            SetRect(scrollView.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0.89f), new Vector2(0.5f, 0.5f), Vector2.zero);
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
            grid.cellSize = new Vector2(300f, 72f);
            grid.spacing = new Vector2(24f, 16f);
            grid.padding = new RectOffset(0, 0, 24, 24);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.childAlignment = TextAnchor.UpperCenter;

            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewport.rectTransform;
            scrollRect.content = content;

            var cardTemplate = CreateImage("Notebook Word Card Template", content, new Color(0.96f, 0.90f, 0.76f));
            cardTemplate.raycastTarget = true;
            SetRect(cardTemplate.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), grid.cellSize);
            var cardLayout = cardTemplate.gameObject.AddComponent<LayoutElement>();
            cardLayout.preferredWidth = grid.cellSize.x;
            cardLayout.preferredHeight = grid.cellSize.y;

            var cardLabel = CreateText("Word Label", cardTemplate.transform, "単語", 22f, TextAlignmentOptions.Center, font);
            cardLabel.color = new Color(0.12f, 0.08f, 0.04f);
            SetRect(cardLabel.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero);

            var card = cardTemplate.gameObject.AddComponent<NotebookWordCardUI>();
            SetObjectReference(card, "wordLabel", cardLabel);
            SetObjectReference(card, "fontAsset", font);
            cardTemplate.gameObject.SetActive(false);

            var categoryPage = new GameObject("Word Category Page", typeof(RectTransform)).GetComponent<RectTransform>();
            categoryPage.SetParent(panel.transform, false);
            SetRect(categoryPage, new Vector2(0.04f, 0.07f), new Vector2(0.96f, 0.88f), new Vector2(0.5f, 0.5f), Vector2.zero);

            var backButton = CreateButton("Back To Word List Button", categoryPage, "単語一覧へ", new Color(0.42f, 0.31f, 0.20f), Color.white, font);
            SetRect(backButton.GetComponent<RectTransform>(), new Vector2(0.01f, 0.91f), new Vector2(0.20f, 0.99f), new Vector2(0.5f, 0.5f), Vector2.zero);
            UnityEventTools.AddPersistentListener(backButton.onClick, notebook.ShowListPage);

            var categoryTitle = CreateText("Category Page Hint", categoryPage, "カードを分類してください", 22f, TextAlignmentOptions.Center, font);
            categoryTitle.color = new Color(0.13f, 0.09f, 0.05f);
            SetRect(categoryTitle.rectTransform, new Vector2(0.22f, 0.91f), new Vector2(0.78f, 0.99f), new Vector2(0.5f, 0.5f), Vector2.zero);

            var dropZones = new[]
            {
                CreateCategoryDropZone(categoryPage, WordNotebookCategory.Cause, "Cause", new Vector2(0.00f, 0.62f), new Vector2(0.48f, 0.89f), font),
                CreateCategoryDropZone(categoryPage, WordNotebookCategory.Action, "Action", new Vector2(0.52f, 0.62f), new Vector2(1.00f, 0.89f), font),
                CreateCategoryDropZone(categoryPage, WordNotebookCategory.Event, "Event", new Vector2(0.00f, 0.32f), new Vector2(0.48f, 0.59f), font),
                CreateCategoryDropZone(categoryPage, WordNotebookCategory.PersonRole, "Person / Role", new Vector2(0.52f, 0.32f), new Vector2(1.00f, 0.59f), font),
                CreateCategoryDropZone(categoryPage, WordNotebookCategory.Place, "Place", new Vector2(0.00f, 0.02f), new Vector2(0.48f, 0.29f), font),
                CreateCategoryDropZone(categoryPage, WordNotebookCategory.Relation, "Relation", new Vector2(0.52f, 0.02f), new Vector2(1.00f, 0.29f), font)
            };

            categoryPage.gameObject.SetActive(false);

            var dragLayer = new GameObject("Drag Layer", typeof(RectTransform)).GetComponent<RectTransform>();
            dragLayer.SetParent(panel.transform, false);
            SetRect(dragLayer, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero);
            dragLayer.SetAsLastSibling();

            SetObjectReference(notebook, "panelRoot", panel.gameObject);
            SetObjectReference(notebook, "collectionManager", collectionManager);
            SetObjectReference(notebook, "listPageRoot", listPage.gameObject);
            SetObjectReference(notebook, "listPageCanvasGroup", listPageCanvasGroup);
            SetObjectReference(notebook, "categoryPageRoot", categoryPage.gameObject);
            SetObjectReference(notebook, "cardContainer", content);
            SetObjectReference(notebook, "cardPrefab", card);
            SetObjectReference(notebook, "dragLayer", dragLayer);
            SetObjectArrayReference(notebook, "categoryDropZones", dropZones);

            panel.gameObject.SetActive(false);
        }

        private static WordNotebookCategoryDropZone CreateCategoryDropZone(Transform parent, WordNotebookCategory category, string title, Vector2 anchorMin, Vector2 anchorMax, TMP_FontAsset font)
        {
            var zoneImage = CreateImage($"{title} Drop Zone", parent, new Color(0.72f, 0.63f, 0.46f, 0.62f));
            zoneImage.raycastTarget = true;
            SetRect(zoneImage.rectTransform, anchorMin, anchorMax, new Vector2(0.5f, 0.5f), Vector2.zero);

            var label = CreateText("Category Label", zoneImage.transform, title, 20f, TextAlignmentOptions.Left, font);
            label.color = new Color(0.12f, 0.08f, 0.04f);
            SetRect(label.rectTransform, new Vector2(0.05f, 0.78f), new Vector2(0.95f, 0.96f), new Vector2(0.5f, 0.5f), Vector2.zero);

            var scrollView = CreateImage("Category Scroll View", zoneImage.transform, new Color(1f, 1f, 1f, 0f));
            scrollView.raycastTarget = false;
            SetRect(scrollView.rectTransform, new Vector2(0.04f, 0.07f), new Vector2(0.96f, 0.74f), new Vector2(0.5f, 0.5f), Vector2.zero);
            var scrollRect = scrollView.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            var viewport = CreateImage("Viewport", scrollView.transform, new Color(1f, 1f, 1f, 0f));
            viewport.raycastTarget = false;
            SetRect(viewport.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero);
            viewport.gameObject.AddComponent<RectMask2D>();

            var content = new GameObject("Card Container", typeof(RectTransform)).GetComponent<RectTransform>();
            content.SetParent(viewport.transform, false);
            SetRect(content, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 72f));

            var grid = content.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(260f, 58f);
            grid.spacing = new Vector2(10f, 8f);
            grid.padding = new RectOffset(0, 0, 0, 8);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 1;
            grid.childAlignment = TextAnchor.UpperCenter;

            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewport.rectTransform;
            scrollRect.content = content;

            var dropZone = zoneImage.gameObject.AddComponent<WordNotebookCategoryDropZone>();
            SetEnumValue(dropZone, "category", (int)category);
            SetObjectReference(dropZone, "cardContainer", content);
            SetObjectReference(dropZone, "highlightImage", zoneImage);
            return dropZone;
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

        private static void SetObjectArrayReference(Object target, string propertyName, Object[] references)
        {
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(propertyName);
            if (property == null || !property.isArray)
            {
                Debug.LogError($"Serialized array property not found: {target.name}.{propertyName}");
                return;
            }

            property.arraySize = references.Length;
            for (var i = 0; i < references.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = references[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetEnumValue(Object target, string propertyName, int enumValueIndex)
        {
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogError($"Serialized enum property not found: {target.name}.{propertyName}");
                return;
            }

            property.enumValueIndex = enumValueIndex;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }
    }
}
