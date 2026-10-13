using PrimeTween;
using TMPro;
using UnityEngine;

namespace StickerPool.View
{
    /// <summary>The title and number bubbles shown above the stickers while they are counted off.</summary>
    public sealed class RollCallView : MonoBehaviour
    {
        [SerializeField] RectTransform titleRect;
        [SerializeField] CanvasGroup titleGroup;
        [SerializeField] TMP_Text title;
        [SerializeField] Color titleColor = new Color(0.949f, 0.933f, 0.882f);
        [SerializeField] Color titleDoneColor = new Color(0.949f, 0.757f, 0.306f);
        [SerializeField] CountBadgeView[] badges;

        Tween titleTween;

        /// <summary>Largest line the view can number; bigger jars need more badges in the scene.</summary>
        public int BadgeCapacity => badges.Length;

        public void Begin(string text, Vector3 titleWorldPosition)
        {
            titleTween.Stop();
            title.text = text;
            title.color = titleColor;
            titleRect.localScale = Vector3.one;
            titleGroup.alpha = 1f;
            titleRect.position = new Vector3(titleWorldPosition.x, titleWorldPosition.y, titleRect.position.z);
            titleRect.gameObject.SetActive(true);
        }

        public void SetTitle(string text) => title.text = text;

        public void MarkDone(string text)
        {
            title.text = text;
            title.color = titleDoneColor;
            titleTween.Stop();
            titleTween = Tween.Custom(0f, 1f, 0.35f, onValueChange: t =>
                titleRect.localScale = Vector3.one * (1f + 0.15f * Mathf.Sin(t * Mathf.PI)), ease: Ease.Linear);
        }

        public void ShowBadge(int index, Vector3 worldPosition, int number)
        {
            if (index >= badges.Length) return;
            badges[index].Show(worldPosition, number);
        }

        public void FadeOut(float badgeDuration, float titleDuration)
        {
            foreach (var badge in badges)
                if (badge.gameObject.activeSelf) badge.FadeOut(badgeDuration);
            titleTween.Stop();
            titleTween = Tween.Alpha(titleGroup, 0f, titleDuration);
        }

        public void Clear()
        {
            titleTween.Stop();
            titleRect.gameObject.SetActive(false);
            foreach (var badge in badges) badge.Hide();
        }
    }
}
