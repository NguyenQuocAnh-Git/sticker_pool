using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace StickerPool.View
{
    /// <summary>A sliding switch. It only reports a tap; the owner decides the new state and calls <see cref="Set"/>.</summary>
    public sealed class UiToggle : MonoBehaviour
    {
        [SerializeField] Button button;
        [SerializeField] Image track;
        [SerializeField] RectTransform knob;
        [SerializeField] Color onColor = new Color32(0x7F, 0xB5, 0x9A, 0xFF);
        [SerializeField] Color offColor = new Color32(0xD9, 0xCB, 0xB8, 0xFF);
        [Tooltip("Distance of the knob from the centre, in canvas pixels.")]
        [SerializeField] float travel = 42f;
        [SerializeField] float duration = 0.2f;

        Tween tween;
        float position01;

        public event Action Toggled;

        public bool IsOn { get; private set; }

        void Awake() => button.onClick.AddListener(() => Toggled?.Invoke());

        void OnDestroy() => button.onClick.RemoveAllListeners();

        public void Set(bool on, bool animate)
        {
            IsOn = on;
            tween.Stop();
            float target = on ? 1f : 0f;
            if (!animate || !isActiveAndEnabled)
            {
                Apply(target);
                return;
            }

            tween = Tween.Custom(position01, target, UiMotion.Time(duration), onValueChange: Apply, ease: Ease.OutBack);
        }

        void Apply(float t)
        {
            position01 = t;
            knob.anchoredPosition = new Vector2(Mathf.LerpUnclamped(-travel, travel, t), knob.anchoredPosition.y);
            track.color = Color.Lerp(offColor, onColor, Mathf.Clamp01(t));
        }
    }
}
