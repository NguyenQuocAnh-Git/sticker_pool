using StickerPool.Data;
using UnityEngine;

namespace StickerPool.View
{
    /// <summary>Plays the short sounds through a fixed set of AudioSources placed in the scene.</summary>
    public sealed class SfxPlayer : MonoBehaviour
    {
        [SerializeField] GameConfig config;
        [SerializeField] AudioSource[] voices;
        [SerializeField, Range(0f, 1f)] float volume = 0.9f;

        int nextVoice;

        public bool Enabled { get; set; } = true;

        /// <summary>The roll-call sound of a tag; pitch climbs along a pentatonic scale with the count.</summary>
        public void PlayCount(AudioClip clip, int countIndex)
        {
            if (clip == null) return;
            var scale = config.countSemitones;
            int semitones = scale[Mathf.Min(countIndex, scale.Length - 1)];
            Play(clip, Mathf.Pow(2f, semitones / 12f));
        }

        public void PlayPop()
        {
            float variance = config.popPitchVariance;
            Play(config.popClip, 1f + Random.Range(-variance, variance));
        }

        public void PlayWin() => Play(config.winClip, 1f);

        /// <summary>A tape sticking on; the n-th tape of a batch plays a little higher.</summary>
        public void PlayTapeStick(int orderInBatch)
        {
            var steps = config.tapeStickSemitones;
            int semitones = steps.Length == 0 ? 0 : steps[Mathf.Min(orderInBatch, steps.Length - 1)];
            float jitter = Random.Range(-0.02f, 0.02f);
            Play(config.tapeStickClip, Mathf.Pow(2f, semitones / 12f) * (1f + jitter));
        }

        void Play(AudioClip clip, float pitch)
        {
            if (!Enabled || clip == null || voices.Length == 0) return;
            var voice = PickVoice();
            voice.pitch = pitch;
            voice.volume = volume;
            voice.PlayOneShot(clip);
        }

        AudioSource PickVoice()
        {
            for (int i = 0; i < voices.Length; i++)
            {
                var candidate = voices[(nextVoice + i) % voices.Length];
                if (candidate.isPlaying) continue;
                nextVoice = (nextVoice + i + 1) % voices.Length;
                return candidate;
            }

            var stolen = voices[nextVoice];
            nextVoice = (nextVoice + 1) % voices.Length;
            return stolen;
        }
    }
}
