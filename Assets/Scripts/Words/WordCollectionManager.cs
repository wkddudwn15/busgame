using System;
using System.Collections.Generic;
using UnityEngine;

namespace BusMystery.Words
{
    public class WordCollectionManager : MonoBehaviour
    {
        private readonly HashSet<string> collectedWordIds = new();

        public event Action<CollectibleWordData> WordCollected;

        public bool IsCollected(CollectibleWordData word)
        {
            return word != null && collectedWordIds.Contains(word.Id);
        }

        public bool Collect(CollectibleWordData word)
        {
            if (word == null || string.IsNullOrEmpty(word.Id))
            {
                return false;
            }

            if (!collectedWordIds.Add(word.Id))
            {
                return false;
            }

            WordCollected?.Invoke(word);
            return true;
        }
    }
}
