using UnityEngine;

namespace StickerPool.View
{
    /// <summary>
    /// Keeps the 1080-wide reference layout fully visible on any aspect ratio: fits the camera
    /// to the reference width, but never shows less than the reference height.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFitWidth : MonoBehaviour
    {
        [Tooltip("World width that must always be visible (jar width plus side margins).")]
        [SerializeField] float referenceWidth = 5.625f;
        [Tooltip("Smallest orthographic size, i.e. half of the reference height.")]
        [SerializeField] float minOrthographicSize = 5f;

        Camera cam;
        float lastAspect;

        void OnEnable() => Apply();

        void Update()
        {
            if (cam == null) cam = GetComponent<Camera>();
            if (!Mathf.Approximately(cam.aspect, lastAspect)) Apply();
        }

        void Apply()
        {
            if (cam == null) cam = GetComponent<Camera>();
            lastAspect = cam.aspect;
            if (lastAspect <= 0f) return;
            cam.orthographicSize = Mathf.Max(minOrthographicSize, referenceWidth / lastAspect * 0.5f);
        }
    }
}
