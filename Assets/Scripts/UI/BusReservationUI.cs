using System.Collections.Generic;
using BusMystery.Bus;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BusMystery.UI
{
    public class BusReservationUI : MonoBehaviour
    {
        [SerializeField] private BusRouteController routeController;
        [SerializeField] private StopReservationController reservationController;
        [SerializeField] private RectTransform optionContainer;
        [SerializeField] private TMP_Text titleLabel;
        [SerializeField] private TMP_FontAsset fontAsset;

        private readonly List<StopReservationOption> options = new();

        private void Awake()
        {
            if (routeController == null)
            {
                routeController = FindFirstObjectByType<BusRouteController>();
            }

            if (reservationController == null)
            {
                reservationController = FindFirstObjectByType<StopReservationController>();
            }

            if (fontAsset == null && titleLabel != null)
            {
                fontAsset = titleLabel.font;
            }
        }

        private void OnEnable()
        {
            if (routeController != null)
            {
                routeController.RouteStateChanged += Refresh;
                routeController.StopPassed += HandleStopPassed;
            }

            if (reservationController != null)
            {
                reservationController.ReservationChanged += HandleReservationChanged;
            }
        }

        private void OnDisable()
        {
            if (routeController != null)
            {
                routeController.RouteStateChanged -= Refresh;
                routeController.StopPassed -= HandleStopPassed;
            }

            if (reservationController != null)
            {
                reservationController.ReservationChanged -= HandleReservationChanged;
            }
        }

        private void Start()
        {
            BuildOptions();
            Refresh();
        }

        private void BuildOptions()
        {
            if (titleLabel != null)
            {
                titleLabel.text = "降車停留所予約";
            }

            if (optionContainer == null || routeController == null || routeController.RouteData == null)
            {
                return;
            }

            foreach (Transform child in optionContainer)
            {
                Destroy(child.gameObject);
            }

            options.Clear();
            var stops = routeController.RouteData.Stops;
            for (var i = 0; i < stops.Count; i++)
            {
                var option = CreateOption();
                option.gameObject.SetActive(true);
                option.Initialize(i, stops[i], reservationController);
                options.Add(option);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(optionContainer);
        }

        private void Refresh()
        {
            if (routeController == null || reservationController == null)
            {
                return;
            }

            var visibilityChanged = false;
            for (var i = 0; i < options.Count; i++)
            {
                var shouldDisplay = ShouldDisplayStop(i);
                if (options[i].gameObject.activeSelf != shouldDisplay)
                {
                    options[i].gameObject.SetActive(shouldDisplay);
                    visibilityChanged = true;
                }

                if (!shouldDisplay)
                {
                    continue;
                }

                options[i].Refresh(routeController.IsPassed(i), reservationController.ReservedStopIndex == i);
            }

            if (visibilityChanged)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(optionContainer);
            }
        }

        private void HandleStopPassed(RouteStopDefinition stop, int stopIndex)
        {
            Refresh();
        }

        private void HandleReservationChanged(RouteStopDefinition stop, int stopIndex)
        {
            Refresh();
        }

        private StopReservationOption CreateOption()
        {
            var option = CreateRuntimeOption(fontAsset);
            var rectTransform = option.GetComponent<RectTransform>();
            rectTransform.SetParent(optionContainer, false);
            rectTransform.anchorMin = new Vector2(0f, 0.5f);
            rectTransform.anchorMax = new Vector2(1f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(0f, 48f);

            var layoutElement = option.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = option.gameObject.AddComponent<LayoutElement>();
            }

            layoutElement.minHeight = 44f;
            layoutElement.preferredHeight = 48f;
            return option;
        }

        private static StopReservationOption CreateRuntimeOption(TMP_FontAsset fontAsset)
        {
            var optionObject = new GameObject("Reservation Option", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement), typeof(StopReservationOption));
            var image = optionObject.GetComponent<Image>();
            image.color = new Color(0.16f, 0.18f, 0.2f);
            image.raycastTarget = true;

            var button = optionObject.GetComponent<Button>();
            button.targetGraphic = image;

            var label = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI)).GetComponent<TMP_Text>();
            label.transform.SetParent(optionObject.transform, false);
            label.text = "停留所";
            label.font = fontAsset;
            label.fontSize = 22f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.NoWrap;

            var labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.anchoredPosition = Vector2.zero;
            labelRect.sizeDelta = Vector2.zero;

            return optionObject.GetComponent<StopReservationOption>();
        }

        private bool ShouldDisplayStop(int stopIndex)
        {
            const int hiddenTerminalStopIndex = 7;
            const int revealAfterStopIndex = 6;

            return stopIndex != hiddenTerminalStopIndex || routeController.IsPassed(revealAfterStopIndex);
        }
    }
}
