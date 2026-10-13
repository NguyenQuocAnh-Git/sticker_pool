using System.Runtime.InteropServices;
using UnityEngine;

namespace StickerPool.View
{
    public enum HapticKind
    {
        Light = 0,
        Medium = 1,
        Soft = 2,
        Rigid = 3,
        Selection = 4,
        Success = 5,
        Warning = 6,
    }

    /// <summary>
    /// Taptic feedback. On iOS it calls the small native plugin in Plugins/iOS; everywhere else
    /// it does nothing, so the Editor and Android builds are unaffected.
    /// </summary>
    public static class Haptics
    {
        const float MinInterval = 0.04f;

        static float lastPlayed = -1f;

        public static bool Enabled { get; set; } = true;

        public static void Play(HapticKind kind)
        {
            if (!Enabled) return;
            float now = Time.unscaledTime;
            if (now - lastPlayed < MinInterval) return;
            lastPlayed = now;
#if UNITY_IOS && !UNITY_EDITOR
            StickerPoolHaptic((int)kind);
#endif
        }

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        static extern void StickerPoolHaptic(int kind);
#endif
    }

    /// <summary>Reads the operating system's "reduce motion" switch (iOS only).</summary>
    public static class SystemAccessibility
    {
        public static bool ReduceMotionEnabled
        {
            get
            {
#if UNITY_IOS && !UNITY_EDITOR
                return StickerPoolReduceMotion() != 0;
#else
                return false;
#endif
            }
        }

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        static extern int StickerPoolReduceMotion();
#endif
    }
}
