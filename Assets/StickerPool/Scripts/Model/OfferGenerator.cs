using System;
using System.Collections.Generic;
using System.Linq;

namespace StickerPool.Model
{
    /// <summary>
    /// Picks the three words offered each turn: always one big sweep, the focus word when it is
    /// on the table, the rest random (port of the prototype's makeOffers).
    /// </summary>
    public static class OfferGenerator
    {
        public static List<string> Make(LevelDef level, IReadOnlyList<PlacedSticker> stickers,
            IReadOnlyCollection<int> alive, int reroll, uint run)
        {
            var rnd = new Mulberry32(SeedFor(level, alive, reroll, run));

            var candidates = new List<(string tag, int hits)>();
            foreach (var tag in level.OfferableTags)
            {
                int hits = CountHits(stickers, alive, tag);
                if (hits > 0) candidates.Add((tag, hits));
            }

            var good = candidates.Where(c => c.hits >= 2).ToList();
            var list = (good.Count >= 3 ? good : candidates).ToList();
            Mulberry32.Shuffle(list, rnd);

            var offers = new List<string>(3);
            var best = list.OrderByDescending(c => c.hits).ToList();
            if (best.Count > 0)
            {
                var top = best.Where(c => c.hits >= best[0].hits - 1).ToList();
                if (top.Count > 0)
                    offers.Add(top[(int)Math.Floor(rnd.Next() * top.Count)].tag);
            }

            if (level.FocusTag != null
                && list.Any(c => c.tag == level.FocusTag && c.hits >= 2)
                && !offers.Contains(level.FocusTag))
                offers.Add(level.FocusTag);

            foreach (var c in list)
            {
                if (offers.Count >= 3) break;
                if (!offers.Contains(c.tag)) offers.Add(c.tag);
            }

            Mulberry32.Shuffle(offers, rnd);
            return offers;
        }

        public const int SlotCount = 3;

        /// <summary>
        /// Refills the tape slots after a turn without disturbing the tapes that are still useful.
        /// Words that still match a sticker stay in their slot; empty slots (the word just picked, or a word
        /// with nothing left to sweep) get a new word. The newcomer is the biggest sweep of the fresh offers
        /// unless a kept word already sweeps as much, so a turn that sweeps a lot is always possible.
        /// </summary>
        /// <param name="kept">Current word per slot, null where the slot is free. Length <see cref="SlotCount"/>.</param>
        public static string[] Refill(LevelDef level, IReadOnlyList<PlacedSticker> stickers,
            IReadOnlyCollection<int> alive, int reroll, uint run, IReadOnlyList<string> kept)
        {
            var slots = new string[SlotCount];
            for (int i = 0; i < SlotCount && i < kept.Count; i++)
                slots[i] = kept[i] != null && CountHits(stickers, alive, kept[i]) > 0 ? kept[i] : null;

            var fresh = Make(level, stickers, alive, reroll, run);

            string best = null;
            int bestHits = 0;
            foreach (var tag in fresh)
            {
                int hits = CountHits(stickers, alive, tag);
                if (hits > bestHits)
                {
                    best = tag;
                    bestHits = hits;
                }
            }

            int keptBest = 0;
            foreach (var tag in slots)
                if (tag != null) keptBest = Math.Max(keptBest, CountHits(stickers, alive, tag));

            var queue = new List<string>();
            if (best != null && keptBest < bestHits) queue.Add(best);

            if (level.FocusTag != null && !queue.Contains(level.FocusTag)
                && Array.IndexOf(slots, level.FocusTag) < 0
                && CountHits(stickers, alive, level.FocusTag) >= 2)
                queue.Add(level.FocusTag);

            foreach (var tag in fresh)
                if (!queue.Contains(tag)) queue.Add(tag);

            var fallback = level.OfferableTags
                .Select(t => (tag: t, hits: CountHits(stickers, alive, t)))
                .Where(x => x.hits > 0)
                .OrderByDescending(x => x.hits)
                .Select(x => x.tag);
            foreach (var tag in fallback)
                if (!queue.Contains(tag)) queue.Add(tag);

            int next = 0;
            for (int i = 0; i < SlotCount; i++)
            {
                if (slots[i] != null) continue;
                while (next < queue.Count && Array.IndexOf(slots, queue[next]) >= 0) next++;
                if (next >= queue.Count) break;
                slots[i] = queue[next++];
            }

            return slots;
        }

        /// <summary>Ids of the alive stickers that carry the tag, in ascending id order.</summary>
        public static List<int> HitsFor(IReadOnlyList<PlacedSticker> stickers, IEnumerable<int> alive, string tag)
        {
            var hits = new List<int>();
            foreach (int id in alive)
                if (stickers[id].Def.HasTag(tag)) hits.Add(id);
            return hits;
        }

        static int CountHits(IReadOnlyList<PlacedSticker> stickers, IEnumerable<int> alive, string tag)
        {
            int n = 0;
            foreach (int id in alive)
                if (stickers[id].Def.HasTag(tag)) n++;
            return n;
        }

        // The prototype computes this in IEEE doubles (the product exceeds 2^53 and loses low bits),
        // so the double arithmetic is reproduced on purpose.
        static uint SeedFor(LevelDef level, IEnumerable<int> alive, int reroll, uint run)
        {
            double value = level.Seed * 7919.0 + HashIds(alive) + reroll * 104729.0 + (double)run * 2654435761.0;
            return (uint)(value % 4294967296.0);
        }

        static uint HashIds(IEnumerable<int> alive)
        {
            uint h = 2166136261;
            foreach (int id in alive.OrderBy(i => i))
            {
                unchecked
                {
                    h ^= (uint)(id + 1);
                    h *= 16777619;
                }
            }

            return h;
        }
    }
}
