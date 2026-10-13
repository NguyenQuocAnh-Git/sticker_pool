using System;
using System.Collections;
using StickerPool.View;
using UnityEngine;

namespace StickerPool.Controller
{
    /// <summary>
    /// Which page is on screen: Main Menu, Level Map or the game itself. Changing page runs the paper
    /// curtain; the swap happens while the curtain covers everything.
    /// </summary>
    public sealed class ScreenFlow : MonoBehaviour
    {
        public enum Page
        {
            Menu,
            Map,
            Game,
        }

        [SerializeField] MainMenuView menu;
        [SerializeField] LevelMapView map;
        [SerializeField] TransitionView transition;
        [Tooltip("Everything that only exists while a level is played: jar, HUD, roll call.")]
        [SerializeField] GameObject[] gameRoots;

        bool running;

        public Page Current { get; private set; } = Page.Menu;

        public bool IsBusy => running;

        /// <summary>Raised when the curtain starts moving; the controller plays the whoosh.</summary>
        public event Action TransitionStarted;

        void Awake()
        {
            transition.CoverStarted += () => TransitionStarted?.Invoke();

            // Start on a bare desk until the controller has loaded the language and shows the menu.
            menu.Hide();
            map.Hide();
            foreach (var root in gameRoots) root.SetActive(false);
        }

        /// <summary>Switches page without a transition (first frame of the app).</summary>
        public void ShowImmediate(Page target) => Apply(target);

        /// <param name="atCovered">Runs after the page swap, while the curtain still hides everything.</param>
        public void GoTo(Page target, Action atCovered = null)
        {
            if (running) return;
            StartCoroutine(Run(target, atCovered));
        }

        IEnumerator Run(Page target, Action atCovered)
        {
            running = true;
            yield return transition.Play(() =>
            {
                Apply(target);
                atCovered?.Invoke();
            });
            running = false;
        }

        void Apply(Page target)
        {
            Current = target;
            if (target == Page.Menu) menu.Show(); else menu.Hide();
            if (target == Page.Map) map.Show(); else map.Hide();
            foreach (var root in gameRoots) root.SetActive(target == Page.Game);
        }
    }
}
