using System;
using UnityEngine;

namespace BusMystery.Bus
{
    public class BusClock : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float elapsedSeconds;
        [SerializeField, Min(0f)] private float timeMultiplier = 1f;
        [SerializeField] private bool paused;

        public event Action<float> Ticked;
        public event Action<bool> PauseChanged;
        public event Action<float> TimeMultiplierChanged;

        public float ElapsedSeconds => elapsedSeconds;
        public float TimeMultiplier => timeMultiplier;
        public bool IsPaused => paused;

        private void Update()
        {
            if (paused)
            {
                return;
            }

            elapsedSeconds += Time.unscaledDeltaTime * timeMultiplier;
            Ticked?.Invoke(elapsedSeconds);
        }

        public void SetPaused(bool value)
        {
            if (paused == value)
            {
                return;
            }

            paused = value;
            PauseChanged?.Invoke(paused);
        }

        public void TogglePaused()
        {
            SetPaused(!paused);
        }

        public void SetTimeMultiplier(float multiplier)
        {
            timeMultiplier = Mathf.Max(0f, multiplier);
            TimeMultiplierChanged?.Invoke(timeMultiplier);
        }

        public void SetElapsedSeconds(float seconds)
        {
            elapsedSeconds = Mathf.Max(0f, seconds);
            Ticked?.Invoke(elapsedSeconds);
        }
    }
}
