using System;
using PrimeTween;
using StickerPool.Data;
using UnityEngine;
using UnityEngine.UI;

namespace StickerPool.View
{
    /// <summary>
    /// A paper card over a dimmed screen: fades and springs in, shrinks out. Tapping the dim area or the
    /// close button closes it. Subclasses add the content and the events.
    /// </summary>
    public class PopupView : MonoBehaviour
    {
        [SerializeField] protected UiTheme theme;
        [SerializeField] CanvasGroup root;
        [SerializeField] RectTransform card;
        [Tooltip("Optional: tapping outside the card closes the popup.")]
        [SerializeField] Button dimButton;
        [SerializeField] Button closeButton;

        Sequence sequence;

        /// <summary>Raised when any popup starts opening; the controller plays the sound.</summary>
        public static event Action AnyOpened;

        /// <summary>Raised when any popup starts closing.</summary>
        public static event Action AnyClosing;

        public event Action Closed;

        public bool IsOpen { get; private set; }

        protected virtual void Awake()
        {
            if (dimButton != null) dimButton.onClick.AddListener(Close);
            if (closeButton != null) closeButton.onClick.AddListener(Close);
        }

        protected virtual void OnDestroy()
        {
            if (dimButton != null) dimButton.onClick.RemoveListener(Close);
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
        }

        public void Open()
        {
            if (IsOpen) return;
            IsOpen = true;
            OnOpening();

            sequence.Stop();
            root.gameObject.SetActive(true);
            root.alpha = 0f;
            root.blocksRaycasts = true;
            card.localScale = Vector3.one * theme.popupOpenScale;

            float duration = UiMotion.Time(theme.popupOpenDuration);
            sequence = Sequence.Create()
                .Group(Tween.Alpha(root, 1f, duration))
                .Group(Tween.Scale(card, 1f, duration, Ease.OutBack));
            AnyOpened?.Invoke();
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            AnyClosing?.Invoke();

            sequence.Stop();
            root.blocksRaycasts = false;
            float duration = UiMotion.Time(theme.popupCloseDuration);
            sequence = Sequence.Create()
                .Group(Tween.Alpha(root, 0f, duration))
                .Group(Tween.Scale(card, theme.popupCloseScale, duration, Ease.InQuad))
                .ChainCallback(Finish);
        }

        /// <summary>Hides at once without sound or animation (used when the screen behind is rebuilt).</summary>
        public void CloseImmediate()
        {
            if (!IsOpen && !root.gameObject.activeSelf) return;
            IsOpen = false;
            sequence.Stop();
            root.blocksRaycasts = false;
            root.gameObject.SetActive(false);
        }

        protected virtual void OnOpening()
        {
        }

        void Finish()
        {
            root.gameObject.SetActive(false);
            Closed?.Invoke();
        }
    }
}
