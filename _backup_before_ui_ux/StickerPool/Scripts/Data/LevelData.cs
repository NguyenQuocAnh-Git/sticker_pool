using System.Collections.Generic;
using StickerPool.Model;
using UnityEngine;
using UnityEngine.Localization;

namespace StickerPool.Data
{
    /// <summary>Design of one level. Add a new asset to <see cref="GameDatabase"/> to add a level.</summary>
    [CreateAssetMenu(menuName = "Sticker Pool/Level", fileName = "Level")]
    public sealed class LevelData : ScriptableObject
    {
        [SerializeField, Min(1)] int stickerCount = 8;
        [SerializeField, Min(1)] int colorCount = 3;
        [Tooltip("Words that may appear on the tapes in this level, in priority order.")]
        [SerializeField] List<TagData> offerableTags = new List<TagData>();
        [Tooltip("Optional funny word the level is built around.")]
        [SerializeField] TagData focusTag;
        [SerializeField, Min(0)] int spareMoves = 2;
        [Tooltip("Fixes which stickers appear. Each replay still reshuffles layout and word offers.")]
        [SerializeField] int seed = 11;
        [SerializeField] LocalizedString hint;

        public int StickerCount => stickerCount;
        public int ColorCount => colorCount;
        public IReadOnlyList<TagData> OfferableTags => offerableTags;
        public TagData FocusTag => focusTag;
        public int SpareMoves => spareMoves;
        public int Seed => seed;
        public LocalizedString Hint => hint;

        public LevelDef ToDef(int number)
        {
            var tags = new List<string>(offerableTags.Count);
            foreach (var tag in offerableTags) tags.Add(tag.Id);
            return new LevelDef(number, stickerCount, colorCount, tags,
                focusTag != null ? focusTag.Id : null, spareMoves, seed);
        }
    }
}
