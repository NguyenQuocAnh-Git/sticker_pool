using PrimeTween;
using UnityEngine;

namespace StickerPool.View
{
    /// <summary>
    /// A hand that keeps tapping a target until the player does it once (first level only).
    /// It follows the target's position, so it works while the tape is still settling.
    /// </summary>
    public sealed class TutorialHandView : MonoBehaviour
    {
        [SerializeField] CanvasGroup group;
        [SerializeField] RectTransform hand;
        [SerializeField] RectTransform ripple;
        [Tooltip("Canvas pixels from the target's centre to the fingertip rest position.")]
        [SerializeField] Vector2 offset = new Vector2(40f, -50f);
        [SerializeField] float tapTravel = 36f;
        [SerializeField] float period = 1.3f;

        Tween loop;
        Tween fade;
        Transform target;

        public bool IsShown { get; private set; }

        void OnDisable() => loop.Stop();

        /// <summary>Appears after <paramref name="delay"/> and starts tapping on the target.</summary>
        public void Show(Transform pointAt, float delay)
        {
            target = pointAt;
            IsShown = true;
            loop.Stop();
            fade.Stop();
            gameObject.SetActive(true);
            group.alpha = 0f;
            group.blocksRaycasts = false;
            Follow();

            fade = Tween.Alpha(group, 1f, UiMotion.Time(0.3f), startDelay: delay);
            loop = Tween.Custom(0f, 1f, period, onValueChange: Animate, ease: Ease.Linear,
                cycles: -1, cycleMode: CycleMode.Restart, startDelay: delay);
        }

        public void Hide()
        {
            if (!IsShown && !gameObject.activeSelf) return;
            IsShown = false;
            loop.Stop();
            fade.Stop();
            gameObject.SetActive(false);
        }

        void Animate(float t)
        {
            Follow();
            // 0-0.45 hover, 0.45-0.6 press down, 0.6-0.8 release, then rest.
            float press = t < 0.45f ? 0f : t < 0.6f ? (t - 0.45f) / 0.15f : t < 0.8f ? 1f - (t - 0.6f) / 0.2f : 0f;
            hand.anchoredPosition = Vector2.down * (press * tapTravel);
            hand.localScale = Vector3.one * (1f - 0.1f * press);

            float ring = Mathf.Clamp01((t - 0.5f) / 0.5f);
            ripple.localScale = Vector3.one * Mathf.Lerp(0.4f, 1.5f, ring);
            ripple.gameObject.SetActive(t >= 0.5f);
            var canvasGroup = ripple.GetComponent<CanvasGroup>();
            if (canvasGroup != null) canvasGroup.alpha = 1f - ring;
        }

        void Follow()
        {
            if (target == null) return;
            var rect = (RectTransform)transform;
            rect.position = target.position;
            rect.anchoredPosition += offset;
        }
    }
}
