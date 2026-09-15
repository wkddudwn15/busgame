using System;
using BusMystery.Bus;
using UnityEngine;

namespace BusMystery.Core
{
    public class PauseController : MonoBehaviour
    {
        [SerializeField] private BusClock busClock;

        public event Action<bool> PauseChanged;

        public bool IsPaused => busClock != null && busClock.IsPaused;

        private void Awake()
        {
            if (busClock == null)
            {
                busClock = FindFirstObjectByType<BusClock>();
            }
        }

        private void OnEnable()
        {
            if (busClock != null)
            {
                busClock.PauseChanged += HandlePauseChanged;
            }
        }

        private void OnDisable()
        {
            if (busClock != null)
            {
                busClock.PauseChanged -= HandlePauseChanged;
            }
        }

        public void TogglePause()
        {
            if (busClock != null)
            {
                busClock.TogglePaused();
            }
        }

        public void SetPaused(bool paused)
        {
            if (busClock != null)
            {
                busClock.SetPaused(paused);
            }
        }

        private void HandlePauseChanged(bool paused)
        {
            PauseChanged?.Invoke(paused);
        }
    }
}
