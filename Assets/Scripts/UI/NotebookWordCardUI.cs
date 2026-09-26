using BusMystery.Words;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BusMystery.UI
{
    public class NotebookWordCardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private static readonly Vector2 DefaultSize = new Vector2(300f, 72f);
        private static readonly Vector2 CompactSize = new Vector2(210f, 42f);
        private const float DefaultFontSize = 22f;
        private const float CompactFontSize = 17f;

        [SerializeField] private TMP_Text wordLabel;
        [SerializeField] private TMP_FontAsset fontAsset;

        private WordNotebookUI notebookUI;
        private CollectibleWordData wordData;

        public CollectibleWordData WordData => wordData;

        public void Initialize(CollectibleWordData word, WordNotebookUI owner = null, bool compact = false)
        {
            wordData = word;
            notebookUI = owner;
            ApplyLayout(compact);

            if (wordLabel == null)
            {
                wordLabel = GetComponentInChildren<TMP_Text>(true);
            }

            if (wordLabel != null)
            {
                if (fontAsset != null)
                {
                    wordLabel.font = fontAsset;
                }

                wordLabel.text = word != null ? word.DisplayText : string.Empty;
                wordLabel.fontSize = compact ? CompactFontSize : DefaultFontSize;
            }
        }

        private void ApplyLayout(bool compact)
        {
            var size = compact ? CompactSize : DefaultSize;
            var rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = size;
            }

            var layout = GetComponent<LayoutElement>();
            if (layout != null)
            {
                layout.preferredWidth = size.x;
                layout.preferredHeight = size.y;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            notebookUI?.BeginCardDrag(this, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            notebookUI?.UpdateCardDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            notebookUI?.EndCardDrag(eventData);
        }
    }
}
