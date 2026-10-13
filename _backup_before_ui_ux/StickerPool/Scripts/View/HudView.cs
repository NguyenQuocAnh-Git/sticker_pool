using System;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace StickerPool.View
{
    /// <summary>Top bar, tool row and info line. Pure display: it reports clicks, it never decides.</summary>
    public sealed class HudView : MonoBehaviour
    {
        [Header("Counters")]
        [SerializeField] TMP_Text movesValue;
        [SerializeField] Color movesColor = new Color(0.949f, 0.933f, 0.882f);
        [SerializeField] Color movesLowColor = new Color(0.937f, 0.51f, 0.439f);
        [SerializeField] LocalizedLabel stickersLeftLabel;

        [Header("Info line")]
        [SerializeField] LocalizedLabel infoLabel;
        [SerializeField] LocalizedString rerolledText;
        [SerializeField] LocalizedString flewOutText;

        [Header("Levels")]
        [SerializeField] LevelDotView[] levelDots;

        [Header("Tools")]
        [SerializeField] Button soundButton;
        [SerializeField] Button rerollButton;
        [SerializeField] Button restartButton;
        [SerializeField] Button languageButton;
        [SerializeField] LocalizedLabel soundLabel;
        [SerializeField] LocalizedLabel rerollLabel;
        [SerializeField] TMP_Text languageLabel;
        [SerializeField] LocalizedString soundOnText;
        [SerializeField] LocalizedString soundOffText;

        bool rerollAvailable;
        Tween movesTween;

        public event Action SoundClicked;
        public event Action RerollClicked;
        public event Action RestartClicked;
        public event Action LanguageClicked;
        public event Action<int> LevelSelected;

        void Awake()
        {
            soundButton.onClick.AddListener(() => SoundClicked?.Invoke());
            rerollButton.onClick.AddListener(() => RerollClicked?.Invoke());
            restartButton.onClick.AddListener(() => RestartClicked?.Invoke());
            languageButton.onClick.AddListener(() => LanguageClicked?.Invoke());
            foreach (var dot in levelDots) dot.Clicked += index => LevelSelected?.Invoke(index);
        }

        void OnDestroy()
        {
            soundButton.onClick.RemoveAllListeners();
            rerollButton.onClick.RemoveAllListeners();
            restartButton.onClick.RemoveAllListeners();
            languageButton.onClick.RemoveAllListeners();
        }

        public void SetMoves(int movesLeft, bool animate)
        {
            movesValue.text = movesLeft.ToString();
            movesValue.color = movesLeft <= 1 ? movesLowColor : movesColor;
            if (!animate) return;
            movesTween.Stop();
            movesValue.transform.localScale = Vector3.one;
            movesTween = Tween.PunchScale(movesValue.transform, new Vector3(0.18f, 0.18f, 0f), 0.3f, frequency: 2);
        }

        public void SetStickersLeft(int count) => stickersLeftLabel.SetArguments(count);

        public void SetRerollsLeft(int count, bool available)
        {
            rerollAvailable = available;
            rerollLabel.SetArguments(count);
            rerollButton.interactable = available;
        }

        public void SetSound(bool on) => soundLabel.SetKey(on ? soundOnText : soundOffText);

        public void SetLanguageCode(string code) => languageLabel.text = code.ToUpperInvariant();

        public void SetLevels(int current, Func<int, bool> isCleared, int unlockedCount)
        {
            for (int i = 0; i < levelDots.Length; i++)
                levelDots[i].Setup(i, isCleared(i), i == current, i < unlockedCount);
        }

        /// <summary>Locks everything that could restart the level while stickers are resolving.</summary>
        public void SetBusy(bool busy, int unlockedCount)
        {
            restartButton.interactable = !busy;
            rerollButton.interactable = !busy && rerollAvailable;
            for (int i = 0; i < levelDots.Length; i++)
                levelDots[i].SetInteractable(!busy, i < unlockedCount);
        }

        public void ShowInfo(LocalizedString entry, params object[] args) => infoLabel.SetKey(entry, args);

        public void ShowInfoText(string richText) => infoLabel.SetText(richText);

        public void ShowRerolled() => infoLabel.SetKey(rerolledText);

        public void ShowFlewOut(string tagName, int count) => infoLabel.SetKey(flewOutText, tagName, count);
    }
}
