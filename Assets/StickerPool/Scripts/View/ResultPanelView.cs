using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace StickerPool.View
{
    /// <summary>The win / lose card that covers the jar.</summary>
    public sealed class ResultPanelView : MonoBehaviour
    {
        [SerializeField] CanvasGroup root;
        [SerializeField] RectTransform card;
        [SerializeField] Image[] stars;
        [SerializeField] Color starOn = new Color(0.949f, 0.757f, 0.306f);
        [SerializeField] Color starOff = new Color(0.29f, 0.345f, 0.384f);
        [SerializeField] LocalizedLabel titleLabel;
        [SerializeField] LocalizedLabel bodyLabel;
        [SerializeField] Button nextButton;
        [SerializeField] Button againButton;
        [SerializeField] Button retryButton;

        [Header("Stars")]
        [SerializeField] float starStagger = 0.2f;
        [SerializeField] float starDuration = 0.4f;
        [SerializeField] float starStartScale = 2.2f;
        [SerializeField] float starTilt = 28f;
        [Tooltip("Fraction of the star animation at which it counts as landed (sound and haptic).")]
        [SerializeField, Range(0f, 1f)] float starLandAt = 0.45f;

        [Header("Texts")]
        [SerializeField] LocalizedString winTitle;
        [SerializeField] LocalizedString finalWinTitle;
        [SerializeField] LocalizedString movesUsedText;
        [SerializeField] LocalizedString movesSpareText;
        [SerializeField] LocalizedString loseTitle;
        [SerializeField] LocalizedString loseBody;

        Sequence showSequence;
        Action redraw;

        public event Action NextClicked;
        public event Action AgainClicked;
        public event Action RetryClicked;

        /// <summary>Raised when the n-th star lands on the card (0-based); the controller adds sound and haptics.</summary>
        public event Action<int> StarLanded;

        public bool IsVisible => root.gameObject.activeSelf;

        void Awake()
        {
            nextButton.onClick.AddListener(() => NextClicked?.Invoke());
            againButton.onClick.AddListener(() => AgainClicked?.Invoke());
            retryButton.onClick.AddListener(() => RetryClicked?.Invoke());
        }

        void OnDestroy()
        {
            nextButton.onClick.RemoveAllListeners();
            againButton.onClick.RemoveAllListeners();
            retryButton.onClick.RemoveAllListeners();
        }

        public void ShowWin(int starCount, int movesUsed, int budget, bool isFinalLevel)
        {
            redraw = () => ShowWin(starCount, movesUsed, budget, isFinalLevel);
            int spare = budget - movesUsed;

            titleLabel.SetKey(isFinalLevel ? finalWinTitle : winTitle);
            string body = movesUsedText.GetLocalizedString(movesUsed, budget);
            if (spare > 0) body += " " + movesSpareText.GetLocalizedString(spare);
            bodyLabel.SetText(body);

            nextButton.gameObject.SetActive(!isFinalLevel);
            againButton.gameObject.SetActive(true);
            retryButton.gameObject.SetActive(false);
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].gameObject.SetActive(true);
                stars[i].color = i < starCount ? starOn : starOff;
            }

            Present(starCount);
        }

        public void ShowLose(int stickersLeft)
        {
            redraw = () => ShowLose(stickersLeft);
            titleLabel.SetKey(loseTitle);
            bodyLabel.SetText(loseBody.GetLocalizedString(stickersLeft));

            nextButton.gameObject.SetActive(false);
            againButton.gameObject.SetActive(false);
            retryButton.gameObject.SetActive(true);
            foreach (var star in stars) star.gameObject.SetActive(false);

            Present(0);
        }

        /// <summary>Re-renders the texts, e.g. after a language change.</summary>
        public void Refresh()
        {
            if (IsVisible) redraw?.Invoke();
        }

        public void Hide()
        {
            showSequence.Stop();
            redraw = null;
            root.gameObject.SetActive(false);
        }

        void Present(int starCount)
        {
            showSequence.Stop();
            root.gameObject.SetActive(true);
            root.alpha = 0f;
            card.localScale = Vector3.one * 0.85f;
            foreach (var star in stars) star.transform.localScale = Vector3.zero;

            showSequence = Sequence.Create()
                .Group(Tween.Alpha(root, 1f, UiMotion.Time(0.25f)))
                .Group(Tween.Scale(card, 1f, UiMotion.Time(0.35f), Ease.OutBack));

            for (int i = 0; i < stars.Length; i++)
            {
                if (!stars[i].gameObject.activeSelf) continue;
                stars[i].transform.localRotation = Quaternion.identity;
                if (i < starCount)
                    showSequence.Group(StickStar(i));
                else
                    stars[i].transform.localScale = Vector3.one;
            }
        }

        /// <summary>A star is slapped on like a sticker: it starts big and tilted, then lands with a bounce.</summary>
        Tween StickStar(int index)
        {
            var star = stars[index].transform;
            float delay = UiMotion.Time(0.3f + index * starStagger);
            float duration = UiMotion.Time(starDuration);
            float tilt = index % 2 == 0 ? -starTilt : starTilt;

            Tween.Delay(delay + duration * starLandAt, () => StarLanded?.Invoke(index));
            return Tween.Custom(0f, 1f, duration, onValueChange: t =>
            {
                float eased = Mathf.Clamp01(t);
                star.localScale = Vector3.one * Mathf.LerpUnclamped(starStartScale, 1f, eased);
                star.localRotation = Quaternion.Euler(0f, 0f, tilt * (1f - eased));
            }, ease: Ease.OutBack, startDelay: delay);
        }
    }
}
