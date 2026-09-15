using System.IO;
using BusMystery.Passengers;
using BusMystery.UI;
using BusMystery.Words;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace BusMystery.Editor
{
    public static class BusMysteryPhase3Setup
    {
        private const string BusScenePath = "Assets/Scenes/Bus.unity";
        private const string PassengerDataFolder = "Assets/Data/Passengers";
        private const string WordDataFolder = "Assets/Data/Words";
        private const string JapaneseFontPath = "Assets/Fonts/NotoSansJP-Regular SDF.asset";

        [MenuItem("Tools/BusMystery/Setup Phase 3")]
        public static void SetupPhase3()
        {
            EnsureFolders();
            var station = EnsureWord("station", "駅", CollectibleWordType.Supporting);
            var motherCar = EnsureWord("mother_car", "母さんの車", CollectibleWordType.Supporting);
            var smartphone = EnsureWord("smartphone", "スマホ", CollectibleWordType.Supporting);
            var whiteCar = EnsureWord("white_car", "白い車", CollectibleWordType.Supporting);
            var mama = EnsureWord("mama", "ママ", CollectibleWordType.Supporting);
            var crashSound = EnsureWord("crash_sound", "衝突音", CollectibleWordType.Supporting);
            var tireSound = EnsureWord("tire_sound", "タイヤの音", CollectibleWordType.Supporting);

            SetPassengerSpans("young_man", new[]
            {
                Span(0, "駅", 0, station),
                Span(1, "母さんの車", 0, motherCar),
                Span(3, "スマホ", 0, smartphone)
            });
            SetPassengerSpans("elementary_brother", new[]
            {
                Span(2, "白い車", 0, whiteCar),
                Span(3, "ママ", 0, mama)
            });
            SetPassengerSpans("commuting_woman", new[]
            {
                Span(3, "衝突音", 0, crashSound),
                Span(4, "タイヤの音", 0, tireSound)
            });
            AssetDatabase.SaveAssets();

            if (!File.Exists(BusScenePath))
            {
                Debug.LogError("Assets/Scenes/Bus.unity was not found. Run Tools > BusMystery > Setup Phase 1 and Setup Phase 2 before Setup Phase 3.");
                return;
            }

            var scene = EditorSceneManager.OpenScene(BusScenePath);
            var canvas = Object.FindFirstObjectByType<Canvas>();
            var memoryUI = Object.FindFirstObjectByType<PassengerMemoryUI>(FindObjectsInactive.Include);
            if (canvas == null || memoryUI == null)
            {
                Debug.LogError("Bus scene is missing Phase 2 Canvas or PassengerMemoryUI. Run Setup Phase 2, then Setup Phase 3 again.");
                return;
            }

            var collectionManager = EnsureCollectionManager();
            SetObjectReference(memoryUI, "wordCollectionManager", collectionManager);

            RemoveGeneratedObject("Word Card Notification");
            CreateNotificationUI(canvas.transform, LoadJapaneseFont());

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("BusMystery Phase 3 setup complete.");
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
            {
                Directory.CreateDirectory("Assets/Data");
            }

            if (!AssetDatabase.IsValidFolder(WordDataFolder))
            {
                Directory.CreateDirectory(WordDataFolder);
            }
        }

        private static CollectibleWordData EnsureWord(string id, string displayText, CollectibleWordType internalType)
        {
            var path = $"{WordDataFolder}/{id}.asset";
            var word = AssetDatabase.LoadAssetAtPath<CollectibleWordData>(path);
            if (word == null)
            {
                word = ScriptableObject.CreateInstance<CollectibleWordData>();
                AssetDatabase.CreateAsset(word, path);
            }

            var serialized = new SerializedObject(word);
            serialized.FindProperty("id").stringValue = id;
            serialized.FindProperty("displayText").stringValue = displayText;
            serialized.FindProperty("internalType").enumValueIndex = (int)internalType;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(word);
            return word;
        }

        private static WordSpanSeed Span(int lineIndex, string targetText, int occurrenceIndex, CollectibleWordData word)
        {
            return new WordSpanSeed(lineIndex, targetText, occurrenceIndex, word);
        }

        private static void SetPassengerSpans(string passengerId, WordSpanSeed[] spans)
        {
            var path = $"{PassengerDataFolder}/{passengerId}.asset";
            var passenger = AssetDatabase.LoadAssetAtPath<PassengerMemoryData>(path);
            if (passenger == null)
            {
                Debug.LogError($"Passenger data was not found: {path}. Run Setup Phase 2 before Setup Phase 3.");
                return;
            }

            var serialized = new SerializedObject(passenger);
            var property = serialized.FindProperty("collectibleWordSpans");
            if (property == null)
            {
                Debug.LogError($"collectibleWordSpans property was not found on {passenger.name}.");
                return;
            }

            property.arraySize = spans.Length;
            for (var i = 0; i < spans.Length; i++)
            {
                var element = property.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("lineIndex").intValue = spans[i].LineIndex;
                element.FindPropertyRelative("targetText").stringValue = spans[i].TargetText;
                element.FindPropertyRelative("occurrenceIndex").intValue = spans[i].OccurrenceIndex;
                element.FindPropertyRelative("wordData").objectReferenceValue = spans[i].Word;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(passenger);
        }

        private static WordCollectionManager EnsureCollectionManager()
        {
            var manager = Object.FindFirstObjectByType<WordCollectionManager>();
            if (manager != null)
            {
                return manager;
            }

            return new GameObject("Word Collection Manager").AddComponent<WordCollectionManager>();
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

        private static void CreateNotificationUI(Transform canvasTransform, TMP_FontAsset font)
        {
            var panel = CreateImage("Word Card Notification", canvasTransform, new Color(0.06f, 0.065f, 0.075f, 0.94f));
            SetRect(panel.rectTransform, new Vector2(0.34f, 0.76f), new Vector2(0.66f, 0.84f), new Vector2(0.5f, 0.5f), Vector2.zero);
            panel.gameObject.AddComponent<CanvasGroup>();

            var label = CreateText("Message Label", panel.transform, "", 22f, TextAlignmentOptions.Center, font);
            SetRect(label.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero);

            var notification = panel.gameObject.AddComponent<WordCardNotificationUI>();
            SetObjectReference(notification, "panelRoot", panel.gameObject);
            SetObjectReference(notification, "messageLabel", label);
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
            return label;
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

        private readonly struct WordSpanSeed
        {
            public WordSpanSeed(int lineIndex, string targetText, int occurrenceIndex, CollectibleWordData word)
            {
                LineIndex = lineIndex;
                TargetText = targetText;
                OccurrenceIndex = occurrenceIndex;
                Word = word;
            }

            public int LineIndex { get; }
            public string TargetText { get; }
            public int OccurrenceIndex { get; }
            public CollectibleWordData Word { get; }
        }
    }
}
