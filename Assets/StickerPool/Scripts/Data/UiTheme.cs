using System;
using UnityEngine;

namespace StickerPool.Data
{
    /// <summary>Which colour of the <see cref="UiTheme"/> palette a tinted element uses.</summary>
    public enum ThemeRole
    {
        Desk,
        DeskShade,
        Paper,
        PaperShade,
        Ink,
        InkSoft,
        Honey,
        Coral,
        Sage,
        White,
        Dim,
    }

    /// <summary>
    /// Design tokens of the warm "craft desk" look: palette, touch size, press and popup feel.
    /// Tinted elements (<c>UiThemeTint</c>) follow it live in the Editor.
    /// </summary>
    [CreateAssetMenu(menuName = "Sticker Pool/Ui Theme", fileName = "UiTheme")]
    public sealed class UiTheme : ScriptableObject
    {
        [Header("Palette")]
        public Color desk = new Color32(0xF6, 0xEB, 0xDD, 0xFF);
        public Color deskShade = new Color32(0xE8, 0xD3, 0xB5, 0xFF);
        public Color paper = new Color32(0xFF, 0xFD, 0xF8, 0xFF);
        public Color paperShade = new Color32(0xF1, 0xE6, 0xD6, 0xFF);
        public Color ink = new Color32(0x3B, 0x2F, 0x2A, 0xFF);
        public Color inkSoft = new Color32(0x7A, 0x6A, 0x60, 0xFF);
        public Color honey = new Color32(0xF2, 0xB5, 0x44, 0xFF);
        public Color coral = new Color32(0xEE, 0x7B, 0x6C, 0xFF);
        public Color sage = new Color32(0x7F, 0xB5, 0x9A, 0xFF);
        public Color white = Color.white;
        [Tooltip("Dark overlay colour used behind popups and while stickers are counted.")]
        public Color dim = new Color32(0x2A, 0x1F, 0x1A, 0xFF);

        [Header("Touch (canvas px at the 1080 reference; 1 pt is about 2.77 px)")]
        [Tooltip("Smallest tappable size. 128 px is about 46 pt, above Apple's 44 pt.")]
        public float minHitSize = 128f;
        [Tooltip("Smallest text size; 32 px is about 11.5 pt.")]
        public float minTextSize = 32f;

        [Header("Button press")]
        [Range(0.8f, 1f)] public float pressedScale = 0.94f;
        public float pressDuration = 0.08f;
        public float releaseDuration = 0.18f;

        [Header("Popup")]
        public float popupOpenDuration = 0.3f;
        public float popupCloseDuration = 0.18f;
        [Range(0.5f, 1f)] public float popupOpenScale = 0.85f;
        [Range(0.5f, 1f)] public float popupCloseScale = 0.92f;
        [Range(0f, 1f)] public float popupDimAlpha = 0.55f;

        [Header("Screen transition (paper curtain)")]
        public float curtainCoverDuration = 0.38f;
        public float curtainRevealDuration = 0.42f;

        [Header("Moves counter")]
        public float lowMovesPulseDuration = 0.8f;
        [Range(0f, 0.2f)] public float lowMovesPulseScale = 0.06f;

        public static event Action Changed;

        public Color Get(ThemeRole role)
        {
            switch (role)
            {
                case ThemeRole.Desk: return desk;
                case ThemeRole.DeskShade: return deskShade;
                case ThemeRole.Paper: return paper;
                case ThemeRole.PaperShade: return paperShade;
                case ThemeRole.Ink: return ink;
                case ThemeRole.InkSoft: return inkSoft;
                case ThemeRole.Honey: return honey;
                case ThemeRole.Coral: return coral;
                case ThemeRole.Sage: return sage;
                case ThemeRole.Dim: return dim;
                default: return white;
            }
        }

        void OnValidate() => Changed?.Invoke();
    }
}
