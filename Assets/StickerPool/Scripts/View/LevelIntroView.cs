using PrimeTween;
using UnityEngine;
using UnityEngine.Localization;

namespace StickerPool.View
{
    /// <summary>"Level 5 · 14 stickers · 6 moves" banner that slides in as a level starts. It never blocks taps.</summary>
    public sealed class LevelIntroView : MonoBehaviour
    {
        [SerializeField] CanvasGroup root;
        [SerializeField] RectTransform banner;
        [SerializeField] LocalizedLabel titleLabel;
        [SerializeField] LocalizedLabel detailLabel;
        [SerializeField] LocalizedString titleText;
        [SerializeField] LocalizedString detailText;
        [SerializeField] float slideDistance = 160f;
        [SerializeField] float slideDuration = 0.4f;
        [SerializeField] float holdDuration = 0.8f;
        [SerializeField] float fadeDuration = 0.25f;

        Sequence sequence;
        float restY;
        bool restKnown;

        public void Play(int levelNumber, int stickerCount, int moves)
        {
            if (!restKnown)
            {
                restY = banner.anchoredPosition.y;
                restKnown = true;
            }

            titleLabel.SetKey(titleText, levelNumber);
            detailLabel.SetKey(detailText, stickerCount, moves);

            sequence.Stop();
            root.gameObject.SetActive(true);
            root.blocksRaycasts = false;
            root.alpha = 0f;
            SetY(restY + slideDistance);

            float slide = UiMotion.Time(slideDuration);
            sequence = Sequence.Create()
                .Group(Tween.Alpha(root, 1f, UiMotion.Time(fadeDuration)))
                .Group(Tween.Custom(restY + slideDistance, restY, slide, SetY, Ease.OutBack))
                .ChainDelay(UiMotion.Time(holdDuration))
                .Chain(Tween.Alpha(root, 0f, UiMotion.Time(fadeDuration)))
                .ChainCallback(() => root.gameObject.SetActive(false));
        }

        public void Hide()
        {
            sequence.Stop();
            root.gameObject.SetActive(false);
        }

        void SetY(float y) => banner.anchoredPosition = new Vector2(banner.anchoredPosition.x, y);
    }
}
