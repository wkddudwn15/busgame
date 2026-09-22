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
        private bool isSubscribed;

        public static bool IsAnyOpen { get; private set; }
        public static bool ShouldBlockBusInput => IsAnyOpen || closedFrame == Time.frameCount;
        public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

        private void Awake()
        {
            ResolveCollectionManager();
            Close();
        }

        private void OnEnable()
        {
            ResolveCollectionManager();
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();

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
                Debug.Log("[Phase4Debug] WordNotebookUI.Open panelRoot is null");
                return;
            }

            ResolveCollectionManager();
            Debug.Log($"[Phase4Debug] WordNotebookUI.Open collectionManagerNull={collectionManager == null} managerInstanceId={(collectionManager != null ? collectionManager.GetInstanceID().ToString() : "<null>")} collectedWordsCount={(collectionManager != null ? collectionManager.CollectedWords.Count.ToString() : "<null>")} cardContainerNull={cardContainer == null} cardPrefabNull={cardPrefab == null}");
            LogSceneManagers();
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
            ResolveCollectionManager();
            Debug.Log($"[Phase4Debug] WordNotebookUI.Refresh start collectionManagerNull={collectionManager == null} managerInstanceId={(collectionManager != null ? collectionManager.GetInstanceID().ToString() : "<null>")} collectedWordsCount={(collectionManager != null ? collectionManager.CollectedWords.Count.ToString() : "<null>")} cardContainerNull={cardContainer == null} cardPrefabNull={cardPrefab == null}");
            if (cardContainer == null || cardPrefab == null || collectionManager == null)
            {
                Debug.Log("[Phase4Debug] WordNotebookUI.Refresh stopped before card generation");
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

            var generatedCount = 0;
            foreach (var word in collectionManager.CollectedWords)
            {
                var card = Instantiate(cardPrefab, cardContainer);
                card.gameObject.SetActive(true);
                card.Initialize(word);
                cards.Add(card);
                generatedCount++;
                Debug.Log($"[Phase4Debug] WordNotebookUI.Refresh generatedCard wordId={(word != null ? word.Id : "<null>")} activeSelf={card.gameObject.activeSelf} activeInHierarchy={card.gameObject.activeInHierarchy}");
            }

            Debug.Log($"[Phase4Debug] WordNotebookUI.Refresh generatedCount={generatedCount}");
            LayoutRebuilder.ForceRebuildLayoutImmediate(cardContainer);
        }

        private void ResolveCollectionManager()
        {
            var memoryUI = FindFirstObjectByType<PassengerMemoryUI>(FindObjectsInactive.Include);
            if (memoryUI != null && memoryUI.CollectionManager != null)
            {
                SetCollectionManager(memoryUI.CollectionManager);
                return;
            }

            if (collectionManager != null)
            {
                return;
            }

            SetCollectionManager(FindFirstObjectByType<WordCollectionManager>(FindObjectsInactive.Include));
        }

        private void SetCollectionManager(WordCollectionManager manager)
        {
            if (collectionManager == manager)
            {
                return;
            }

            Unsubscribe();
            collectionManager = manager;
            Subscribe();
        }

        private void Subscribe()
        {
            if (isSubscribed || collectionManager == null || !isActiveAndEnabled)
            {
                return;
            }

            collectionManager.WordCollected += HandleWordCollected;
            isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!isSubscribed || collectionManager == null)
            {
                return;
            }

            collectionManager.WordCollected -= HandleWordCollected;
            isSubscribed = false;
        }

        private static void LogSceneManagers()
        {
            var managers = FindObjectsByType<WordCollectionManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Debug.Log($"[Phase4Debug] WordNotebookUI Scene WordCollectionManager count={managers.Length}");
            foreach (var manager in managers)
            {
                Debug.Log($"[Phase4Debug] WordNotebookUI Scene WordCollectionManager instanceId={manager.GetInstanceID()} name={manager.name} activeSelf={manager.gameObject.activeSelf} activeInHierarchy={manager.gameObject.activeInHierarchy} collectedWordsCount={manager.CollectedWords.Count}");
            }
        }
    }
}
