using System.Collections.Generic;
using UnityEngine;

namespace StickerPool.Controller
{
    /// <summary>Persists the few settings and the progress that survive between sessions (PlayerPrefs).</summary>
    public sealed class SaveService
    {
        const string SoundKey = "stickerpool.sound";
        const string HapticsKey = "stickerpool.haptics";
        const string ReducedMotionKey = "stickerpool.reducedmotion";
        const string TutorialKey = "stickerpool.tutorial";
        const string LanguageKey = "stickerpool.language";
        const string ClearedKey = "stickerpool.cleared";
        const string StarsKey = "stickerpool.stars";

        readonly Dictionary<int, int> starsByLevel = new Dictionary<int, int>();

        public SaveService()
        {
            // Older saves only know which levels were cleared; count those as one star.
            var cleared = PlayerPrefs.GetString(ClearedKey, string.Empty);
            foreach (var part in cleared.Split(',', System.StringSplitOptions.RemoveEmptyEntries))
                if (int.TryParse(part, out int index)) starsByLevel[index] = 1;

            var stars = PlayerPrefs.GetString(StarsKey, string.Empty);
            foreach (var part in stars.Split(',', System.StringSplitOptions.RemoveEmptyEntries))
            {
                var pair = part.Split(':');
                if (pair.Length == 2 && int.TryParse(pair[0], out int index) && int.TryParse(pair[1], out int count))
                    starsByLevel[index] = Mathf.Clamp(count, 1, 3);
            }
        }

        public bool SoundOn
        {
            get => PlayerPrefs.GetInt(SoundKey, 1) == 1;
            set => SetFlag(SoundKey, value);
        }

        public bool HapticsOn
        {
            get => PlayerPrefs.GetInt(HapticsKey, 1) == 1;
            set => SetFlag(HapticsKey, value);
        }

        /// <summary>Follows the iOS "reduce motion" setting until the player chooses for themselves.</summary>
        public bool ReducedMotion
        {
            get => PlayerPrefs.HasKey(ReducedMotionKey)
                ? PlayerPrefs.GetInt(ReducedMotionKey) == 1
                : View.SystemAccessibility.ReduceMotionEnabled;
            set => SetFlag(ReducedMotionKey, value);
        }

        public bool TutorialSeen
        {
            get => PlayerPrefs.GetInt(TutorialKey, 0) == 1;
            set => SetFlag(TutorialKey, value);
        }

        /// <summary>Null until the player picks a language.</summary>
        public string LanguageCode
        {
            get => PlayerPrefs.HasKey(LanguageKey) ? PlayerPrefs.GetString(LanguageKey) : null;
            set
            {
                PlayerPrefs.SetString(LanguageKey, value);
                PlayerPrefs.Save();
            }
        }

        public bool IsCleared(int levelIndex) => starsByLevel.ContainsKey(levelIndex);

        /// <summary>Best star count of a level, 0 when it was never cleared.</summary>
        public int StarsOf(int levelIndex) => starsByLevel.TryGetValue(levelIndex, out int stars) ? stars : 0;

        public int TotalStars
        {
            get
            {
                int total = 0;
                foreach (int stars in starsByLevel.Values) total += stars;
                return total;
            }
        }

        public int HighestClearedIndex
        {
            get
            {
                int highest = -1;
                foreach (int index in starsByLevel.Keys) if (index > highest) highest = index;
                return highest;
            }
        }

        /// <summary>Stores a win; a replay never lowers the best star count.</summary>
        public void RecordWin(int levelIndex, int stars)
        {
            stars = Mathf.Clamp(stars, 1, 3);
            if (StarsOf(levelIndex) >= stars) return;
            starsByLevel[levelIndex] = stars;

            var parts = new List<string>();
            foreach (var pair in starsByLevel) parts.Add(pair.Key + ":" + pair.Value);
            PlayerPrefs.SetString(StarsKey, string.Join(",", parts));
            PlayerPrefs.Save();
        }

        static void SetFlag(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
