using UnityEngine;

namespace StickerPool.View
{
    /// <summary>A particle system that is emitted on demand (tape pop sparkles, win confetti).</summary>
    public sealed class BurstView : MonoBehaviour
    {
        [SerializeField] ParticleSystem particles;
        [SerializeField] int defaultCount = 6;

        public void EmitAt(Vector3 worldPosition, int count = -1)
        {
            transform.position = new Vector3(worldPosition.x, worldPosition.y, transform.position.z);
            particles.Emit(count < 0 ? defaultCount : count);
        }

        public void PlayAt(Vector3 worldPosition)
        {
            transform.position = new Vector3(worldPosition.x, worldPosition.y, transform.position.z);
            particles.Play();
        }
    }
}
