using UnityEngine;

namespace StickerPool.View
{
    /// <summary>A full-screen page (menu, level map). The paper curtain does the transition, so show/hide are instant.</summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class UiScreen : MonoBehaviour
    {
        CanvasGroup group;

        CanvasGroup Group => group != null ? group : group = GetComponent<CanvasGroup>();

        public bool IsShown => gameObject.activeSelf;

        public void Show()
        {
            gameObject.SetActive(true);
            Group.alpha = 1f;
            Group.blocksRaycasts = true;
            Group.interactable = true;
            OnShown();
        }

        public void Hide()
        {
            Group.blocksRaycasts = false;
            gameObject.SetActive(false);
        }

        /// <summary>Runs after the page became visible; subclasses start their entrance animation.</summary>
        protected virtual void OnShown()
        {
        }
    }
}
