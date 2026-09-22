using System.Collections.Generic;
using BusMystery.Words;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BusMystery.UI
{
    public class WordNotebookUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private WordCollectionManager collectionManager;
        [SerializeField] private RectTransform cardContainer;
        [SerializeField] private NotebookWordCardUI cardPrefab;

        private readonly List<NotebookWordCardUI> cards = new();
        private static int closedFrame = -1;

        public static bool IsAnyOpen { get; private set; }
        public static bool ShouldBlockBusInput => IsAnyOpen || closedFrame == Time.frameCount;
        public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

        private void Awake()
        {
            if (collectionManager == null)
            {
                collectionManager = FindFirstObjectByType<WordCollectionManager>();
            }

            Close();
        }

        private void OnEnable()
        {
            if (collectionManager != null)
            {
                collectionManager.WordCollected += HandleWordCollected;
            }
        }

        private void OnDisable()
        {
            if (collectionManager != null)
            {
                collectionManager.WordCollected -= HandleWordCollected;
            }

            if (IsOpen)
            {
                IsAnyOpen = false;
                closedFrame = Time.frameCount;
            }
        }

        private void Update()
        {
            if (!IsOpen)
            {
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                Close();
            }
        }

        public void Open()
        {
            if (panelRoot == null)
            {
                return;
            }

            panelRoot.SetActive(true);
            IsAnyOpen = true;
            Refresh();
        }

        public void Close()
        {
            var wasOpen = IsOpen;
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            IsAnyOpen = false;
            if (wasOpen)
            {
                closedFrame = Time.frameCount;
            }
        }

        private void HandleWordCollected(CollectibleWordData word)
        {
            if (IsOpen)
            {
                Refresh();
            }
        }

        private void Refresh()
        {
            if (cardContainer == null || cardPrefab == null || collectionManager == null)
            {
                return;
            }

            foreach (var card in cards)
            {
                if (card != null)
                {
                    Destroy(card.gameObject);
                }
            }

            cards.Clear();

            foreach (var word in collectionManager.CollectedWords)
            {
                var card = Instantiate(cardPrefab, cardContainer);
                card.gameObject.SetActive(true);
                card.Initialize(word);
                cards.Add(card);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(cardContainer);
        }
    }
}
