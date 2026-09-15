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
            var thisPlace = EnsureWord("this_place", "この場所", CollectibleWordType.Supporting);
            var girlfriend = EnsureWord("girlfriend", "彼女", CollectibleWordType.Character);
            var firstDate = EnsureWord("first_date", "初めてのデート", CollectibleWordType.Supporting);
            var leftHomeEarly = EnsureWord("left_home_early", "少し早く家を出た", CollectibleWordType.Supporting);
            var meetingPlace = EnsureWord("meeting_place", "待ち合わせ場所", CollectibleWordType.Supporting);
            var missingMemory = EnsureWord("missing_memory", "記憶がない", CollectibleWordType.Supporting);
            var station = EnsureWord("station", "駅", CollectibleWordType.Supporting);
            var favoriteSong = EnsureWord("favorite_song", "大好きな曲", CollectibleWordType.Supporting);
            var loudSound = EnsureWord("loud_sound", "大きな音", CollectibleWordType.Supporting);
            var thatCar = EnsureWord("that_car", "あの車", CollectibleWordType.Supporting);
            var sameColor = EnsureWord("same_color", "同じ色", CollectibleWordType.Supporting);
            var mama = EnsureWord("mama", "ママ", CollectibleWordType.Supporting);
            var shirakawaBridge = EnsureWord("shirakawa_bridge", "白川橋", CollectibleWordType.Misleading);
            var oneStationBefore = EnsureWord("one_station_before", "一つ手前の駅", CollectibleWordType.Supporting);
            var music = EnsureWord("music", "音楽", CollectibleWordType.Supporting);
            var bus = EnsureWord("bus", "バス", CollectibleWordType.Supporting);

            SetPassengerLines("young_man", new[]
            {
                "僕はなんでこの場所にいるんだろう。",
                "今日は彼女と初めてのデートだったんだ。",
                "だからいつもより少し早く家を出たんだ。",
                "そして、待ち合わせ場所で待っていたはずなのに。",
                "そこから記憶がないんだ"
            });
            SetPassengerLines("elementary_brother", new[]
            {
                "今日はママとお兄ちゃんと一緒におでかけなんだよ。",
                "僕の大好きな車に乗って！",
                "お兄ちゃんが駅に用事があるって言っていたから、ついてきたんだ。",
                "大好きな曲をかけて、お歌を歌いながら、ドーンって大きな音がしたの！",
                "あの車、ママの車と同じ色だったな。",
                "あれ、お兄ちゃんはいるけど、ママはどこにいるの？"
            });
            SetPassengerLines("commuting_woman", new[]
            {
                "なんか、バスっていいよね。",
                "音楽を聴きながら、無心で外の風景を見るのが好きなの。",
                "今日は白川橋駅に向かってたはず。",
                "いつも最寄りの駅より一つ手前の駅で降りて、そこからバスに乗り換えるんだ。",
                "……そういえば今日って、バスに乗ったんだっけ。"
            });

            SetPassengerSpans("young_man", new[]
            {
                Span(0, "この場所", 0, thisPlace),
                Span(1, "彼女", 0, girlfriend),
                Span(1, "初めてのデート", 0, firstDate),
                Span(2, "少し早く家を出た", 0, leftHomeEarly),
                Span(3, "待ち合わせ場所", 0, meetingPlace),
                Span(4, "記憶がない", 0, missingMemory)
            });
            SetPassengerSpans("elementary_brother", new[]
            {
                Span(0, "ママ", 0, mama),
                Span(2, "駅", 0, station),
                Span(3, "大好きな曲", 0, favoriteSong),
                Span(3, "大きな音", 0, loudSound),
                Span(4, "あの車", 0, thatCar),
                Span(4, "ママ", 0, mama),
                Span(4, "同じ色", 0, sameColor),
                Span(5, "ママ", 0, mama)
            });
            SetPassengerSpans("commuting_woman", new[]
            {
                Span(0, "バス", 0, bus),
                Span(1, "音楽", 0, music),
                Span(2, "白川橋", 0, shirakawaBridge),
                Span(3, "一つ手前の駅", 0, oneStationBefore),
                Span(3, "バス", 0, bus),
                Span(4, "バス", 0, bus)
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

        private static void SetPassengerLines(string passengerId, string[] lines)
        {
            var path = $"{PassengerDataFolder}/{passengerId}.asset";
            var passenger = AssetDatabase.LoadAssetAtPath<PassengerMemoryData>(path);
            if (passenger == null)
            {
                Debug.LogError($"Passenger data was not found: {path}. Run Setup Phase 2 before Setup Phase 3.");
                return;
            }

            var serialized = new SerializedObject(passenger);
            var memoryLines = serialized.FindProperty("memoryLines");
            if (memoryLines == null)
            {
                Debug.LogError($"memoryLines property was not found on {passenger.name}.");
                return;
            }

            memoryLines.arraySize = lines.Length;
            for (var i = 0; i < lines.Length; i++)
            {
                memoryLines.GetArrayElementAtIndex(i).stringValue = lines[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(passenger);
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
