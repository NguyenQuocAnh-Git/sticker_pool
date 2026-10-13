using System;
using System.Collections.Generic;

namespace StickerPool.Model
{
    /// <summary>
    /// Bit-exact port of the prototype's mulberry32 PRNG so the same seed yields the same sticker set.
    /// </summary>
    public sealed class Mulberry32
    {
        int state;

        public Mulberry32(uint seed)
        {
            state = unchecked((int)seed);
        }

        public double Next()
        {
            unchecked
            {
                state += 0x6D2B79F5;
                int t = (state ^ (int)((uint)state >> 15)) * (1 | state);
                t = (t + (t ^ (int)((uint)t >> 7)) * (61 | t)) ^ t;
                return (double)(uint)(t ^ (int)((uint)t >> 14)) / 4294967296.0;
            }
        }

        public static void Shuffle<T>(IList<T> items, Mulberry32 rnd)
        {
            for (int i = items.Count - 1; i > 0; i--)
            {
                int j = (int)Math.Floor(rnd.Next() * (i + 1));
                (items[i], items[j]) = (items[j], items[i]);
            }
        }
    }
}
