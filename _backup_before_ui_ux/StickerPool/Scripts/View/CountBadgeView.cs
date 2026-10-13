using PrimeTween;
using TMPro;
using UnityEngine;

namespace StickerPool.View
{
    /// <summary>The yellow number bubble that appears under each sticker as it lands in the line.</summary>
    public sealed class CountBadgeView : MonoBehaviour
    {
        [SerializeField] RectTransform rect;
        [SerializeField] CanvasGroup group;
        [SerializeField] TMP_Text number;
        [SerializeField] float popDuration = 0.25f;
        [SerializeField] float popDrop = 0.06f;

        Tween tween;

        public void Show(Vector3 worldPosition, int value)
        {
            tween.Stop();
            number.text = value.ToString();
            gameObject.SetActive(true);
            var z = rect.position.z;
            rect.position = new Vector3(worldPosition.x, worldPosition.y, z);
            tween = Tween.Custom(0f, 1f, popDuration, onValueChange: t =>
            {
                rect.localScale = Vector3.one * Mathf.LerpUnclamped(0.4f, 1f, t);
                group.alpha = Mathf.Clamp01(t * 1.5f);
                rect.position = new Vector3(worldPosition.x, worldPosition.y - popDrop * (1f - t), z);
            }, ease: Ease.OutBack);
        }

        public void FadeOut(float duration)
        {
            tween.Stop();
            tween = Tween.Alpha(group, 0f, duration);
        }

        public void Hide()
        {
            tween.Stop();
            gameObject.SetActive(false);
        }
    }
}
