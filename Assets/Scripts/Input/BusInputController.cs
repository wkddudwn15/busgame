using BusMystery.Bus;
using BusMystery.Core;
using BusMystery.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BusMystery.Input
{
    public class BusInputController : MonoBehaviour
    {
        [SerializeField] private BusViewController viewController;
        [SerializeField] private PauseController pauseController;

        private void Awake()
        {
            if (viewController == null)
            {
                viewController = FindFirstObjectByType<BusViewController>();
            }

            if (pauseController == null)
            {
                pauseController = FindFirstObjectByType<PauseController>();
            }
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (PassengerMemoryUI.IsAnyOpen)
            {
                return;
            }

            if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
            {
                viewController?.ShowLeft();
            }

            if (keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame)
            {
                viewController?.ShowFront();
            }

            if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
            {
                viewController?.ShowRight();
            }

            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                pauseController?.TogglePause();
            }
        }
    }
}
