using System.Collections.Generic;
using UnityEngine;

namespace StickerPool.Controller
{
    /// <summary>Persists the few settings that survive between sessions (PlayerPrefs).</summary>
    public sealed class SaveService
    {
        const string SoundKey = "stickerpool.sound";
        const string LanguageKey = "stickerpool.language";
        const string ClearedKey = "stickerpool.cleared";

        readonly HashSet<int> cleared = new HashSet<int>();

        public SaveService()
        {
            var raw = PlayerPrefs.GetString(ClearedKey, string.Empty);
            foreach (var part in raw.Split(',', System.StringSplitOptions.RemoveEmptyEntries))
                if (int.TryParse(part, out int index)) cleared.Add(index);
        }

        public bool SoundOn
        {
            get => PlayerPrefs.GetInt(SoundKey, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(SoundKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
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

        public bool IsCleared(int levelIndex) => cleared.Contains(levelIndex);

        public int HighestClearedIndex
        {
            get
            {
                int highest = -1;
                foreach (int index in cleared) if (index > highest) highest = index;
                return highest;
            }
        }

        public void MarkCleared(int levelIndex)
        {
            if (!cleared.Add(levelIndex)) return;
            PlayerPrefs.SetString(ClearedKey, string.Join(",", cleared));
            PlayerPrefs.Save();
        }
    }
}
