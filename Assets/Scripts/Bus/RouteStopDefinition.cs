using System;
using UnityEngine;

namespace BusMystery.Bus
{
    [Serializable]
    public class RouteStopDefinition
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private int order;
        [SerializeField, Min(0f)] private float travelTimeFromPreviousSeconds = 300f;
        [SerializeField] private bool terminal;
        [SerializeField, TextArea] private string futureInfo;

        public string Id => id;
        public string DisplayName => displayName;
        public int Order => order;
        public float TravelTimeFromPreviousSeconds => travelTimeFromPreviousSeconds;
        public bool IsTerminal => terminal;
        public string FutureInfo => futureInfo;
    }
}
