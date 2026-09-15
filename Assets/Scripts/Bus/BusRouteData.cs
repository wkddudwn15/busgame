using System.Collections.Generic;
using UnityEngine;

namespace BusMystery.Bus
{
    [CreateAssetMenu(menuName = "BusMystery/Bus Route Data", fileName = "BusRouteData")]
    public class BusRouteData : ScriptableObject
    {
        [SerializeField] private List<RouteStopDefinition> stops = new();
        [SerializeField, Min(0f)] private float approachNoticeSeconds = 30f;

        public IReadOnlyList<RouteStopDefinition> Stops => stops;
        public float ApproachNoticeSeconds => approachNoticeSeconds;

        public bool IsValid => stops != null && stops.Count > 0;

        public float GetArrivalTimeSeconds(int stopIndex)
        {
            if (stops == null || stopIndex < 0 || stopIndex >= stops.Count)
            {
                return 0f;
            }

            var total = 0f;
            for (var i = 0; i <= stopIndex; i++)
            {
                total += Mathf.Max(0f, stops[i].TravelTimeFromPreviousSeconds);
            }

            return total;
        }
    }
}
