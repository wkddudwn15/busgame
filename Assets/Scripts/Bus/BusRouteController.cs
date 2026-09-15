using System;
using UnityEngine;

namespace BusMystery.Bus
{
    public class BusRouteController : MonoBehaviour
    {
        [SerializeField] private BusRouteData routeData;
        [SerializeField] private BusClock busClock;

        private int nextStopIndex;
        private int lastPassedStopIndex = -1;
        private bool approachNoticeActive;

        public event Action<RouteStopDefinition, int> StopApproachStarted;
        public event Action<RouteStopDefinition, int> StopPassed;
        public event Action RouteStateChanged;

        public BusRouteData RouteData => routeData;
        public int NextStopIndex => nextStopIndex;
        public int LastPassedStopIndex => lastPassedStopIndex;
        public bool ApproachNoticeActive => approachNoticeActive;
        public RouteStopDefinition LastPassedStop => IsValidStopIndex(lastPassedStopIndex) ? routeData.Stops[lastPassedStopIndex] : null;
        public RouteStopDefinition NextStop => IsValidStopIndex(nextStopIndex) ? routeData.Stops[nextStopIndex] : null;
        public float NextStopArrivalTimeSeconds => IsValidStopIndex(nextStopIndex) ? routeData.GetArrivalTimeSeconds(nextStopIndex) : 0f;
        public float SecondsUntilNextStop => Mathf.Max(0f, NextStopArrivalTimeSeconds - (busClock != null ? busClock.ElapsedSeconds : 0f));

        private void Awake()
        {
            if (busClock == null)
            {
                busClock = GetComponent<BusClock>();
            }
        }

        private void OnEnable()
        {
            if (busClock != null)
            {
                busClock.Ticked += HandleClockTicked;
            }
        }

        private void OnDisable()
        {
            if (busClock != null)
            {
                busClock.Ticked -= HandleClockTicked;
            }
        }

        private void Start()
        {
            RecalculateStateFromTime();
        }

        public bool IsPassed(int stopIndex)
        {
            return stopIndex <= lastPassedStopIndex;
        }

        public bool IsValidStopIndex(int stopIndex)
        {
            return routeData != null && routeData.Stops != null && stopIndex >= 0 && stopIndex < routeData.Stops.Count;
        }

        public float GetArrivalTimeSeconds(int stopIndex)
        {
            return routeData != null ? routeData.GetArrivalTimeSeconds(stopIndex) : 0f;
        }

        public void SkipToNextStop()
        {
            if (!IsValidStopIndex(nextStopIndex) || busClock == null)
            {
                return;
            }

            busClock.SetElapsedSeconds(GetArrivalTimeSeconds(nextStopIndex));
        }

        public void JumpToBeforeStop(int stopIndex, float secondsBefore = 20f)
        {
            if (!IsValidStopIndex(stopIndex) || busClock == null)
            {
                return;
            }

            busClock.SetElapsedSeconds(Mathf.Max(0f, GetArrivalTimeSeconds(stopIndex) - Mathf.Max(0f, secondsBefore)));
        }

        private void HandleClockTicked(float elapsedSeconds)
        {
            if (routeData == null || !routeData.IsValid)
            {
                return;
            }

            var newLastPassedStopIndex = -1;
            for (var i = 0; i < routeData.Stops.Count; i++)
            {
                if (elapsedSeconds >= GetArrivalTimeSeconds(i))
                {
                    newLastPassedStopIndex = i;
                }
            }

            var oldLastPassedStopIndex = lastPassedStopIndex;
            lastPassedStopIndex = newLastPassedStopIndex;
            nextStopIndex = lastPassedStopIndex + 1;

            if (newLastPassedStopIndex > oldLastPassedStopIndex)
            {
                for (var i = oldLastPassedStopIndex + 1; i <= newLastPassedStopIndex; i++)
                {
                    StopPassed?.Invoke(routeData.Stops[i], i);
                }
            }

            var wasApproachNoticeActive = approachNoticeActive;
            approachNoticeActive = IsValidStopIndex(nextStopIndex) && SecondsUntilNextStop <= routeData.ApproachNoticeSeconds;

            if (approachNoticeActive && !wasApproachNoticeActive)
            {
                StopApproachStarted?.Invoke(routeData.Stops[nextStopIndex], nextStopIndex);
            }

            if (newLastPassedStopIndex != oldLastPassedStopIndex || approachNoticeActive != wasApproachNoticeActive)
            {
                RouteStateChanged?.Invoke();
            }
        }

        private void RecalculateStateFromTime()
        {
            nextStopIndex = 0;
            lastPassedStopIndex = -1;
            approachNoticeActive = false;

            if (busClock != null)
            {
                HandleClockTicked(busClock.ElapsedSeconds);
            }
        }
    }
}
