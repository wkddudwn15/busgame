using BusMystery.Bus;
using UnityEngine;

namespace BusMystery.DebugTools
{
    public class DebugBusControls : MonoBehaviour
    {
        [SerializeField] private BusClock busClock;
        [SerializeField] private BusRouteController routeController;
        [SerializeField, Min(0f)] private float jumpBeforeStopSeconds = 20f;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void Awake()
        {
            if (busClock == null)
            {
                busClock = FindFirstObjectByType<BusClock>();
            }

            if (routeController == null)
            {
                routeController = FindFirstObjectByType<BusRouteController>();
            }
        }

        private void OnGUI()
        {
            const int width = 240;
            GUILayout.BeginArea(new Rect(12, 12, width, Screen.height - 24), GUI.skin.box);
            GUILayout.Label("Bus Debug");

            if (busClock != null)
            {
                GUILayout.Label($"Timer: {FormatTime(busClock.ElapsedSeconds)}");
                GUILayout.Label($"Speed: x{busClock.TimeMultiplier:0.##}");
                GUILayout.BeginHorizontal();
                AddSpeedButton(1f);
                AddSpeedButton(2f);
                AddSpeedButton(5f);
                AddSpeedButton(10f);
                GUILayout.EndHorizontal();
            }

            if (routeController != null)
            {
                if (GUILayout.Button("Next Stop"))
                {
                    routeController.SkipToNextStop();
                }

                GUILayout.Label("Jump Before Stop");
                var stops = routeController.RouteData != null ? routeController.RouteData.Stops : null;
                if (stops != null)
                {
                    for (var i = 0; i < stops.Count; i++)
                    {
                        if (GUILayout.Button($"{i + 1}. {stops[i].DisplayName}"))
                        {
                            routeController.JumpToBeforeStop(i, jumpBeforeStopSeconds);
                        }
                    }
                }
            }

            GUILayout.EndArea();
        }

        private void AddSpeedButton(float multiplier)
        {
            if (GUILayout.Button($"x{multiplier:0}"))
            {
                busClock.SetTimeMultiplier(multiplier);
            }
        }

        private static string FormatTime(float seconds)
        {
            var totalSeconds = Mathf.FloorToInt(seconds);
            return $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
        }
#else
        private void Awake()
        {
            gameObject.SetActive(false);
        }
#endif
    }
}
