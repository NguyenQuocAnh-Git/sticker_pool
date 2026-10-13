using System;
using PrimeTween;
using StickerPool.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StickerPool.View
{
    /// <summary>
    /// One washi-tape word button. The tape is tinted by the word it carries (not by its slot), so a word
    /// always keeps the same colour and the colour words match what they name.
    /// </summary>
    public sealed class TapeView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] Button button;
        [SerializeField] CanvasGroup group;
        [SerializeField] Image paper;
        [SerializeField] LocalizedLabel categoryLabel;
        [SerializeField] LocalizedLabel wordLabel;
        [Tooltip("Resting tilt in degrees.")]
        [SerializeField] float restRotation = -3f;
        [SerializeField] float waitingAlpha = 0.45f;
        [SerializeField] float pressedScale = 0.94f;
        [SerializeField] Color darkInk = new Color(0.165f, 0.137f, 0.125f);
        [SerializeField] Color lightInk = new Color(1f, 1f, 1f);
        [Tooltip("Tapes brighter than this get dark text, the rest get light text.")]
        [SerializeField, Range(0f, 1f)] float inkSwitchLuminance = 0.5f;

        [Header("Entrance")]
        [SerializeField] float appearDuration = 0.6f;
        [Tooltip("Canvas pixels the tape starts below its resting place.")]
        [SerializeField] float appearDrop = 170f;
        [Tooltip("Extra tilt in degrees the tape starts with before it settles.")]
        [SerializeField] float appearKick = 14f;
        [Tooltip("Scale the tape starts at.")]
        [SerializeField, Range(0.2f, 1f)] float appearStartScale = 0.6f;
        [Tooltip("How much the tape flattens for a moment when it sticks down.")]
        [SerializeField, Range(0f, 0.2f)] float appearSquash = 0.08f;

        [SerializeField] float disappearDrop = 70f;

        Tween pressTween;
        Tween actionTween;
        Tween fadeTween;

        public event Action<TapeView> Clicked;

        public TagData Tag { get; private set; }
        public Vector3 AnchorWorld => transform.position;

        void Awake() => button.onClick.AddListener(() => Clicked?.Invoke(this));

        void OnDestroy() => button.onClick.RemoveAllListeners();

        public void Show(TagData tag)
        {
            Tag = tag;
            actionTween.Stop();
            fadeTween.Stop();
            pressTween.Stop();
            gameObject.SetActive(true);
            categoryLabel.SetKey(tag.Category.Label);
            wordLabel.SetKey(tag.Label);
            ApplyColors(tag.TapeColor);
            group.alpha = 1f;
            group.blocksRaycasts = true;
            button.interactable = true;
            ApplyPose(0f, 1f);
        }

        /// <summary>
        /// Empties the slot. The object stays in the layout (just invisible) so the other tapes never shift.
        /// </summary>
        public void Hide()
        {
            actionTween.Stop();
            fadeTween.Stop();
            pressTween.Stop();
            Tag = null;
            group.alpha = 0f;
            group.blocksRaycasts = false;
            button.interactable = false;
        }

        /// <summary>A tape that keeps its word comes back to full strength after a turn, without any entrance.</summary>
        public void Restore()
        {
            button.interactable = true;
            fadeTween.Stop();
            fadeTween = Tween.Alpha(group, 1f, 0.2f);
        }

        /// <summary>The tape peels away: it drops a little, tilts, shrinks and fades.</summary>
        public void PlayDisappear(float duration)
        {
            actionTween.Stop();
            fadeTween.Stop();
            pressTween.Stop();
            button.interactable = false;
            var rect = (RectTransform)transform;
            Vector2 start = rect.anchoredPosition;
            float kick = restRotation <= 0f ? 10f : -10f;
            actionTween = Tween.Custom(0f, 1f, duration, onValueChange: t =>
            {
                float e = t * t;
                rect.anchoredPosition = start + Vector2.down * (disappearDrop * e);
                rect.localRotation = Quaternion.Euler(0f, 0f, restRotation + kick * e);
                float s = Mathf.Lerp(1f, 0.7f, e);
                rect.localScale = new Vector3(s, s, 1f);
                group.alpha = 1f - e;
            }, ease: Ease.Linear);
        }

        /// <summary>While stickers are resolving: untouched tapes fade, the chosen one stays bright.</summary>
        public void SetWaiting(bool waiting, bool isChosen)
        {
            fadeTween.Stop();
            button.interactable = !waiting;
            group.alpha = waiting && !isChosen ? waitingAlpha : 1f;
        }

        /// <summary>
        /// A fresh tape slides up from below, tilts, overshoots and sticks down with a small squash.
        /// Call right after <see cref="Show"/>; the tape stays invisible until its delay has passed.
        /// </summary>
        public void PlayAppear(float delay)
        {
            actionTween.Stop();
            fadeTween.Stop();
            var rect = (RectTransform)transform;
            if (rect.parent is RectTransform parent) LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
            Vector2 rest = rect.anchoredPosition;
            float kick = restRotation <= 0f ? -appearKick : appearKick;

            ApplyAppear(rect, rest, kick, 0f);
            actionTween = Tween.Custom(0f, 1f, appearDuration,
                onValueChange: t => ApplyAppear(rect, rest, kick, t), ease: Ease.Linear, startDelay: delay);
        }

        void ApplyAppear(RectTransform rect, Vector2 rest, float kick, float t)
        {
            float eased = EaseOutBack(Mathf.Clamp01(t));
            rect.anchoredPosition = rest + Vector2.down * (appearDrop * (1f - eased));
            rect.localRotation = Quaternion.Euler(0f, 0f, restRotation + kick * (1f - eased));

            float scale = Mathf.LerpUnclamped(appearStartScale, 1f, eased);
            float squash = t > 0.45f ? Mathf.Sin((t - 0.45f) / 0.55f * Mathf.PI) * appearSquash : 0f;
            rect.localScale = new Vector3(scale * (1f + squash), scale * (1f - squash), 1f);
            group.alpha = Mathf.Clamp01(t * 4f);
        }

        static float EaseOutBack(float x)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            float p = x - 1f;
            return 1f + c3 * p * p * p + c1 * p * p;
        }

        public void PlayPicked()
        {
            actionTween.Stop();
            actionTween = Tween.Custom(0f, 1f, 0.5f, onValueChange: t =>
            {
                float bump = t < 0.4f ? t / 0.4f : (1f - t) / 0.6f;
                ApplyPose(bump, 1f + 0.1f * bump);
            }, ease: Ease.Linear);
        }

        public void PlayBounce(float duration)
        {
            actionTween.Stop();
            actionTween = Tween.Custom(0f, 1f, duration, onValueChange: t =>
            {
                float bump = 1f - Mathf.Abs(t * 2f - 1f);
                ApplyPose(bump, 1f + 0.12f * bump);
            }, ease: Ease.Linear);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!button.interactable) return;
            pressTween.Stop();
            pressTween = Tween.Scale(transform, pressedScale, 0.08f, Ease.OutQuad);
        }

        public void OnPointerUp(PointerEventData eventData) => ReleasePress();

        public void OnPointerExit(PointerEventData eventData) => ReleasePress();

        void ReleasePress()
        {
            pressTween.Stop();
            pressTween = Tween.Scale(transform, 1f, 0.15f, Ease.OutBack);
        }

        void ApplyColors(Color tapeColor)
        {
            paper.color = tapeColor;
            float luminance = 0.2126f * tapeColor.r + 0.7152f * tapeColor.g + 0.0722f * tapeColor.b;
            var ink = luminance > inkSwitchLuminance ? darkInk : lightInk;
            wordLabel.Text.color = ink;
            var categoryInk = ink;
            categoryInk.a = 0.75f;
            categoryLabel.Text.color = categoryInk;
        }

        void ApplyPose(float straighten, float scale)
        {
            transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(restRotation, 0f, straighten));
            transform.localScale = Vector3.one * scale;
        }
    }
}
