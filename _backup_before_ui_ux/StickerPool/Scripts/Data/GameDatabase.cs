using System.Collections.Generic;
using StickerPool.Model;
using UnityEngine;

namespace StickerPool.Data
{
    /// <summary>
    /// Single entry point to all game content. Do not reorder stickers after levels are tuned,
    /// because level generation walks this list.
    /// </summary>
    [CreateAssetMenu(menuName = "Sticker Pool/Game Database", fileName = "GameDatabase")]
    public sealed class GameDatabase : ScriptableObject
    {
        [SerializeField] List<StickerData> stickers = new List<StickerData>();
        [Tooltip("Colour words in the order used by level generation.")]
        [SerializeField] List<TagData> colorTags = new List<TagData>();
        [SerializeField] List<TagData> allTags = new List<TagData>();
        [SerializeField] List<LevelData> levels = new List<LevelData>();

        Dictionary<string, TagData> tagsById;
        Dictionary<string, StickerData> stickersById;
        List<StickerDef> stickerDefs;
        List<string> colorTagIds;
        List<LevelDef> levelDefs;

        public IReadOnlyList<StickerData> Stickers => stickers;
        public IReadOnlyList<LevelData> Levels => levels;
        public int LevelCount => levels.Count;

        public IReadOnlyList<StickerDef> StickerDefs => stickerDefs ??= BuildStickerDefs();
        public IReadOnlyList<string> ColorTagIds => colorTagIds ??= BuildColorTagIds();
        public IReadOnlyList<LevelDef> LevelDefs => levelDefs ??= BuildLevelDefs();

        public TagData GetTag(string id)
        {
            tagsById ??= BuildTagLookup();
            return tagsById[id];
        }

        public StickerData GetSticker(string id)
        {
            if (stickersById == null)
            {
                stickersById = new Dictionary<string, StickerData>();
                foreach (var s in stickers) stickersById[s.Id] = s;
            }

            return stickersById[id];
        }

        void OnValidate() => ClearCaches();
        void OnEnable() => ClearCaches();

        void ClearCaches()
        {
            tagsById = null;
            stickersById = null;
            stickerDefs = null;
            colorTagIds = null;
            levelDefs = null;
        }

        List<StickerDef> BuildStickerDefs()
        {
            var list = new List<StickerDef>(stickers.Count);
            foreach (var s in stickers) list.Add(s.ToDef());
            return list;
        }

        List<string> BuildColorTagIds()
        {
            var list = new List<string>(colorTags.Count);
            foreach (var t in colorTags) list.Add(t.Id);
            return list;
        }

        List<LevelDef> BuildLevelDefs()
        {
            var list = new List<LevelDef>(levels.Count);
            for (int i = 0; i < levels.Count; i++) list.Add(levels[i].ToDef(i + 1));
            return list;
        }

        Dictionary<string, TagData> BuildTagLookup()
        {
            var map = new Dictionary<string, TagData>();
            foreach (var t in allTags) map[t.Id] = t;
            return map;
        }
    }
}
