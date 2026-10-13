using System;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StickerPool.View
{
    /// <summary>
    /// The scrolling level path: level 1 at the bottom, a washi-tape trail winding upwards.
    /// Nodes are placed in the scene; this view only positions them along the wave and fills in their state.
    /// </summary>
    public sealed class LevelMapView : UiScreen
    {
        [SerializeField] ScrollRect scroll;
        [SerializeField] RectTransform content;
        [SerializeField] LevelNodeView[] nodes;
        [Tooltip("Strips between neighbouring nodes; needs one fewer than nodes.")]
        [SerializeField] Image[] trail;
        [SerializeField] RectTransform soonBanner;
        [SerializeField] Button backButton;
        [SerializeField] TMP_Text starsValue;

        [Header("Layout (canvas pixels)")]
        [SerializeField] float bottomPadding = 240f;
        [SerializeField] float topPadding = 200f;
        [SerializeField] float spacing = 280f;
        [SerializeField] float amplitude = 260f;
        [SerializeField] float waveStep = 0.95f;
        [SerializeField] float nodeRadius = 90f;
        [SerializeField] float trailThickness = 38f;
        [Tooltip("Show the 'more levels soon' banner while the game has fewer levels than this.")]
        [SerializeField] int plannedLevelCount = 20;
        [SerializeField] Color trailDone = new Color32(0xF2, 0xB5, 0x44, 0xFF);
        [SerializeField] Color trailTodo = new Color32(0xE8, 0xD3, 0xB5, 0xFF);

        [Header("Entrance")]
        [SerializeField] float nodePopDuration = 0.4f;
        [SerializeField] float nodePopStagger = 0.05f;

        int levelCount;
        int currentIndex;
        int unlockedCount;
        int totalStars;
        Func<int, int> starsOf;
        Sequence entrance;

        public event Action<int> LevelChosen;
        public event Action BackClicked;

        void Awake()
        {
            backButton.onClick.AddListener(() => BackClicked?.Invoke());
            foreach (var node in nodes) node.Clicked += index => LevelChosen?.Invoke(index);
        }

        void OnDestroy() => backButton.onClick.RemoveAllListeners();

        /// <param name="starsOf">Best star count of a level index, 0 when never cleared.</param>
        public void Setup(int count, Func<int, int> starsOf, int current, int unlocked, int stars)
        {
            levelCount = Mathf.Min(count, nodes.Length);
            this.starsOf = starsOf;
            currentIndex = current;
            unlockedCount = unlocked;
            totalStars = stars;
        }

        protected override void OnShown()
        {
            Canvas.ForceUpdateCanvases();
            Layout();
            ScrollToCurrent();
            PlayEntrance();
            starsValue.text = $"{totalStars} / {levelCount * 3}";
        }

        void Layout()
        {
            bool showSoon = levelCount < plannedLevelCount;
            int slots = levelCount + (showSoon ? 1 : 0);
            float viewport = ((RectTransform)scroll.viewport).rect.height;
            float height = Mathf.Max(viewport, bottomPadding + (slots - 1) * spacing + topPadding);
            content.sizeDelta = new Vector2(content.sizeDelta.x, height);

            for (int i = 0; i < nodes.Length; i++)
            {
                bool active = i < levelCount;
                nodes[i].gameObject.SetActive(active);
                if (!active) continue;
                ((RectTransform)nodes[i].transform).anchoredPosition = NodePosition(i);
                nodes[i].Setup(i, starsOf(i), i == currentIndex, i < unlockedCount);
            }

            soonBanner.gameObject.SetActive(showSoon);
            if (showSoon) soonBanner.anchoredPosition = NodePosition(levelCount);

            for (int i = 0; i < trail.Length; i++)
            {
                bool active = i < levelCount - 1;
                trail[i].gameObject.SetActive(active);
                if (!active) continue;
                PlaceTrail(trail[i].rectTransform, NodePosition(i), NodePosition(i + 1));
                trail[i].color = starsOf(i) > 0 ? trailDone : trailTodo;
            }
        }

        Vector2 NodePosition(int i) => new Vector2(Mathf.Sin(i * waveStep) * amplitude, bottomPadding + i * spacing);

        void PlaceTrail(RectTransform strip, Vector2 from, Vector2 to)
        {
            var delta = to - from;
            float length = Mathf.Max(0f, delta.magnitude - nodeRadius * 2f);
            strip.anchoredPosition = (from + to) * 0.5f;
            strip.sizeDelta = new Vector2(length, trailThickness);
            strip.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        }

        void ScrollToCurrent()
        {
            float viewport = ((RectTransform)scroll.viewport).rect.height;
            float height = content.rect.height;
            float depth = height - NodePosition(Mathf.Clamp(currentIndex, 0, levelCount - 1)).y;
            float offset = Mathf.Clamp(depth - viewport * 0.5f, 0f, Mathf.Max(0f, height - viewport));
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, offset);
            scroll.velocity = Vector2.zero;
        }

        void PlayEntrance()
        {
            entrance.Stop();
            entrance = Sequence.Create();
            float duration = UiMotion.Time(nodePopDuration);
            for (int i = 0; i < levelCount; i++)
            {
                int distance = Mathf.Abs(i - currentIndex);
                if (distance > 6) continue;
                var node = nodes[i].transform;
                node.localScale = Vector3.zero;
                entrance.Group(Tween.Scale(node, 0f, 1f, duration, Ease.OutBack,
                    startDelay: UiMotion.Time(distance * nodePopStagger)));
            }

            for (int i = 0; i < levelCount; i++)
                if (Mathf.Abs(i - currentIndex) > 6) nodes[i].transform.localScale = Vector3.one;
        }
    }
}
