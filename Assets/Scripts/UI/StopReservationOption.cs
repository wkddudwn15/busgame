using BusMystery.Bus;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BusMystery.UI
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(LayoutElement))]
    public class StopReservationOption : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text label;

        private int stopIndex;
        private StopReservationController reservationController;

        public void Initialize(int index, RouteStopDefinition stop, StopReservationController reservation)
        {
            stopIndex = index;
            reservationController = reservation;

            ResolveReferences();

            if (label != null)
            {
                label.text = $"{stop.Order}. {stop.DisplayName}";
            }

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(Reserve);
            }
        }

        public void Refresh(bool passed, bool reserved)
        {
            if (button != null)
            {
                button.interactable = !passed;
            }

            if (label != null)
            {
                label.color = passed ? new Color(0.55f, 0.55f, 0.55f) : reserved ? new Color(1f, 0.92f, 0.35f) : Color.white;
                label.fontStyle = reserved ? FontStyles.Bold : FontStyles.Normal;
            }
        }

        private void Reserve()
        {
            reservationController?.TryReserve(stopIndex);
        }

        private void ResolveReferences()
        {
            button = GetComponent<Button>();
            label = GetComponentInChildren<TMP_Text>(true);
        }
    }
}
