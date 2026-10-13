using System.Collections.Generic;
using System.Linq;

namespace StickerPool.Model
{
    /// <summary>
    /// Builds the sticker set of a level. The set is fixed by the level seed; each run seed
    /// reshuffles order, spawn jitter and angle (port of the prototype's genLevel).
    /// </summary>
    public static class LevelGenerator
    {
        public static List<PlacedSticker> Generate(LevelDef level, IReadOnlyList<StickerDef> items,
            IReadOnlyList<string> colorTags, uint run)
        {
            var rnd = new Mulberry32((uint)level.Seed);
            var runRnd = new Mulberry32(run);

            var colors = colorTags.ToList();
            Mulberry32.Shuffle(colors, rnd);
            colors = colors
                .OrderByDescending(c => FocusCount(level, items, c))
                .Take(level.ColorCount)
                .ToList();

            var scored = new List<(double score, StickerDef item)>();
            foreach (var item in items)
            {
                if (!colors.Contains(item.ColorTag)) continue;
                scored.Add((Score(level, item, rnd), item));
            }

            var pool = scored.OrderByDescending(s => s.score).Select(s => s.item).ToList();

            var picked = new List<StickerDef>(level.StickerCount);
            for (int i = 0; i < level.StickerCount; i++)
                picked.Add(pool[i % pool.Count]);
            Mulberry32.Shuffle(picked, runRnd);

            var result = new List<PlacedSticker>(picked.Count);
            for (int i = 0; i < picked.Count; i++)
            {
                double x = runRnd.Next();
                double rot = (runRnd.Next() - 0.5) * 2.4;
                result.Add(new PlacedSticker(i, picked[i], x, rot));
            }

            return result;
        }

        static int FocusCount(LevelDef level, IReadOnlyList<StickerDef> items, string color)
        {
            if (level.FocusTag == null) return 0;
            int count = 0;
            foreach (var it in items)
                if (it.ColorTag == color && it.HasTag(level.FocusTag)) count++;
            return count;
        }

        static double Score(LevelDef level, StickerDef item, Mulberry32 rnd)
        {
            double score = level.FocusTag != null && item.HasTag(level.FocusTag) ? 4 : 0;
            int open = 0;
            foreach (var tag in item.Tags)
                if (tag != item.ColorTag && level.CanOffer(tag)) open++;
            return score + open + rnd.Next() * 0.9;
        }
    }
}
