using System;
using System.Collections.Generic;
using PrimeTween;
using StickerPool.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StickerPool.View
{
    /// <summary>
    /// Feel of every button except the washi tapes: press squash, release spring, a click sound
    /// (through <see cref="Clicked"/>), a haptic tick, and a hit area never smaller than the theme's minimum.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class UiButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        static readonly List<UiButton> Active = new List<UiButton>();

        [SerializeField] UiTheme theme;
        [Tooltip("The Graphic that receives the taps; its raycast area grows to the theme's minimum hit size.")]
        [SerializeField] Graphic hitArea;
        [Tooltip("What gets squashed on press. Defaults to this object.")]
        [SerializeField] Transform visual;
        [SerializeField] HapticKind haptic = HapticKind.Light;
        [SerializeField] bool silent;

        Button button;
        Tween pressTween;

        /// <summary>Raised on every click of any UiButton; the controller plays the click sound.</summary>
        public static event Action Clicked;

        Transform Target => visual != null ? visual : transform;

        void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
            ApplyHitPadding();
        }

        void OnEnable() => Active.Add(this);

        void OnDisable()
        {
            Active.Remove(this);
            ResetScale();
        }

        void OnDestroy() => button.onClick.RemoveListener(OnClick);

        /// <summary>Tween.StopAll() can freeze a button mid-press; this puts them all back.</summary>
        public static void ResetAll()
        {
            foreach (var active in Active) active.ResetScale();
        }

        void ResetScale()
        {
            pressTween.Stop();
            if (this != null) Target.localScale = Vector3.one;
        }

        void OnClick()
        {
            if (silent) return;
            Haptics.Play(haptic);
            Clicked?.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!button.interactable) return;
            float pressed = theme != null ? theme.pressedScale : 0.94f;
            float duration = theme != null ? theme.pressDuration : 0.08f;
            pressTween.Stop();
            pressTween = Tween.Scale(Target, pressed, UiMotion.Time(duration), Ease.OutQuad);
        }

        public void OnPointerUp(PointerEventData eventData) => Release();

        public void OnPointerExit(PointerEventData eventData) => Release();

        void Release()
        {
            if (Target.localScale == Vector3.one) return;
            float duration = theme != null ? theme.releaseDuration : 0.18f;
            pressTween.Stop();
            pressTween = Tween.Scale(Target, 1f, UiMotion.Time(duration), Ease.OutBack);
        }

        void ApplyHitPadding()
        {
            if (hitArea == null) return;
            float minimum = theme != null ? theme.minHitSize : 128f;
            var size = ((RectTransform)hitArea.transform).rect.size;
            float growX = Mathf.Max(0f, (minimum - size.x) * 0.5f);
            float growY = Mathf.Max(0f, (minimum - size.y) * 0.5f);
            hitArea.raycastPadding = new Vector4(-growX, -growY, -growX, -growY);
        }
    }
}
