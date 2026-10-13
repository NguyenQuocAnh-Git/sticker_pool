using UnityEngine;

namespace StickerPool.Data
{
    /// <summary>Every tunable number of game feel. Defaults come from GDD sections 7-9.</summary>
    [CreateAssetMenu(menuName = "Sticker Pool/Game Config", fileName = "GameConfig")]
    public sealed class GameConfig : ScriptableObject
    {
        [Header("Progress")]
        [Tooltip("Playtest build: every level is selectable. Turn off to unlock levels by clearing them.")]
        public bool unlockAllLevels = true;

        [Header("Spawn (world units; 1 unit = one sticker cell)")]
        public int spawnStickersPerLongRow = 5;
        public int spawnStickersPerShortRow = 4;
        public float spawnRowSpacing = 0.8f;
        public float spawnHeightAboveJar = 0.45f;
        public float spawnJitter = 0.16f;
        public float spawnStaggerHeight = 0.13f;

        [Header("Poke (tap a sticker in the jar)")]
        public float pokeUpSpeed = 10.8f;
        public float pokeSideSpeed = 3f;

        [Header("Roll call: lift to the line")]
        public float liftDuration = 0.47f;
        public float liftStagger = 0.075f;
        [Range(0f, 1f)] public float countAtFraction = 0.9f;
        public float countPunchDuration = 0.2f;
        public float holdAfterCount = 0.48f;
        public int maxPerRow = 6;
        public float maxRowScale = 1.3f;
        public float rowGap = 0.1f;
        public float rowSideMargin = 0.24f;
        public float rowExtraSpacing = 0.3f;
        public float rollTitleOffset = 0.62f;
        public float badgeDrop = 0.18f;
        public float liftArcHeight = 0.43f;
        public float liftMidScale = 1.12f;
        [Range(0f, 1f)] public float liftMidOffset = 0.55f;
        public AnimationCurve liftEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Roll call: fly into the tape")]
        public float flyDuration = 0.48f;
        public float flyStagger = 0.045f;
        public float flyEndScale = 0.3f;
        public float flyEndRotation = 0.8f;
        public float flyEndAlpha = 0.2f;
        public float popLead = 0.01f;
        public AnimationCurve flyEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public float tapeBounceDuration = 0.26f;
        public float settleDelay = 0.2f;

        [Header("Tape entrance")]
        [Tooltip("Pause before the first tapes of a level appear, so the stickers start falling first.")]
        public float tapeFirstDelay = 0.35f;
        [Tooltip("Gap between the three tapes appearing one after another.")]
        public float tapeStagger = 0.09f;
        [Tooltip("How long the picked tape (or a tape with nothing left to sweep) takes to peel away.")]
        public float tapeDisappearDuration = 0.25f;
        [Tooltip("Sound of a tape sticking on. Its tack lands about 0.13 s into the clip.")]
        public AudioClip tapeStickClip;
        [Tooltip("Seconds after the tape starts appearing when the clip should start.")]
        public float tapeSoundOffset = 0.09f;
        [Tooltip("Pitch of the 1st, 2nd and 3rd tape of a batch, in semitones (a rising little arpeggio).")]
        public int[] tapeStickSemitones = { 0, 3, 7 };

        [Header("Dim")]
        [Range(0f, 1f)] public float dimAlpha = 0.62f;
        public float dimFadeDuration = 0.3f;

        [Header("Praise thresholds")]
        public int greatCount = 4;
        public int awesomeCount = 6;

        [Header("Audio")]
        public AudioClip popClip;
        public AudioClip winClip;
        [Range(0f, 0.5f)] public float popPitchVariance = 0.15f;
        [Tooltip("Semitones above the base pitch for each count (pentatonic); the last value repeats.")]
        public int[] countSemitones = { 0, 2, 4, 7, 9, 12, 14, 16, 19 };
    }
}
