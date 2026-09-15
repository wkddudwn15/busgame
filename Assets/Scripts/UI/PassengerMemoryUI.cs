using BusMystery.Passengers;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BusMystery.UI
{
    public class PassengerMemoryUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text speakerLabel;
        [SerializeField] private TMP_Text bodyLabel;

        private PassengerMemoryData currentData;
        private int lineIndex;
        private int openedFrame = -1;

        public static bool IsAnyOpen { get; private set; }
        public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

        private void Awake()
        {
            if (panelRoot == null)
            {
                panelRoot = gameObject;
            }

            Close();
        }

        private void Update()
        {
            if (!IsOpen)
            {
                return;
            }

            if (Time.frameCount == openedFrame)
            {
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.escapeKey.wasPressedThisFrame)
                {
                    Close();
                    return;
                }

                if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
                {
                    Advance();
                }
            }

            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                Advance();
            }
        }

        public void Open(PassengerMemoryData data)
        {
            if (data == null)
            {
                return;
            }

            currentData = data;
            lineIndex = 0;
            openedFrame = Time.frameCount;
            panelRoot.SetActive(true);
            IsAnyOpen = true;
            Refresh();
        }

        public void Advance()
        {
            if (currentData == null || currentData.MemoryLines == null || currentData.MemoryLines.Count == 0)
            {
                Close();
                return;
            }

            lineIndex++;
            if (lineIndex >= currentData.MemoryLines.Count)
            {
                Close();
                return;
            }

            Refresh();
        }

        public void Close()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            currentData = null;
            lineIndex = 0;
            openedFrame = -1;
            IsAnyOpen = false;
        }

        private void Refresh()
        {
            if (currentData == null)
            {
                return;
            }

            if (speakerLabel != null)
            {
                speakerLabel.text = currentData.DisplayName;
            }

            if (bodyLabel != null)
            {
                var hasLine = currentData.MemoryLines != null && lineIndex >= 0 && lineIndex < currentData.MemoryLines.Count;
                bodyLabel.text = hasLine ? currentData.MemoryLines[lineIndex] : string.Empty;
            }
        }
    }
}
