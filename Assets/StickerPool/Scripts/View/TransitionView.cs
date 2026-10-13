using System;
using System.Collections;
using PrimeTween;
using StickerPool.Data;
using UnityEngine;

namespace StickerPool.View
{
    /// <summary>
    /// The paper curtain between screens: it rises over the screen, the caller swaps what is behind it,
    /// then it keeps rising and uncovers the new screen. While it runs it blocks every tap.
    /// </summary>
    public sealed class TransitionView : MonoBehaviour
    {
        [SerializeField] UiTheme theme;
        [Tooltip("Stretches over the whole canvas; its two torn edges stick out above and below.")]
        [SerializeField] RectTransform curtain;
        [Tooltip("Extra travel so the torn edges leave the screen completely.")]
        [SerializeField] float edgeOverhang = 220f;

        public bool IsRunning { get; private set; }

        /// <summary>Raised when the curtain starts to cover the screen.</summary>
        public event Action CoverStarted;

        /// <summary>Raised when the curtain starts to uncover the screen.</summary>
        public event Action RevealStarted;

        /// <param name="atCovered">Runs while nothing of the old screen is visible; swap screens here.</param>
        public IEnumerator Play(Action atCovered)
        {
            IsRunning = true;
            gameObject.SetActive(true);

            float travel = ((RectTransform)curtain.parent).rect.height + edgeOverhang;
            float cover = UiMotion.Time(theme.curtainCoverDuration);
            float reveal = UiMotion.Time(theme.curtainRevealDuration);

            SetY(-travel);
            CoverStarted?.Invoke();
            Tween.Custom(-travel, 0f, cover, SetY, Ease.OutCubic);
            yield return new WaitForSeconds(cover);
            SetY(0f);

            atCovered?.Invoke();
            yield return null;

            RevealStarted?.Invoke();
            Tween.Custom(0f, travel, reveal, SetY, Ease.InCubic);
            yield return new WaitForSeconds(reveal);

            gameObject.SetActive(false);
            IsRunning = false;
        }

        void SetY(float y) => curtain.anchoredPosition = new Vector2(0f, y);
    }
}
