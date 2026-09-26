using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace BusMystery.UI
{
    public class WordNotebookCategoryDropZone : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private WordNotebookCategory category;
        [SerializeField] private RectTransform cardContainer;
        [SerializeField] private Image highlightImage;
        [SerializeField] private TMP_Text titleLabel;

        private WordNotebookUI notebookUI;
        private Color normalColor;
        private Color highlightedColor;
        private bool hasColors;
        private string baseTitle;

        public WordNotebookCategory Category => category;
        public RectTransform CardContainer => cardContainer;

        private void Awake()
        {
            CaptureColors();
        }

        public void Initialize(WordNotebookUI owner)
        {
            notebookUI = owner;
            CaptureColors();
            CaptureTitle();
            SetHighlighted(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            notebookUI?.SetHoveredDropZone(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            notebookUI?.ClearHoveredDropZone(this);
        }

        public void SetHighlighted(bool highlighted)
        {
            CaptureColors();
            if (highlightImage == null)
            {
                return;
            }

            highlightImage.color = highlighted ? highlightedColor : normalColor;
        }

        public void SetCount(int count)
        {
            CaptureTitle();
            if (titleLabel != null)
            {
                titleLabel.text = $"{baseTitle} ({count})";
            }
        }

        private void CaptureColors()
        {
            if (hasColors || highlightImage == null)
            {
                return;
            }

            normalColor = highlightImage.color;
            highlightedColor = new Color(Mathf.Min(1f, normalColor.r + 0.08f), Mathf.Min(1f, normalColor.g + 0.08f), Mathf.Min(1f, normalColor.b + 0.08f), Mathf.Min(1f, normalColor.a + 0.18f));
            hasColors = true;
        }

        private void CaptureTitle()
        {
            if (!string.IsNullOrEmpty(baseTitle) || titleLabel == null)
            {
                return;
            }

            var currentText = titleLabel.text;
            var countStart = currentText.LastIndexOf(" (", System.StringComparison.Ordinal);
            baseTitle = countStart > 0 ? currentText.Substring(0, countStart) : currentText;
        }
    }
}
