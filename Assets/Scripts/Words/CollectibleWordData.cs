using UnityEngine;

namespace BusMystery.Words
{
    [CreateAssetMenu(menuName = "BusMystery/Collectible Word Data", fileName = "CollectibleWordData")]
    public class CollectibleWordData : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayText;
        [SerializeField, TextArea] private string description;
        [SerializeField] private CollectibleWordType internalType;

        public string Id => id;
        public string DisplayText => displayText;
        public string Description => description;
        public CollectibleWordType InternalType => internalType;
    }
}
