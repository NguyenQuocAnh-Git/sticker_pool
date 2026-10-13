using System.Collections.Generic;
using StickerPool.Model;
using UnityEngine;
using UnityEngine.Localization;

namespace StickerPool.Data
{
    /// <summary>One sticker type. The prefab holds the hand-tuned sprite and collider.</summary>
    [CreateAssetMenu(menuName = "Sticker Pool/Sticker", fileName = "Sticker")]
    public sealed class StickerData : ScriptableObject
    {
        [SerializeField] string id;
        [SerializeField] LocalizedString displayName;
        [SerializeField] GameObject prefab;
        [SerializeField] TagData colorTag;
        [Tooltip("Group, place, funny word and material tags (anything except the colour).")]
        [SerializeField] List<TagData> otherTags = new List<TagData>();

        public string Id => id;
        public LocalizedString DisplayName => displayName;
        public GameObject Prefab => prefab;
        public TagData ColorTag => colorTag;
        public IReadOnlyList<TagData> OtherTags => otherTags;

        public StickerDef ToDef()
        {
            var tags = new List<string>(otherTags.Count + 1) { colorTag.Id };
            foreach (var tag in otherTags) tags.Add(tag.Id);
            return new StickerDef(id, colorTag.Id, tags);
        }
    }
}
