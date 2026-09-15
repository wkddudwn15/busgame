using System;
using UnityEngine;

namespace BusMystery.Bus
{
    public class BusViewController : MonoBehaviour
    {
        [SerializeField] private BusViewDirection currentView = BusViewDirection.Front;

        public event Action<BusViewDirection> ViewChanged;

        public BusViewDirection CurrentView => currentView;

        private void Start()
        {
            ViewChanged?.Invoke(currentView);
        }

        public void SetView(BusViewDirection view)
        {
            if (currentView == view)
            {
                return;
            }

            currentView = view;
            ViewChanged?.Invoke(currentView);
        }

        public void ShowLeft()
        {
            SetView(BusViewDirection.Left);
        }

        public void ShowFront()
        {
            SetView(BusViewDirection.Front);
        }

        public void ShowRight()
        {
            SetView(BusViewDirection.Right);
        }
    }
}
