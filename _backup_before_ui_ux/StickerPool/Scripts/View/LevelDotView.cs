using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StickerPool.View
{
    /// <summary>One level button in the HUD row.</summary>
    public sealed class LevelDotView : MonoBehaviour
    {
        [SerializeField] Button button;
        [SerializeField] Image background;
        [SerializeField] Image ring;
        [SerializeField] TMP_Text number;
        [SerializeField] Color idleColor = new Color(0.153f, 0.2f, 0.235f);
        [SerializeField] Color clearedColor = new Color(0.173f, 0.396f, 0.322f);
        [SerializeField] Color idleTextColor = new Color(0.616f, 0.69f, 0.659f);
        [SerializeField] Color activeTextColor = new Color(0.949f, 0.933f, 0.882f);

        int index;

        public event Action<int> Clicked;

        void Awake() => button.onClick.AddListener(() => Clicked?.Invoke(index));

        void OnDestroy() => button.onClick.RemoveAllListeners();

        public void Setup(int levelIndex, bool cleared, bool current, bool unlocked)
        {
            index = levelIndex;
            number.text = (levelIndex + 1).ToString();
            background.color = cleared ? clearedColor : idleColor;
            ring.enabled = current;
            number.color = cleared || current ? activeTextColor : idleTextColor;
            button.interactable = unlocked;
            var tint = unlocked ? 1f : 0.45f;
            background.canvasRenderer.SetAlpha(tint);
            number.canvasRenderer.SetAlpha(tint);
        }

        public void SetInteractable(bool value, bool unlocked) => button.interactable = value && unlocked;
    }
}
