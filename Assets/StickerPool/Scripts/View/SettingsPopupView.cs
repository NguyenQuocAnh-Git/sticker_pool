using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StickerPool.View
{
    /// <summary>Sound, haptics, reduced motion, language; plus Restart and Home while a level is being played.</summary>
    public sealed class SettingsPopupView : PopupView
    {
        [SerializeField] UiToggle soundToggle;
        [SerializeField] UiToggle hapticsToggle;
        [SerializeField] UiToggle reducedMotionToggle;
        [SerializeField] Button languageButton;
        [SerializeField] TMP_Text languageValue;
        [SerializeField] Button restartButton;
        [SerializeField] Button homeButton;
        [Tooltip("Rows that only make sense during a level (Restart, Home).")]
        [SerializeField] GameObject inGameRows;

        public event Action SoundToggled;
        public event Action HapticsToggled;
        public event Action ReducedMotionToggled;
        public event Action LanguageClicked;
        public event Action RestartClicked;
        public event Action HomeClicked;

        protected override void Awake()
        {
            base.Awake();
            soundToggle.Toggled += () => SoundToggled?.Invoke();
            hapticsToggle.Toggled += () => HapticsToggled?.Invoke();
            reducedMotionToggle.Toggled += () => ReducedMotionToggled?.Invoke();
            languageButton.onClick.AddListener(() => LanguageClicked?.Invoke());
            restartButton.onClick.AddListener(() => RestartClicked?.Invoke());
            homeButton.onClick.AddListener(() => HomeClicked?.Invoke());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            languageButton.onClick.RemoveAllListeners();
            restartButton.onClick.RemoveAllListeners();
            homeButton.onClick.RemoveAllListeners();
        }

        /// <summary>Shows the level-only rows (Restart, Home) when a level is on screen.</summary>
        public void SetInGame(bool inGame) => inGameRows.SetActive(inGame);

        public void SetSound(bool on, bool animate = true) => soundToggle.Set(on, animate);

        public void SetHaptics(bool on, bool animate = true) => hapticsToggle.Set(on, animate);

        public void SetReducedMotion(bool on, bool animate = true) => reducedMotionToggle.Set(on, animate);

        public void SetLanguageCode(string code) => languageValue.text = code.ToUpperInvariant();
    }
}
