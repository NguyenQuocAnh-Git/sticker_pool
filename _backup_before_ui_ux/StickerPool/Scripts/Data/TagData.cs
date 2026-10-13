using UnityEngine;
using UnityEngine.Localization;

namespace StickerPool.Data
{
    /// <summary>A word the player can pick (a colour, a group, a place...).</summary>
    [CreateAssetMenu(menuName = "Sticker Pool/Tag", fileName = "Tag")]
    public sealed class TagData : ScriptableObject
    {
        [SerializeField] string id;
        [SerializeField] TagCategoryData category;
        [SerializeField] LocalizedString label;
        [Tooltip("Played while the stickers are counted off. Pitch rises with the count.")]
        [SerializeField] AudioClip rollCallClip;
        [Tooltip("Washi tape colour. Colour words use their own colour; other words share one colour per category.")]
        [SerializeField] Color tapeColor = Color.white;

        public Color TapeColor => tapeColor;
        public string Id => id;
        public TagCategoryData Category => category;
        public LocalizedString Label => label;
        public AudioClip RollCallClip => rollCallClip;
    }
}
