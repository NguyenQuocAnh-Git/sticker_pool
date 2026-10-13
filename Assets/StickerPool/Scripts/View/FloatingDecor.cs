using PrimeTween;
using UnityEngine;

namespace StickerPool.View
{
    /// <summary>A decoration that bobs and sways forever (menu stickers, tutorial hand). Restarts when enabled.</summary>
    public sealed class FloatingDecor : MonoBehaviour
    {
        [SerializeField] float bobDistance = 18f;
        [SerializeField] float swayDegrees = 4f;
        [SerializeField] float period = 2.6f;
        [Tooltip("Shifts the phase so a group of decorations does not move in sync.")]
        [SerializeField, Range(0f, 1f)] float phase;

        Tween tween;
        Vector2 restPosition;
        float restRotation;

        void OnEnable()
        {
            var rect = (RectTransform)transform;
            restPosition = rect.anchoredPosition;
            restRotation = rect.localEulerAngles.z;
            if (UiMotion.Reduced) return;

            tween = Tween.Custom(0f, 1f, period, onValueChange: t =>
            {
                float wave = Mathf.Sin((t + phase) * Mathf.PI * 2f);
                rect.anchoredPosition = restPosition + Vector2.up * (wave * bobDistance);
                rect.localRotation = Quaternion.Euler(0f, 0f, restRotation + wave * swayDegrees);
            }, ease: Ease.Linear, cycles: -1, cycleMode: CycleMode.Restart);
        }

        void OnDisable()
        {
            tween.Stop();
            var rect = (RectTransform)transform;
            rect.anchoredPosition = restPosition;
            rect.localRotation = Quaternion.Euler(0f, 0f, restRotation);
        }
    }
}
