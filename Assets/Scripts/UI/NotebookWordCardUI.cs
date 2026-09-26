using BusMystery.Words;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BusMystery.UI
{
    public class NotebookWordCardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private TMP_Text wordLabel;
        [SerializeField] private TMP_FontAsset fontAsset;

        private WordNotebookUI notebookUI;
        private CollectibleWordData wordData;

        public CollectibleWordData WordData => wordData;

        public void Initialize(CollectibleWordData word, WordNotebookUI owner = null)
        {
            wordData = word;
            notebookUI = owner;

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
