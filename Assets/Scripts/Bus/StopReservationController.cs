using System;
using UnityEngine;

namespace BusMystery.Bus
{
    public class StopReservationController : MonoBehaviour
    {
        [SerializeField] private BusRouteController routeController;

        private int reservedStopIndex = -1;
        private bool firstReservationMade;

        public event Action<RouteStopDefinition, int> ReservationChanged;
        public event Action<RouteStopDefinition, int> FirstReservationMade;
        public event Action<RouteStopDefinition, int> ReservedStopReached;

        public int ReservedStopIndex => reservedStopIndex;
        public bool HasReservation => reservedStopIndex >= 0;

        private void Awake()
        {
            if (routeController == null)
            {
                routeController = GetComponent<BusRouteController>();
            }
        }

        private void OnEnable()
        {
            if (routeController != null)
            {
                routeController.StopPassed += HandleStopPassed;
            }
        }

        private void OnDisable()
        {
            if (routeController != null)
            {
                routeController.StopPassed -= HandleStopPassed;
            }
        }

        public bool CanReserve(int stopIndex)
        {
            return routeController != null && routeController.IsValidStopIndex(stopIndex) && !routeController.IsPassed(stopIndex);
        }

        public bool TryReserve(int stopIndex)
        {
            if (!CanReserve(stopIndex))
            {
                return false;
            }

            reservedStopIndex = stopIndex;
            var stop = routeController.RouteData.Stops[stopIndex];
            ReservationChanged?.Invoke(stop, stopIndex);

            if (!firstReservationMade)
            {
                firstReservationMade = true;
                FirstReservationMade?.Invoke(stop, stopIndex);
            }

            return true;
        }

        private void HandleStopPassed(RouteStopDefinition stop, int stopIndex)
        {
            if (stopIndex != reservedStopIndex)
            {
                return;
            }

            Debug.Log($"Reserved stop reached: {stop.DisplayName}");
            ReservedStopReached?.Invoke(stop, stopIndex);
        }
    }
}
