using UnityEngine;

namespace StickerPool.View
{
    /// <summary>Stretches a RectTransform to the device safe area (notch, Dynamic Island, home bar).</summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        RectTransform rect;
        Rect lastSafeArea;
        Vector2Int lastScreen;

        void OnEnable() => Apply();

        void Update()
        {
            if (Screen.safeArea != lastSafeArea || Screen.width != lastScreen.x || Screen.height != lastScreen.y)
                Apply();
        }

        void Apply()
        {
            if (rect == null) rect = (RectTransform)transform;
            var safe = Screen.safeArea;
            lastSafeArea = safe;
            lastScreen = new Vector2Int(Screen.width, Screen.height);
            if (Screen.width <= 0 || Screen.height <= 0) return;

            var min = safe.position;
            var max = safe.position + safe.size;
            rect.anchorMin = new Vector2(min.x / Screen.width, min.y / Screen.height);
            rect.anchorMax = new Vector2(max.x / Screen.width, max.y / Screen.height);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
