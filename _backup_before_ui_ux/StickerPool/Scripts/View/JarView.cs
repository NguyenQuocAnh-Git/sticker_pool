using PrimeTween;
using UnityEngine;

namespace StickerPool.View
{
    /// <summary>
    /// The jar: a craft mat with three collider walls, a mask that hides stickers above its mouth,
    /// and a dim layer used while stickers are counted off.
    /// </summary>
    public sealed class JarView : MonoBehaviour
    {
        [Tooltip("Inner size in world units. One sticker cell is a fifth of the width.")]
        [SerializeField] Vector2 size = new Vector2(5f, 5f);
        [SerializeField] Transform stickerRoot;
        [SerializeField] SpriteRenderer dim;

        Tween dimTween;

        public Transform StickerRoot => stickerRoot;
        public Vector2 Size => size;
        public Vector2 Center => transform.position;
        public float CellSize => size.x / 5f;
        public float Left => Center.x - size.x * 0.5f;
        public float Top => Center.y + size.y * 0.5f;

        public bool Contains(Vector2 point)
        {
            var c = Center;
            return Mathf.Abs(point.x - c.x) <= size.x * 0.5f && Mathf.Abs(point.y - c.y) <= size.y * 0.5f;
        }

        public void SetDim(float alpha)
        {
            dimTween.Stop();
            SetDimAlpha(alpha);
        }

        public Tween FadeDim(float alpha, float duration)
        {
            dimTween.Stop();
            dimTween = Tween.Alpha(dim, alpha, duration, Ease.OutQuad);
            return dimTween;
        }

        void SetDimAlpha(float alpha)
        {
            var c = dim.color;
            c.a = alpha;
            dim.color = c;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, size);
        }
    }
}
