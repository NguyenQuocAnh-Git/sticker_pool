using UnityEngine;
using UnityEngine.Localization;

namespace StickerPool.Data
{
    /// <summary>A zone of words on the tape: colour, group, place, funny, material.</summary>
    [CreateAssetMenu(menuName = "Sticker Pool/Tag Category", fileName = "TagCategory")]
    public sealed class TagCategoryData : ScriptableObject
    {
        [SerializeField] string id;
        [SerializeField] LocalizedString label;

        public string Id => id;
        public LocalizedString Label => label;
    }
}
