using System;
using UnityEngine;

namespace BusMystery.Words
{
    [Serializable]
    public class MemoryWordSpan
    {
        [SerializeField, Min(0)] private int lineIndex;
        [SerializeField] private string targetText;
        [SerializeField, Min(0)] private int occurrenceIndex;
        [SerializeField] private CollectibleWordData wordData;

        public int LineIndex => lineIndex;
        public string TargetText => targetText;
        public int OccurrenceIndex => occurrenceIndex;
        public CollectibleWordData WordData => wordData;
    }
}
