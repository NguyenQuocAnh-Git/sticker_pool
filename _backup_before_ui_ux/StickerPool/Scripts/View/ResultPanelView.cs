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
                .Group(Tween.Alpha(root, 1f, 0.25f))
                .Group(Tween.Scale(card, 1f, 0.35f, Ease.OutBack));

            for (int i = 0; i < stars.Length; i++)
            {
                if (!stars[i].gameObject.activeSelf) continue;
                if (i < starCount)
                    showSequence.Group(Tween.Scale(stars[i].transform, 0f, 1f, 0.35f, Ease.OutBack, startDelay: 0.25f + i * 0.14f));
                else
                    stars[i].transform.localScale = Vector3.one;
            }
        }
    }
}
