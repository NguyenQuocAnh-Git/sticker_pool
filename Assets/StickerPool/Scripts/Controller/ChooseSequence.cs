using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using StickerPool.Data;
using StickerPool.View;
using UnityEngine;

namespace StickerPool.Controller
{
    /// <summary>
    /// The choreography after a word is picked: matching stickers lift into a numbered line,
    /// hold, then fly into the chosen tape one by one (GDD section 8).
    /// Durations go through <see cref="UiMotion"/> so "reduce motion" shortens them.
    /// </summary>
    public sealed class ChooseSequence
    {
        readonly GameConfig config;
        readonly JarView jar;
        readonly RollCallView rollCall;
        readonly SfxPlayer sfx;
        readonly BurstView popBurst;

        public ChooseSequence(GameConfig config, JarView jar, RollCallView rollCall, SfxPlayer sfx, BurstView popBurst)
        {
            this.config = config;
            this.jar = jar;
            this.rollCall = rollCall;
            this.sfx = sfx;
            this.popBurst = popBurst;
        }

        struct Slot
        {
            public Vector3 position;
            public float scale;
        }

        /// <param name="onLineComplete">Raised when every sticker is numbered, before the hold.</param>
        public IEnumerator Run(TagData tag, IReadOnlyList<StickerView> views, TapeView tape, string tagText,
            string praise, Action onLineComplete)
        {
            int count = views.Count;
            var slots = BuildSlots(count, out float rowScale, out Vector3 titlePosition);

            rollCall.Begin(tagText, titlePosition);
            jar.FadeDim(config.dimAlpha, T(config.dimFadeDuration));

            for (int i = 0; i < count; i++)
            {
                LiftToSlot(views[i], slots[i], T(i * config.liftStagger));
                ScheduleCount(tag, views[i], slots[i], i, tagText, rowScale);
            }

            yield return new WaitForSeconds(T((count - 1) * config.liftStagger + config.liftDuration + 0.06f));

            rollCall.MarkDone($"{tagText} · {count}{praise}");
            onLineComplete?.Invoke();
            yield return new WaitForSeconds(T(config.holdAfterCount));

            FlyToTape(views, slots, tape);
            yield return new WaitForSeconds(T(config.flyDuration + count * config.flyStagger + 0.06f));

            tape.PlayBounce(T(config.tapeBounceDuration));
        }

        Slot[] BuildSlots(int count, out float rowScale, out Vector3 titlePosition)
        {
            int rows = Mathf.CeilToInt(count / (float)config.maxPerRow);
            int columns = Mathf.CeilToInt(count / (float)rows);
            float cell = jar.CellSize;
            float size = Mathf.Min(cell * config.maxRowScale,
                (jar.Size.x - config.rowSideMargin * cell) / columns - config.rowGap * cell);
            float rowSpacing = size + config.rowExtraSpacing * cell;
            float firstRowY = jar.Center.y + (rows - 1) * rowSpacing * 0.5f;

            rowScale = size / cell;
            titlePosition = new Vector3(jar.Center.x, firstRowY + size * 0.5f + config.rollTitleOffset * cell, 0f);

            var slots = new Slot[count];
            for (int i = 0; i < count; i++)
            {
                int row = i / columns;
                int inRow = Mathf.Min(columns, count - row * columns);
                int column = i - row * columns;
                float x = jar.Center.x + (column - (inRow - 1) * 0.5f) * (size + config.rowGap * cell);
                slots[i] = new Slot { position = new Vector3(x, firstRowY - row * rowSpacing, 0f), scale = rowScale };
            }

            return slots;
        }

        void LiftToSlot(StickerView view, Slot slot, float delay)
        {
            var t = view.transform;
            Vector3 start = t.position;
            float startRotation = view.Body.rotation;
            view.EnterFlight();

            Vector3 mid = start + (slot.position - start) * 0.45f + Vector3.up * config.liftArcHeight;
            float midScale = slot.scale * config.liftMidScale;
            float midOffset = Mathf.Max(0.01f, config.liftMidOffset);

            Tween.Custom(0f, 1f, T(config.liftDuration), onValueChange: p =>
            {
                if (p < midOffset)
                {
                    float f = p / midOffset;
                    Apply(t, Vector3.LerpUnclamped(start, mid, f),
                        Mathf.LerpUnclamped(startRotation, startRotation * 0.4f, f),
                        Mathf.LerpUnclamped(1f, midScale, f));
                }
                else
                {
                    float f = (p - midOffset) / (1f - midOffset);
                    Apply(t, Vector3.LerpUnclamped(mid, slot.position, f),
                        Mathf.LerpUnclamped(startRotation * 0.4f, 0f, f),
                        Mathf.LerpUnclamped(midScale, slot.scale, f));
                }
            }, ease: config.liftEase, startDelay: delay);
        }

        void ScheduleCount(TagData tag, StickerView view, Slot slot, int index, string tagText, float rowScale)
        {
            float delay = T(index * config.liftStagger + config.liftDuration * config.countAtFraction);
            Tween.Delay(delay, () =>
            {
                var badgePosition = slot.position + Vector3.down * (rowScale * jar.CellSize * 0.5f + config.badgeDrop * jar.CellSize);
                rollCall.ShowBadge(index, badgePosition, index + 1);
                rollCall.SetTitle($"{tagText} · {index + 1}");
                sfx.PlayCount(tag.RollCallClip, index);
                Haptics.Play(HapticKind.Light);

                var t = view.transform;
                Tween.Custom(0f, 1f, T(config.countPunchDuration), onValueChange: p =>
                    t.localScale = Vector3.one * (slot.scale * (1f + 0.18f * Mathf.Sin(p * Mathf.PI))), ease: Ease.Linear);
            });
        }

        void FlyToTape(IReadOnlyList<StickerView> views, Slot[] slots, TapeView tape)
        {
            jar.FadeDim(0f, T(config.dimFadeDuration));
            rollCall.FadeOut(T(0.2f), T(0.25f));

            Vector3 target = tape.AnchorWorld;
            target.z = 0f;
            float endRotation = config.flyEndRotation * Mathf.Rad2Deg;

            for (int i = 0; i < views.Count; i++)
            {
                var view = views[i];
                var t = view.transform;
                var slot = slots[i];
                float delay = T(i * config.flyStagger);
                float flyDuration = T(config.flyDuration);

                Tween.Custom(0f, 1f, flyDuration, onValueChange: p =>
                {
                    Apply(t, Vector3.LerpUnclamped(slot.position, target, p),
                        Mathf.LerpUnclamped(0f, endRotation, p),
                        Mathf.LerpUnclamped(slot.scale, slot.scale * config.flyEndScale, p));
                    view.SetAlpha(Mathf.LerpUnclamped(1f, config.flyEndAlpha, p));
                }, ease: config.flyEase, startDelay: delay);

                Tween.Delay(delay + flyDuration - config.popLead, () =>
                {
                    sfx.PlayPop();
                    Haptics.Play(HapticKind.Soft);
                    popBurst.EmitAt(target);
                });
            }
        }

        static float T(float seconds) => UiMotion.Time(seconds);

        static void Apply(Transform t, Vector3 position, float rotationDegrees, float scale)
        {
            t.SetPositionAndRotation(position, Quaternion.Euler(0f, 0f, rotationDegrees));
            t.localScale = Vector3.one * scale;
        }
    }
}
