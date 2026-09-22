using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace BusMystery.Words
{
    public class WordCollectionManager : MonoBehaviour
    {
        private readonly HashSet<string> collectedWordIds = new();
        private readonly List<CollectibleWordData> collectedWords = new();
        private ReadOnlyCollection<CollectibleWordData> collectedWordsView;

        public event Action<CollectibleWordData> WordCollected;

        public IReadOnlyList<CollectibleWordData> CollectedWords
        {
            get
            {
                if (collectedWordsView == null)
                {
                    collectedWordsView = collectedWords.AsReadOnly();
                }

                return collectedWordsView;
            }
        }

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

            collectedWords.Add(word);
            WordCollected?.Invoke(word);
            return true;
        }
    }
}
