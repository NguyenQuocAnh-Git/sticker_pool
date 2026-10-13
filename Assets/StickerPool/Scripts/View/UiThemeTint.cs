using StickerPool.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StickerPool.View
{
    /// <summary>
    /// Paints the Graphic, TMP text, SpriteRenderer or Camera on this object with one colour of the
    /// <see cref="UiTheme"/>. Editing the theme asset recolours everything live, also in Edit Mode.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class UiThemeTint : MonoBehaviour
    {
        [SerializeField] UiTheme theme;
        [SerializeField] ThemeRole role = ThemeRole.Paper;
        [SerializeField, Range(0f, 1f)] float alpha = 1f;

        void OnEnable()
        {
            UiTheme.Changed += Apply;
            Apply();
        }

        void OnDisable() => UiTheme.Changed -= Apply;

        void OnValidate() => Apply();

        public void Apply()
        {
            if (theme == null) return;
            var color = theme.Get(role);
            color.a *= alpha;

            if (TryGetComponent<TMP_Text>(out var text)) text.color = color;
            else if (TryGetComponent<Graphic>(out var graphic)) graphic.color = color;
            else if (TryGetComponent<SpriteRenderer>(out var sprite)) sprite.color = color;
            else if (TryGetComponent<Camera>(out var cam)) cam.backgroundColor = color;
        }
    }
}
