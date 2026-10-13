using System;
using PrimeTween;
using StickerPool.Data;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace StickerPool.View
{
    /// <summary>
    /// Top bar (settings, level, moves), progress pill, hint bubble and the Swap-words button.
    /// Pure display: it reports taps, it never decides.
    /// </summary>
    public sealed class HudView : MonoBehaviour
    {
        [SerializeField] UiTheme theme;

        [Header("Top bar")]
        [SerializeField] Button settingsButton;
        [SerializeField] LocalizedLabel levelLabel;
        [SerializeField] Image[] levelStars;
        [SerializeField] Color starOn = new Color32(0xF2, 0xB5, 0x44, 0xFF);
        [SerializeField] Color starOff = new Color32(0xD9, 0xCB, 0xB8, 0xFF);

        [Header("Moves")]
        [SerializeField] RectTransform movesPill;
        [SerializeField] Image movesPillBackground;
        [SerializeField] TMP_Text movesValue;
        [SerializeField] Image[] movesPips;
        [SerializeField] Color movesColor = new Color32(0x3B, 0x2F, 0x2A, 0xFF);
        [SerializeField] Color movesLowColor = Color.white;
        [SerializeField] Color pillColor = new Color32(0xFF, 0xFD, 0xF8, 0xFF);
        [SerializeField] Color pillLowColor = new Color32(0xEE, 0x7B, 0x6C, 0xFF);
        [SerializeField] Color pipOn = new Color32(0xF2, 0xB5, 0x44, 0xFF);
        [SerializeField] Color pipLow = Color.white;
        [SerializeField] Color pipOff = new Color32(0xD9, 0xCB, 0xB8, 0xFF);

        [Header("Progress")]
        [Tooltip("Stretches from the left edge of the track to the share of stickers already swept.")]
        [SerializeField] RectTransform progressFill;
        [SerializeField] LocalizedLabel progressLabel;
        [SerializeField] float progressDuration = 0.45f;

        [Header("Hint bubble")]
        [SerializeField] RectTransform infoBubble;
        [SerializeField] LocalizedLabel infoLabel;
        [SerializeField] LocalizedString rerolledText;
        [SerializeField] LocalizedString flewOutText;

        [Header("Swap words")]
        [SerializeField] Button rerollButton;
        [SerializeField] CanvasGroup rerollGroup;
        [SerializeField] TMP_Text rerollBadge;
        [SerializeField, Range(0f, 1f)] float disabledAlpha = 0.45f;

        [Tooltip("The tapes container; it drops into the freed space while Swap words is hidden.")]
        [SerializeField] RectTransform tapes;
        [SerializeField] float tapesYWithoutSwap = 200f;

        [Header("Tutorial")]
        [SerializeField] TutorialHandView tutorialHand;

        bool rerollAvailable;
        bool swapYKnown;
        float tapesYWithSwap;
        float progress;
        Tween movesTween;
        Tween pulseTween;
        Tween progressTween;
        Tween infoTween;

        public event Action SettingsClicked;
        public event Action RerollClicked;

        void Awake()
        {
            settingsButton.onClick.AddListener(() => SettingsClicked?.Invoke());
            rerollButton.onClick.AddListener(() => RerollClicked?.Invoke());
        }

        void OnDestroy()
        {
            settingsButton.onClick.RemoveAllListeners();
            rerollButton.onClick.RemoveAllListeners();
        }

        public void SetLevel(int levelNumber, int bestStars)
        {
            levelLabel.SetArguments(levelNumber);
            for (int i = 0; i < levelStars.Length; i++)
                levelStars[i].color = i < bestStars ? starOn : starOff;
        }

        public void SetMoves(int movesLeft, int budget, bool animate)
        {
            movesValue.text = movesLeft.ToString();
            bool low = movesLeft <= 1;
            movesValue.color = low ? movesLowColor : movesColor;
            movesPillBackground.color = low ? pillLowColor : pillColor;

            for (int i = 0; i < movesPips.Length; i++)
            {
                movesPips[i].gameObject.SetActive(i < budget);
                movesPips[i].color = i < movesLeft ? (low ? pipLow : pipOn) : pipOff;
            }

            PulseWhenLow(low && movesLeft > 0);
            if (!animate) return;
            movesTween.Stop();
            movesValue.transform.localScale = Vector3.one;
            movesTween = Tween.PunchScale(movesValue.transform, new Vector3(0.25f, 0.25f, 0f), UiMotion.Time(0.3f), frequency: 2);
        }

        /// <summary>Fills the progress pill: <paramref name="total"/> stickers at the start of the level.</summary>
        public void SetStickers(int left, int total, bool animate)
        {
            progressLabel.SetArguments(left);
            float target = total <= 0 ? 0f : 1f - (float)left / total;
            progressTween.Stop();
            if (!animate || UiMotion.Reduced)
            {
                ApplyProgress(target);
                return;
            }

            progressTween = Tween.Custom(progress, target, UiMotion.Time(progressDuration),
                onValueChange: ApplyProgress, ease: Ease.OutCubic);
        }

        /// <summary>Shows or hides the Swap words button; the tapes move down when it is hidden.</summary>
        public void SetSwapFeature(bool enabled)
        {
            if (!swapYKnown)
            {
                tapesYWithSwap = tapes.anchoredPosition.y;
                swapYKnown = true;
            }

            rerollButton.gameObject.SetActive(enabled);
            tapes.anchoredPosition = new Vector2(tapes.anchoredPosition.x, enabled ? tapesYWithSwap : tapesYWithoutSwap);
        }

        public void SetRerollsLeft(int count, bool available)
        {
            rerollAvailable = available;
            rerollBadge.text = count.ToString();
            ApplyRerollState(true);
        }

        /// <summary>Locks the Swap button while stickers are resolving.</summary>
        public void SetBusy(bool busy) => ApplyRerollState(!busy);

        public void ShowInfo(LocalizedString entry, params object[] args)
        {
            infoLabel.SetKey(entry, args);
            PunchInfo();
        }

        public void ShowInfoText(string richText)
        {
            infoLabel.SetText(richText);
            PunchInfo();
        }

        public void ShowRerolled()
        {
            infoLabel.SetKey(rerolledText);
            PunchInfo();
        }

        public void ShowFlewOut(string tagName, int count)
        {
            infoLabel.SetKey(flewOutText, tagName, count);
            PunchInfo();
        }

        /// <summary>Points a hand at the target until <see cref="HideTutorial"/>.</summary>
        public void ShowTutorial(Transform target, float delay) => tutorialHand.Show(target, delay);

        public void HideTutorial() => tutorialHand.Hide();

        void ApplyProgress(float value)
        {
            progress = value;
            var max = progressFill.anchorMax;
            max.x = Mathf.Clamp01(value);
            progressFill.anchorMax = max;
        }

        void ApplyRerollState(bool notBusy)
        {
            bool usable = rerollAvailable && notBusy;
            rerollButton.interactable = usable;
            rerollGroup.alpha = rerollAvailable ? (notBusy ? 1f : disabledAlpha) : disabledAlpha;
        }

        void PulseWhenLow(bool low)
        {
            pulseTween.Stop();
            movesPill.localScale = Vector3.one;
            if (!low || UiMotion.Reduced) return;

            float amount = theme.lowMovesPulseScale;
            pulseTween = Tween.Custom(0f, 1f, theme.lowMovesPulseDuration,
                onValueChange: t => movesPill.localScale = Vector3.one * (1f + amount * Mathf.Sin(t * Mathf.PI * 2f)),
                ease: Ease.Linear, cycles: -1, cycleMode: CycleMode.Restart);
        }

        void PunchInfo()
        {
            infoTween.Stop();
            infoBubble.localScale = Vector3.one;
            infoTween = Tween.PunchScale(infoBubble, new Vector3(0.05f, 0.05f, 0f), UiMotion.Time(0.25f), frequency: 2);
        }
    }
}
