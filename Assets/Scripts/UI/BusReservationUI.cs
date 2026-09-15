using System.Collections.Generic;
using BusMystery.Bus;
using TMPro;
using UnityEngine;

namespace BusMystery.UI
{
    public class BusReservationUI : MonoBehaviour
    {
        [SerializeField] private BusRouteController routeController;
        [SerializeField] private StopReservationController reservationController;
        [SerializeField] private RectTransform optionContainer;
        [SerializeField] private StopReservationOption optionPrefab;
        [SerializeField] private TMP_Text titleLabel;

        private readonly List<StopReservationOption> options = new();

        private void OnEnable()
        {
            if (routeController != null)
            {
                routeController.RouteStateChanged += Refresh;
                routeController.StopPassed += HandleStopPassed;
            }

            if (reservationController != null)
            {
                reservationController.ReservationChanged += HandleReservationChanged;
            }
        }

        private void OnDisable()
        {
            if (routeController != null)
            {
                routeController.RouteStateChanged -= Refresh;
                routeController.StopPassed -= HandleStopPassed;
            }

            if (reservationController != null)
            {
                reservationController.ReservationChanged -= HandleReservationChanged;
            }
        }

        private void Start()
        {
            BuildOptions();
            Refresh();
        }

        private void BuildOptions()
        {
            if (titleLabel != null)
            {
                titleLabel.text = "降車停留所予約";
            }

            if (optionContainer == null || optionPrefab == null || routeController == null || routeController.RouteData == null)
            {
                return;
            }

            foreach (Transform child in optionContainer)
            {
                Destroy(child.gameObject);
            }

            options.Clear();
            var stops = routeController.RouteData.Stops;
            for (var i = 0; i < stops.Count; i++)
            {
                var option = Instantiate(optionPrefab, optionContainer);
                option.gameObject.SetActive(true);
                option.Initialize(i, stops[i], reservationController);
                options.Add(option);
            }
        }

        private void Refresh()
        {
            if (routeController == null || reservationController == null)
            {
                return;
            }

            for (var i = 0; i < options.Count; i++)
            {
                options[i].Refresh(routeController.IsPassed(i), reservationController.ReservedStopIndex == i);
            }
        }

        private void HandleStopPassed(RouteStopDefinition stop, int stopIndex)
        {
            Refresh();
        }

        private void HandleReservationChanged(RouteStopDefinition stop, int stopIndex)
        {
            Refresh();
        }
    }
}
