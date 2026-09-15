using BusMystery.Passengers;
using BusMystery.Words;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BusMystery.UI
{
    public class PassengerMemoryUI : MonoBehaviour
    {
        private const string HoverColor = "#FFD966";
        private const string CollectedColor = "#9FD7FF";

        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text speakerLabel;
        [SerializeField] private TMP_Text bodyLabel;
        [SerializeField] private WordCollectionManager wordCollectionManager;

        private PassengerMemoryData currentData;
        private int lineIndex;
        private int openedFrame = -1;
        private string hoveredWordId;

        public static bool IsAnyOpen { get; private set; }
        public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

        private void Awake()
        {
            if (panelRoot == null)
            {
                panelRoot = gameObject;
            }

            if (wordCollectionManager == null)
            {
                wordCollectionManager = FindFirstObjectByType<WordCollectionManager>();
            }

            Close();
        }

        private void Update()
        {
            if (!IsOpen)
            {
                return;
            }

            if (Time.frameCount == openedFrame)
            {
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.escapeKey.wasPressedThisFrame)
                {
                    Close();
                    return;
                }

                if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
                {
                    Advance();
                }
            }

            var mouse = Mouse.current;
            if (mouse != null)
            {
                var mousePosition = mouse.position.ReadValue();
                UpdateHoveredWord(mousePosition);

                if (mouse.leftButton.wasPressedThisFrame)
                {
                    if (TryCollectWordAt(mousePosition))
                    {
                        return;
                    }

                    Advance();
                }
            }
        }

        public void Open(PassengerMemoryData data)
        {
            if (data == null)
            {
                return;
            }

            currentData = data;
            lineIndex = 0;
            openedFrame = Time.frameCount;
            hoveredWordId = null;
            panelRoot.SetActive(true);
            IsAnyOpen = true;
            Refresh();
        }

        public void Advance()
        {
            if (currentData == null || currentData.MemoryLines == null || currentData.MemoryLines.Count == 0)
            {
                Close();
                return;
            }

            lineIndex++;
            if (lineIndex >= currentData.MemoryLines.Count)
            {
                Close();
                return;
            }

            Refresh();
        }

        public void Close()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            currentData = null;
            lineIndex = 0;
            openedFrame = -1;
            hoveredWordId = null;
            IsAnyOpen = false;
        }

        private void Refresh()
        {
            if (currentData == null)
            {
                return;
            }

            if (speakerLabel != null)
            {
                speakerLabel.text = currentData.DisplayName;
            }

            if (bodyLabel != null)
            {
                var hasLine = currentData.MemoryLines != null && lineIndex >= 0 && lineIndex < currentData.MemoryLines.Count;
                bodyLabel.text = hasLine ? BuildLinkedLine(currentData.MemoryLines[lineIndex], lineIndex, hoveredWordId) : string.Empty;
                bodyLabel.ForceMeshUpdate();
            }
        }

        private void UpdateHoveredWord(Vector2 screenPosition)
        {
            var word = FindWordAt(screenPosition);
            var nextHoveredWordId = word != null ? word.Id : null;
            if (hoveredWordId == nextHoveredWordId)
            {
                return;
            }

            hoveredWordId = nextHoveredWordId;
            Refresh();
        }

        private bool TryCollectWordAt(Vector2 screenPosition)
        {
            var word = FindWordAt(screenPosition);
            if (word == null)
            {
                return false;
            }

            wordCollectionManager?.Collect(word);
            Refresh();
            return true;
        }

        private CollectibleWordData FindWordAt(Vector2 screenPosition)
        {
            if (bodyLabel == null)
            {
                return null;
            }

            var linkIndex = TMP_TextUtilities.FindIntersectingLink(bodyLabel, screenPosition, null);
            if (linkIndex < 0 || linkIndex >= bodyLabel.textInfo.linkCount)
            {
                return null;
            }

            var linkId = bodyLabel.textInfo.linkInfo[linkIndex].GetLinkID();
            return FindWordById(linkId);
        }

        private CollectibleWordData FindWordById(string wordId)
        {
            if (currentData == null || string.IsNullOrEmpty(wordId))
            {
                return null;
            }

            foreach (var span in currentData.CollectibleWordSpans)
            {
                if (span.LineIndex == lineIndex && span.WordData != null && span.WordData.Id == wordId)
                {
                    return span.WordData;
                }
            }

            return null;
        }

        private string BuildLinkedLine(string source, int sourceLineIndex, string highlightedWordId)
        {
            if (currentData == null || string.IsNullOrEmpty(source))
            {
                return source;
            }

            var result = source;
            var spans = new System.Collections.Generic.List<(int Start, int Length, CollectibleWordData Word)>();
            foreach (var span in currentData.CollectibleWordSpans)
            {
                if (span.LineIndex != sourceLineIndex || span.WordData == null || string.IsNullOrEmpty(span.TargetText))
                {
                    continue;
                }

                var start = FindOccurrence(source, span.TargetText, span.OccurrenceIndex);
                if (start >= 0)
                {
                    spans.Add((start, span.TargetText.Length, span.WordData));
                }
            }

            spans.Sort((left, right) => right.Start.CompareTo(left.Start));
            foreach (var span in spans)
            {
                var wordText = result.Substring(span.Start, span.Length);
                var isHighlighted = span.Word.Id == highlightedWordId;
                var isCollected = wordCollectionManager != null && wordCollectionManager.IsCollected(span.Word);
                var color = isHighlighted && !isCollected ? HoverColor : isCollected ? CollectedColor : null;
                var styledText = color != null ? $"<color={color}>{wordText}</color>" : wordText;

                if (isHighlighted)
                {
                    styledText = $"<u>{styledText}</u>";
                }

                var linkedText = $"<link=\"{span.Word.Id}\">{styledText}</link>";

                result = result.Remove(span.Start, span.Length).Insert(span.Start, linkedText);
            }

            return result;
        }

        private static int FindOccurrence(string source, string target, int occurrenceIndex)
        {
            var startIndex = 0;
            for (var i = 0; i <= occurrenceIndex; i++)
            {
                var found = source.IndexOf(target, startIndex, System.StringComparison.Ordinal);
                if (found < 0)
                {
                    return -1;
                }

                if (i == occurrenceIndex)
                {
                    return found;
                }

                startIndex = found + target.Length;
            }

            return -1;
        }
    }
}
