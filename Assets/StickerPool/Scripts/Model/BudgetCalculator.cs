using System.Collections.Generic;

namespace StickerPool.Model
{
    /// <summary>
    /// Fixed per-level move target: the median greedy solution over 41 sample shuffles
    /// (GDD 6.6). Results are cached per level seed.
    /// </summary>
    public sealed class BudgetCalculator
    {
        const int SampleCount = 41;

        readonly IReadOnlyList<StickerDef> items;
        readonly IReadOnlyList<string> colorTags;
        readonly Dictionary<int, int> cache = new Dictionary<int, int>();

        public BudgetCalculator(IReadOnlyList<StickerDef> items, IReadOnlyList<string> colorTags)
        {
            this.items = items;
            this.colorTags = colorTags;
        }

        public int SolveMoves(LevelDef level)
        {
            if (cache.TryGetValue(level.Seed, out int cached)) return cached;

            var results = new List<int>(SampleCount);
            for (int k = 0; k < SampleCount; k++)
            {
                uint run = SampleRun(k, level.Seed);
                var stickers = LevelGenerator.Generate(level, items, colorTags, run);
                results.Add(GreedySolver.CountMoves(level, stickers, run));
            }

            results.Sort();
            int median = results[SampleCount / 2];
            cache[level.Seed] = median;
            return median;
        }

        static uint SampleRun(int k, int seed)
        {
            unchecked
            {
                int mixed = (k + 1) * (int)2654435761u;
                return (uint)(mixed ^ seed);
            }
        }
    }
}
