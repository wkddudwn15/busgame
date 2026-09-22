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
                Debug.Log($"[Phase4Debug] WordCollectionManager.Collect managerInstanceId={GetInstanceID()} wordId=<null-or-empty> collectedWordsCount={CollectedWords.Count}");
                LogSceneManagers();
                return false;
            }

            if (!collectedWordIds.Add(word.Id))
            {
                Debug.Log($"[Phase4Debug] WordCollectionManager.Collect managerInstanceId={GetInstanceID()} wordId={word.Id} duplicate=true collectedWordsCount={CollectedWords.Count}");
                LogSceneManagers();
                return false;
            }

            collectedWords.Add(word);
            Debug.Log($"[Phase4Debug] WordCollectionManager.Collect managerInstanceId={GetInstanceID()} wordId={word.Id} duplicate=false collectedWordsCount={CollectedWords.Count}");
            LogSceneManagers();
            WordCollected?.Invoke(word);
            return true;
        }

        private static void LogSceneManagers()
        {
            var managers = FindObjectsByType<WordCollectionManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Debug.Log($"[Phase4Debug] Scene WordCollectionManager count={managers.Length}");
            foreach (var manager in managers)
            {
                Debug.Log($"[Phase4Debug] Scene WordCollectionManager instanceId={manager.GetInstanceID()} name={manager.name} activeSelf={manager.gameObject.activeSelf} activeInHierarchy={manager.gameObject.activeInHierarchy} collectedWordsCount={manager.CollectedWords.Count}");
            }
        }
    }
}
