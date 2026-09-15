using BusMystery.Bus;
using BusMystery.Core;
using TMPro;
using UnityEngine;

namespace BusMystery.UI
{
    public class BusHUD : MonoBehaviour
    {
        [SerializeField] private BusViewController viewController;
        [SerializeField] private BusRouteController routeController;
        [SerializeField] private StopReservationController reservationController;
        [SerializeField] private PauseController pauseController;
        [SerializeField] private TMP_Text viewLabel;
        [SerializeField] private TMP_Text approachLabel;
        [SerializeField] private TMP_Text previousStopLabel;
        [SerializeField] private TMP_Text reservationStatusLabel;
        [SerializeField] private TMP_Text pauseLabel;
        [SerializeField] private TMP_Text eventLogLabel;

        private void OnEnable()
        {
            if (viewController != null)
            {
                viewController.ViewChanged += HandleViewChanged;
            }

            if (routeController != null)
            {
                routeController.RouteStateChanged += RefreshRouteLabels;
                routeController.StopApproachStarted += HandleApproachStarted;
            }

            if (reservationController != null)
            {
                reservationController.ReservationChanged += HandleReservationChanged;
                reservationController.ReservedStopReached += HandleReservedStopReached;
            }

            if (pauseController != null)
            {
                pauseController.PauseChanged += HandlePauseChanged;
            }
        }

        private void OnDisable()
        {
            if (viewController != null)
            {
                viewController.ViewChanged -= HandleViewChanged;
            }

            if (routeController != null)
            {
                routeController.RouteStateChanged -= RefreshRouteLabels;
                routeController.StopApproachStarted -= HandleApproachStarted;
            }

            if (reservationController != null)
            {
                reservationController.ReservationChanged -= HandleReservationChanged;
                reservationController.ReservedStopReached -= HandleReservedStopReached;
            }

            if (pauseController != null)
            {
                pauseController.PauseChanged -= HandlePauseChanged;
            }
        }

        private void Start()
        {
            if (viewController != null)
            {
                HandleViewChanged(viewController.CurrentView);
            }

            RefreshRouteLabels();
            HandlePauseChanged(pauseController != null && pauseController.IsPaused);
            if (reservationStatusLabel != null)
            {
                reservationStatusLabel.text = "予約：なし";
            }
        }

        private void HandleViewChanged(BusViewDirection view)
        {
            if (viewLabel != null)
            {
                viewLabel.text = $"{view.ToString().ToUpperInvariant()} VIEW";
            }
        }

        private void HandleApproachStarted(RouteStopDefinition stop, int stopIndex)
        {
            RefreshRouteLabels();
        }

        private void RefreshRouteLabels()
        {
            if (approachLabel != null)
            {
                var showApproach = routeController != null && routeController.ApproachNoticeActive && routeController.NextStop != null;
                approachLabel.text = showApproach ? $"まもなく　{routeController.NextStop.DisplayName}" : string.Empty;
            }

            if (previousStopLabel != null)
            {
                var lastStop = routeController != null ? routeController.LastPassedStop : null;
                previousStopLabel.text = lastStop != null ? $"直前：{lastStop.DisplayName}" : "直前：なし";
            }
        }

        private void HandleReservationChanged(RouteStopDefinition stop, int stopIndex)
        {
            if (reservationStatusLabel != null)
            {
                reservationStatusLabel.text = $"予約：{stop.DisplayName}";
            }
        }

        private void HandleReservedStopReached(RouteStopDefinition stop, int stopIndex)
        {
            if (eventLogLabel != null)
            {
                eventLogLabel.text = $"Reserved stop reached: {stop.DisplayName}";
            }
        }

        private void HandlePauseChanged(bool paused)
        {
            if (pauseLabel != null)
            {
                pauseLabel.gameObject.SetActive(paused);
                pauseLabel.text = "PAUSE";
            }
        }
    }
}
