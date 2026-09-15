using System.Collections.Generic;
using BusMystery.Bus;
using BusMystery.Words;
using UnityEngine;

namespace BusMystery.Passengers
{
    [CreateAssetMenu(menuName = "BusMystery/Passenger Memory Data", fileName = "PassengerMemoryData")]
    public class PassengerMemoryData : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private BusViewDirection viewDirection = BusViewDirection.Front;
        [SerializeField] private Vector2 hotspotAnchorMin = new(0.4f, 0.4f);
        [SerializeField] private Vector2 hotspotAnchorMax = new(0.6f, 0.7f);
        [SerializeField] private List<string> memoryLines = new();
        [SerializeField] private List<MemoryWordSpan> collectibleWordSpans = new();

        public string Id => id;
        public string DisplayName => displayName;
        public BusViewDirection ViewDirection => viewDirection;
        public Vector2 HotspotAnchorMin => hotspotAnchorMin;
        public Vector2 HotspotAnchorMax => hotspotAnchorMax;
        public IReadOnlyList<string> MemoryLines => memoryLines;
        public IReadOnlyList<MemoryWordSpan> CollectibleWordSpans => collectibleWordSpans;
    }
}
