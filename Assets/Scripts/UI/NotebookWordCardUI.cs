using BusMystery.Words;
using TMPro;
using UnityEngine;

namespace BusMystery.UI
{
    public class NotebookWordCardUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text wordLabel;

        public void Initialize(CollectibleWordData word)
        {
            if (wordLabel == null)
            {
                wordLabel = GetComponentInChildren<TMP_Text>(true);
            }

            if (wordLabel != null)
            {
                wordLabel.text = word != null ? word.DisplayText : string.Empty;
            }
        }
    }
}
