using System.Collections.Generic;

namespace StickerPool.Model
{
    /// <summary>A sticker type: its stable id and every tag it carries (colour first).</summary>
    public sealed class StickerDef
    {
        readonly HashSet<string> tagSet;

        public StickerDef(string id, string colorTag, IReadOnlyList<string> tags)
        {
            Id = id;
            ColorTag = colorTag;
            Tags = tags;
            tagSet = new HashSet<string>(tags);
        }

        public string Id { get; }
        public string ColorTag { get; }
        public IReadOnlyList<string> Tags { get; }

        public bool HasTag(string tagId) => tagSet.Contains(tagId);
    }

    /// <summary>Everything the rules need to know about one level.</summary>
    public sealed class LevelDef
    {
        public LevelDef(int number, int stickerCount, int colorCount, IReadOnlyList<string> offerableTags,
            string focusTag, int spareMoves, int seed)
        {
            Number = number;
            StickerCount = stickerCount;
            ColorCount = colorCount;
            OfferableTags = offerableTags;
            FocusTag = string.IsNullOrEmpty(focusTag) ? null : focusTag;
            SpareMoves = spareMoves;
            Seed = seed;
        }

        public int Number { get; }
        public int StickerCount { get; }
        public int ColorCount { get; }
        public IReadOnlyList<string> OfferableTags { get; }
        public string FocusTag { get; }
        public int SpareMoves { get; }
        public int Seed { get; }

        public bool CanOffer(string tagId)
        {
            for (int i = 0; i < OfferableTags.Count; i++)
                if (OfferableTags[i] == tagId) return true;
            return false;
        }
    }

    /// <summary>One sticker instance in a generated jar.</summary>
    public sealed class PlacedSticker
    {
        public PlacedSticker(int id, StickerDef def, double x, double rotation)
        {
            Id = id;
            Def = def;
            X = x;
            Rotation = rotation;
        }

        public int Id { get; }
        public StickerDef Def { get; }

        /// <summary>0..1 random value, used to jitter the spawn position.</summary>
        public double X { get; }

        /// <summary>Initial angle in radians.</summary>
        public double Rotation { get; }
    }
}
