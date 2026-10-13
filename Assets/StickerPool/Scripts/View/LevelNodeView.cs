using System;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StickerPool.View
{
    /// <summary>One round sticker on the level map: locked, open (current) or cleared with 1-3 stars.</summary>
    public sealed class LevelNodeView : MonoBehaviour
    {
        [SerializeField] Button button;
        [SerializeField] Image disc;
        [SerializeField] Image halo;
        [SerializeField] TMP_Text number;
        [SerializeField] Image lockIcon;
        [SerializeField] GameObject starsRow;
        [SerializeField] Image[] stars;
        [SerializeField] Color lockedColor = new Color32(0xD9, 0xCB, 0xB8, 0xFF);
        [SerializeField] Color openColor = new Color32(0xF2, 0xB5, 0x44, 0xFF);
        [SerializeField] Color clearedColor = new Color32(0x7F, 0xB5, 0x9A, 0xFF);
        [SerializeField] Color starOn = new Color32(0xF2, 0xB5, 0x44, 0xFF);
        [SerializeField] Color starOff = new Color32(0xD9, 0xCB, 0xB8, 0xFF);

        int index;
        Tween haloTween;

        public event Action<int> Clicked;

        void Awake() => button.onClick.AddListener(() => Clicked?.Invoke(index));

        void OnDestroy() => button.onClick.RemoveAllListeners();

        void OnDisable() => haloTween.Stop();

        /// <param name="starCount">Best result, 0 when the level was never cleared.</param>
        public void Setup(int levelIndex, int starCount, bool current, bool unlocked)
        {
            index = levelIndex;
            number.text = (levelIndex + 1).ToString();
            bool cleared = starCount > 0;

            disc.color = !unlocked ? lockedColor : cleared ? clearedColor : openColor;
            number.gameObject.SetActive(unlocked);
            lockIcon.gameObject.SetActive(!unlocked);
            button.interactable = unlocked;

            starsRow.SetActive(cleared);
            for (int i = 0; i < stars.Length; i++)
                stars[i].color = i < starCount ? starOn : starOff;

            StartHalo(current && unlocked);
        }

        void StartHalo(bool show)
        {
            haloTween.Stop();
            halo.gameObject.SetActive(show);
            if (!show) return;

            var rect = halo.rectTransform;
            var color = halo.color;
            rect.localScale = Vector3.one;
            if (UiMotion.Reduced)
            {
                color.a = 0.5f;
                halo.color = color;
                return;
            }

            haloTween = Tween.Custom(0f, 1f, 1.3f, onValueChange: t =>
            {
                rect.localScale = Vector3.one * Mathf.Lerp(1f, 1.35f, t);
                color.a = Mathf.Lerp(0.6f, 0f, t);
                halo.color = color;
            }, ease: Ease.OutQuad, cycles: -1, cycleMode: CycleMode.Restart);
        }
    }
}
