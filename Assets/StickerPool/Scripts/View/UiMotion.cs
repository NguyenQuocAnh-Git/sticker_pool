namespace StickerPool.View
{
    /// <summary>
    /// The player's "reduce motion" choice. Every animated duration that is not part of the core
    /// roll call goes through <see cref="Time"/>; the roll call uses it too (GDD section 8: 35%).
    /// </summary>
    public static class UiMotion
    {
        const float ReducedFactor = 0.35f;

        public static bool Reduced { get; set; }

        public static float Factor => Reduced ? ReducedFactor : 1f;

        public static float Time(float seconds) => seconds * Factor;
    }
}
