using BusMystery.Words;
using TMPro;
using UnityEngine;

namespace BusMystery.UI
{
    public class NotebookWordCardUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text wordLabel;
        [SerializeField] private TMP_FontAsset fontAsset;

        public void Initialize(CollectibleWordData word)
        {
            if (wordLabel == null)
            {
                wordLabel = GetComponentInChildren<TMP_Text>(true);
            }

            if (wordLabel != null)
            {
                if (fontAsset != null)
                {
                    wordLabel.font = fontAsset;
                }

                wordLabel.text = word != null ? word.DisplayText : string.Empty;
            }
        }
    }
}
