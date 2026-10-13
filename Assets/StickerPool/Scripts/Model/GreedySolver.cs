using System.Collections.Generic;

namespace StickerPool.Model
{
    /// <summary>Machine solution: every turn take the offered word that sweeps the most stickers.</summary>
    public static class GreedySolver
    {
        const int MoveLimit = 200;

        public static int CountMoves(LevelDef level, IReadOnlyList<PlacedSticker> stickers, uint run)
        {
            var alive = new SortedSet<int>();
            foreach (var s in stickers) alive.Add(s.Id);

            int moves = 0;
            while (alive.Count > 0 && moves < MoveLimit)
            {
                List<int> best = null;
                foreach (var tag in OfferGenerator.Make(level, stickers, alive, 0, run))
                {
                    var hits = OfferGenerator.HitsFor(stickers, alive, tag);
                    if (best == null || hits.Count > best.Count) best = hits;
                }

                if (best == null) break;
                foreach (int id in best) alive.Remove(id);
                moves++;
            }

            return moves;
        }
    }
}
