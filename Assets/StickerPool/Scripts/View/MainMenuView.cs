using System;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace StickerPool.View
{
    /// <summary>Logo, Play (or Continue), Levels and Settings. Pure display: it only reports taps.</summary>
    public sealed class MainMenuView : UiScreen
    {
        [SerializeField] Button playButton;
        [SerializeField] Button levelsButton;
        [SerializeField] Button settingsButton;
        [SerializeField] LocalizedLabel playLabel;
        [SerializeField] LocalizedString playText;
        [SerializeField] LocalizedString continueText;
        [SerializeField] TMP_Text starsValue;
        [Tooltip("Pops in one after another when the menu appears: logo, buttons.")]
        [SerializeField] RectTransform[] entranceItems;
        [SerializeField] float entranceDuration = 0.45f;
        [SerializeField] float entranceStagger = 0.08f;

        Sequence entrance;

        public event Action PlayClicked;
        public event Action LevelsClicked;
        public event Action SettingsClicked;

        void Awake()
        {
            playButton.onClick.AddListener(() => PlayClicked?.Invoke());
            levelsButton.onClick.AddListener(() => LevelsClicked?.Invoke());
            settingsButton.onClick.AddListener(() => SettingsClicked?.Invoke());
        }

        void OnDestroy()
        {
            playButton.onClick.RemoveAllListeners();
            levelsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.RemoveAllListeners();
        }

        /// <param name="continueLevelNumber">1-based number of the level Play opens next.</param>
        public void SetProgress(int continueLevelNumber, bool hasProgress, int totalStars)
        {
            if (hasProgress) playLabel.SetKey(continueText, continueLevelNumber);
            else playLabel.SetKey(playText);
            starsValue.text = totalStars.ToString();
        }

        protected override void OnShown()
        {
            entrance.Stop();
            entrance = Sequence.Create();
            float duration = UiMotion.Time(entranceDuration);
            for (int i = 0; i < entranceItems.Length; i++)
            {
                entranceItems[i].localScale = Vector3.zero;
                entrance.Group(Tween.Scale(entranceItems[i], 0f, 1f, duration, Ease.OutBack,
                    startDelay: UiMotion.Time(i * entranceStagger)));
            }
        }
    }
}
