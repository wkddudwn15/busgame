using BusMystery.Bus;
using BusMystery.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BusMystery.Passengers
{
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(CanvasGroup))]
    public class PassengerHotspot : MonoBehaviour
    {
        [SerializeField] private PassengerMemoryData passengerData;
        [SerializeField] private BusViewController viewController;
        [SerializeField] private PassengerMemoryUI memoryUI;
        [SerializeField] private TMP_Text label;

        private Button button;
        private CanvasGroup canvasGroup;

        public PassengerMemoryData PassengerData => passengerData;

        private void Awake()
        {
            button = GetComponent<Button>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (viewController == null)
            {
                viewController = FindFirstObjectByType<BusViewController>();
            }

            if (memoryUI == null)
            {
                memoryUI = FindFirstObjectByType<PassengerMemoryUI>(FindObjectsInactive.Include);
            }

            if (label == null)
            {
                label = GetComponentInChildren<TMP_Text>(true);
            }
        }

        private void OnEnable()
        {
            if (button != null)
            {
                button.onClick.AddListener(OpenMemory);
            }

            if (viewController != null)
            {
                viewController.ViewChanged += HandleViewChanged;
            }
        }

        private void OnDisable()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OpenMemory);
            }

            if (viewController != null)
            {
                viewController.ViewChanged -= HandleViewChanged;
            }
        }

        private void Start()
        {
            if (label != null && passengerData != null)
            {
                label.text = passengerData.DisplayName;
            }

            RefreshVisibility();
        }

        public void Initialize(PassengerMemoryData data, BusViewController busViewController, PassengerMemoryUI passengerMemoryUI)
        {
            passengerData = data;
            viewController = busViewController;
            memoryUI = passengerMemoryUI;

            if (label == null)
            {
                label = GetComponentInChildren<TMP_Text>(true);
            }

            if (label != null && passengerData != null)
            {
                label.text = passengerData.DisplayName;
            }

            RefreshVisibility();
        }

        private void OpenMemory()
        {
            if (passengerData != null && memoryUI != null)
            {
                memoryUI.Open(passengerData);
            }
        }

        private void HandleViewChanged(BusViewDirection view)
        {
            RefreshVisibility();
        }

        private void RefreshVisibility()
        {
            if (passengerData == null || viewController == null)
            {
                SetVisible(false);
                return;
            }

            SetVisible(viewController.CurrentView == passengerData.ViewDirection);
        }

        private void SetVisible(bool visible)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }
        }
    }
}
