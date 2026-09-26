using System.Collections.Generic;
using BusMystery.Words;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BusMystery.UI
{
    public class WordNotebookUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private WordCollectionManager collectionManager;
        [SerializeField] private GameObject listPageRoot;
        [SerializeField] private CanvasGroup listPageCanvasGroup;
        [SerializeField] private GameObject categoryPageRoot;
        [SerializeField] private RectTransform cardContainer;
        [SerializeField] private NotebookWordCardUI cardPrefab;
        [SerializeField] private RectTransform dragLayer;
        [SerializeField] private WordNotebookCategoryDropZone[] categoryDropZones;

        private readonly List<NotebookWordCardUI> cards = new();
        private readonly Dictionary<string, WordNotebookCategory> classifiedWords = new();
        private readonly Dictionary<WordNotebookCategory, int> categoryCounts = new();
        private static int closedFrame = -1;
        private bool isSubscribed;
        private NotebookWordCardUI dragCard;
        private NotebookWordCardUI dragSourceCard;
        private WordNotebookCategoryDropZone hoveredDropZone;

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
                return;
            }

            ResolveCollectionManager();
            panelRoot.SetActive(true);
            IsAnyOpen = true;
            ShowListPage();
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
            ClearDragState();
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

        public void ShowListPage()
        {
            if (listPageRoot != null)
            {
                listPageRoot.SetActive(true);
            }

            SetListPageVisible(true);

            if (categoryPageRoot != null)
            {
                categoryPageRoot.SetActive(false);
            }

            ClearHoveredDropZone(hoveredDropZone);
        }

        public void ShowCategoryPage()
        {
            ShowCategoryPage(false, false);
        }

        private void ShowCategoryPage(bool keepListPageActive, bool hideListPageVisually)
        {
            if (listPageRoot != null)
            {
                listPageRoot.SetActive(keepListPageActive);
            }

            SetListPageVisible(!hideListPageVisually);

            if (categoryPageRoot != null)
            {
                categoryPageRoot.SetActive(true);
                categoryPageRoot.transform.SetAsLastSibling();
            }

            if (dragLayer != null)
            {
                dragLayer.SetAsLastSibling();
            }
        }

        public void BeginCardDrag(NotebookWordCardUI sourceCard, PointerEventData eventData)
        {
            if (sourceCard == null || sourceCard.WordData == null)
            {
                return;
            }

            ShowCategoryPage(true, true);
            dragSourceCard = sourceCard;
            dragCard = Instantiate(cardPrefab, dragLayer != null ? dragLayer : panelRoot.transform);
            dragCard.gameObject.SetActive(true);
            dragCard.Initialize(sourceCard.WordData);
            PrepareDragCardRect(dragCard);

            var canvasGroup = dragCard.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = dragCard.gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.92f;
            UpdateCardDrag(eventData);
        }

        public void UpdateCardDrag(PointerEventData eventData)
        {
            if (dragCard == null || eventData == null)
            {
                return;
            }

            var dragRect = dragCard.GetComponent<RectTransform>();
            var parentRect = dragRect.parent as RectTransform;
            if (parentRect == null)
            {
                return;
            }

            var camera = eventData != null ? eventData.pressEventCamera : null;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, camera, out var localPoint))
            {
                dragRect.anchoredPosition = localPoint;
            }

            UpdateHoveredDropZone(eventData.position, camera);
        }

        public void EndCardDrag(PointerEventData eventData)
        {
            if (dragSourceCard != null && dragSourceCard.WordData != null && hoveredDropZone != null)
            {
                classifiedWords[dragSourceCard.WordData.Id] = hoveredDropZone.Category;
            }

            ClearDragState();
            ShowCategoryPage(false, false);
            Refresh();
        }

        public void SetHoveredDropZone(WordNotebookCategoryDropZone dropZone)
        {
            if (dragCard == null || dropZone == hoveredDropZone)
            {
                return;
            }

            ClearHoveredDropZone(hoveredDropZone);
            hoveredDropZone = dropZone;
            hoveredDropZone?.SetHighlighted(true);
        }

        public void ClearHoveredDropZone(WordNotebookCategoryDropZone dropZone)
        {
            if (dropZone == null || hoveredDropZone != dropZone)
            {
                return;
            }

            hoveredDropZone.SetHighlighted(false);
            hoveredDropZone = null;
        }

        private void UpdateHoveredDropZone(Vector2 screenPosition, Camera eventCamera)
        {
            if (categoryDropZones == null)
            {
                ClearHoveredDropZone(hoveredDropZone);
                return;
            }

            foreach (var dropZone in categoryDropZones)
            {
                if (dropZone == null)
                {
                    continue;
                }

                var dropZoneRect = dropZone.transform as RectTransform;
                if (dropZoneRect == null)
                {
                    continue;
                }

                if (RectTransformUtility.RectangleContainsScreenPoint(dropZoneRect, screenPosition, eventCamera))
                {
                    SetHoveredDropZone(dropZone);
                    return;
                }
            }

            ClearHoveredDropZone(hoveredDropZone);
        }

        private void Refresh()
        {
            ResolveCollectionManager();
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
            categoryCounts.Clear();

            foreach (var word in collectionManager.CollectedWords)
            {
                if (word == null)
                {
                    continue;
                }

                var isClassified = classifiedWords.TryGetValue(word.Id, out var category);
                if (isClassified)
                {
                    categoryCounts[category] = categoryCounts.TryGetValue(category, out var count) ? count + 1 : 1;
                }

                var targetContainer = GetCardContainer(word);
                if (targetContainer == null)
                {
                    continue;
                }

                var card = Instantiate(cardPrefab, targetContainer);
                card.gameObject.SetActive(true);
                card.Initialize(word, this, isClassified);
                cards.Add(card);
            }

            UpdateCategoryCounts();
            LayoutRebuilder.ForceRebuildLayoutImmediate(cardContainer);
            if (categoryDropZones != null)
            {
                foreach (var dropZone in categoryDropZones)
                {
                    if (dropZone != null && dropZone.CardContainer != null)
                    {
                        LayoutRebuilder.ForceRebuildLayoutImmediate(dropZone.CardContainer);
                        var viewport = dropZone.CardContainer.parent as RectTransform;
                        if (viewport != null)
                        {
                            LayoutRebuilder.ForceRebuildLayoutImmediate(viewport);
                        }
                    }
                }
            }
        }

        private RectTransform GetCardContainer(CollectibleWordData word)
        {
            if (word == null || string.IsNullOrEmpty(word.Id))
            {
                return null;
            }

            if (!classifiedWords.TryGetValue(word.Id, out var category))
            {
                return cardContainer;
            }

            if (categoryDropZones == null)
            {
                return null;
            }

            foreach (var dropZone in categoryDropZones)
            {
                if (dropZone != null && dropZone.Category == category)
                {
                    return dropZone.CardContainer;
                }
            }

            return null;
        }

        private void UpdateCategoryCounts()
        {
            if (categoryDropZones == null)
            {
                return;
            }

            foreach (var dropZone in categoryDropZones)
            {
                if (dropZone == null)
                {
                    continue;
                }

                categoryCounts.TryGetValue(dropZone.Category, out var count);
                dropZone.SetCount(count);
            }
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

            InitializeDropZones();
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

        private void SetListPageVisible(bool visible)
        {
            if (listPageCanvasGroup == null && listPageRoot != null)
            {
                listPageCanvasGroup = listPageRoot.GetComponent<CanvasGroup>();
            }

            if (listPageCanvasGroup == null)
            {
                return;
            }

            listPageCanvasGroup.alpha = visible ? 1f : 0f;
            listPageCanvasGroup.interactable = visible;
            listPageCanvasGroup.blocksRaycasts = visible;
        }

        private static void PrepareDragCardRect(NotebookWordCardUI card)
        {
            if (card == null)
            {
                return;
            }

            var rect = card.GetComponent<RectTransform>();
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(300f, 72f);
            rect.localScale = Vector3.one;
        }

        private void InitializeDropZones()
        {
            if (categoryDropZones == null)
            {
                return;
            }

            foreach (var dropZone in categoryDropZones)
            {
                dropZone?.Initialize(this);
            }
        }

        private void ClearDragState()
        {
            ClearHoveredDropZone(hoveredDropZone);
            if (dragCard != null)
            {
                Destroy(dragCard.gameObject);
            }

            dragCard = null;
            dragSourceCard = null;
        }
    }
}
