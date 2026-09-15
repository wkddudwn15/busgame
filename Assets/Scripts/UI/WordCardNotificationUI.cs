using System.Collections;
using BusMystery.Words;
using TMPro;
using UnityEngine;

namespace BusMystery.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class WordCardNotificationUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text messageLabel;
        [SerializeField, Min(0.1f)] private float visibleSeconds = 1.6f;

        private CanvasGroup canvasGroup;
        private Coroutine hideRoutine;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (panelRoot == null)
            {
                panelRoot = gameObject;
            }

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            Hide();
        }

        private void OnEnable()
        {
            var collectionManager = FindFirstObjectByType<WordCollectionManager>();
            if (collectionManager != null)
            {
                collectionManager.WordCollected += HandleWordCollected;
            }
        }

        private void OnDisable()
        {
            var collectionManager = FindFirstObjectByType<WordCollectionManager>();
            if (collectionManager != null)
            {
                collectionManager.WordCollected -= HandleWordCollected;
            }
        }

        private void HandleWordCollected(CollectibleWordData word)
        {
            if (messageLabel != null)
            {
                messageLabel.text = $"単語カードを取得：{word.DisplayText}";
            }

            Show();

            if (hideRoutine != null)
            {
                StopCoroutine(hideRoutine);
            }

            hideRoutine = StartCoroutine(HideAfterDelay());
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSecondsRealtime(visibleSeconds);
            Hide();
            hideRoutine = null;
        }

        private void Show()
        {
            canvasGroup.alpha = 1f;
        }

        private void Hide()
        {
            canvasGroup.alpha = 0f;
        }
    }
}
