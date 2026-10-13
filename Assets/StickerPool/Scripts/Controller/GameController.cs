using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using StickerPool.Data;
using StickerPool.Model;
using StickerPool.View;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using Random = UnityEngine.Random;

namespace StickerPool.Controller
{
    /// <summary>
    /// Wires the model (<see cref="GameSession"/>) to the views: starts levels, reacts to taps,
    /// runs the choose sequence and decides win or lose. Everything it touches is assigned in the scene.
    /// </summary>
    public sealed class GameController : MonoBehaviour
    {
        [Header("Content")]
        [SerializeField] GameDatabase database;
        [SerializeField] GameConfig config;

        [Header("Game views")]
        [SerializeField] JarView jar;
        [SerializeField] HudView hud;
        [SerializeField] TapeView[] tapes;
        [SerializeField] RollCallView rollCall;
        [SerializeField] ResultPanelView result;
        [SerializeField] LevelIntroView intro;
        [SerializeField] SfxPlayer sfx;
        [SerializeField] BurstView popBurst;
        [SerializeField] BurstView winConfetti;

        [Header("Pages and popups")]
        [SerializeField] ScreenFlow flow;
        [SerializeField] MainMenuView menu;
        [SerializeField] LevelMapView map;
        [SerializeField] SettingsPopupView settings;
        [SerializeField] ConfirmPopupView confirm;

        [Header("Texts")]
        [SerializeField] LocalizedString greatText;
        [SerializeField] LocalizedString awesomeText;
        [SerializeField] LocalizedString restartTitle;
        [SerializeField] LocalizedString restartBody;
        [SerializeField] LocalizedString homeTitle;
        [SerializeField] LocalizedString homeBody;

        [Header("Level start")]
        [Tooltip("How long the banner waits after the curtain starts to lift.")]
        [SerializeField] float introDelayAfterCurtain = 0.2f;
        [Tooltip("Extra wait before the first tapes stick on, so they appear once the curtain is gone.")]
        [SerializeField] float tapeDelayAfterCurtain = 0.3f;
        [Tooltip("How long after the level starts the tutorial hand appears.")]
        [SerializeField] float tutorialDelay = 1.6f;

        readonly Dictionary<int, StickerView> viewsById = new Dictionary<int, StickerView>();

        SaveService save;
        StickerObjectPool stickerPool;
        BudgetCalculator budgets;
        ChooseSequence chooseSequence;
        GameSession session;
        int levelIndex;
        int startingStickers;
        bool busy;

        int UnlockedCount => config.unlockAllLevels
            ? database.LevelCount
            : Mathf.Min(database.LevelCount, save.HighestClearedIndex + 2);

        void Awake()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Time.fixedDeltaTime = 1f / 60f; // physics tuned for a 60 Hz step

            save = new SaveService();
            stickerPool = new StickerObjectPool(jar.StickerRoot, view => view.Tapped += OnStickerTapped);
            budgets = new BudgetCalculator(database.StickerDefs, database.ColorTagIds);
            chooseSequence = new ChooseSequence(config, jar, rollCall, sfx, popBurst);

            foreach (var tape in tapes) tape.Clicked += OnTapeClicked;
            hud.SettingsClicked += OnHudSettingsClicked;
            hud.RerollClicked += OnRerollClicked;
            result.NextClicked += OnNextClicked;
            result.AgainClicked += OnRestartLevel;
            result.RetryClicked += OnRestartLevel;
            result.StarLanded += OnStarLanded;

            menu.PlayClicked += OnPlayClicked;
            menu.LevelsClicked += OnLevelsClicked;
            menu.SettingsClicked += OnMenuSettingsClicked;
            map.LevelChosen += OnLevelChosen;
            map.BackClicked += OnBackToMenu;

            settings.SoundToggled += OnSoundToggled;
            settings.HapticsToggled += OnHapticsToggled;
            settings.ReducedMotionToggled += OnReducedMotionToggled;
            settings.LanguageClicked += OnLanguageClicked;
            settings.RestartClicked += OnSettingsRestart;
            settings.HomeClicked += OnSettingsHome;

            UiButton.Clicked += sfx.PlayClick;
            PopupView.AnyOpened += sfx.PlayPopupOpen;
            PopupView.AnyClosing += sfx.PlayPopupClose;
            flow.TransitionStarted += sfx.PlayWhoosh;
        }

        IEnumerator Start()
        {
            yield return LocalizationSettings.InitializationOperation;
            ApplySavedLocale();
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;

            sfx.Enabled = save.SoundOn;
            Haptics.Enabled = save.HapticsOn;
            UiMotion.Reduced = save.ReducedMotion;
            settings.SetSound(sfx.Enabled, false);
            settings.SetHaptics(Haptics.Enabled, false);
            settings.SetReducedMotion(UiMotion.Reduced, false);
            settings.SetLanguageCode(CurrentLanguageCode());

            hud.SetSwapFeature(config.swapWordsEnabled);
            RefreshMenu();
            flow.ShowImmediate(ScreenFlow.Page.Menu);
        }

        void OnDestroy()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            foreach (var tape in tapes) tape.Clicked -= OnTapeClicked;
            hud.SettingsClicked -= OnHudSettingsClicked;
            hud.RerollClicked -= OnRerollClicked;
            result.NextClicked -= OnNextClicked;
            result.AgainClicked -= OnRestartLevel;
            result.RetryClicked -= OnRestartLevel;
            result.StarLanded -= OnStarLanded;

            menu.PlayClicked -= OnPlayClicked;
            menu.LevelsClicked -= OnLevelsClicked;
            menu.SettingsClicked -= OnMenuSettingsClicked;
            map.LevelChosen -= OnLevelChosen;
            map.BackClicked -= OnBackToMenu;

            settings.SoundToggled -= OnSoundToggled;
            settings.HapticsToggled -= OnHapticsToggled;
            settings.ReducedMotionToggled -= OnReducedMotionToggled;
            settings.LanguageClicked -= OnLanguageClicked;
            settings.RestartClicked -= OnSettingsRestart;
            settings.HomeClicked -= OnSettingsHome;

            UiButton.Clicked -= sfx.PlayClick;
            PopupView.AnyOpened -= sfx.PlayPopupOpen;
            PopupView.AnyClosing -= sfx.PlayPopupClose;
            flow.TransitionStarted -= sfx.PlayWhoosh;
        }

        // ---- Pages ----

        void RefreshMenu() =>
            menu.SetProgress(FirstUnclearedLevel() + 1, save.HighestClearedIndex >= 0, save.TotalStars);

        void RefreshMap() =>
            map.Setup(database.LevelCount, save.StarsOf, FirstUnclearedLevel(), UnlockedCount, save.TotalStars);

        void OnPlayClicked() => EnterLevel(FirstUnclearedLevel());

        void OnLevelsClicked()
        {
            RefreshMap();
            flow.GoTo(ScreenFlow.Page.Map);
        }

        void OnLevelChosen(int index)
        {
            if (index < UnlockedCount) EnterLevel(index);
        }

        void OnBackToMenu()
        {
            RefreshMenu();
            flow.GoTo(ScreenFlow.Page.Menu);
        }

        /// <summary>Curtain over the screen, then the level starts behind it.</summary>
        void EnterLevel(int index) =>
            flow.GoTo(ScreenFlow.Page.Game, () => StartLevel(index, fromCurtain: true));

        void LeaveLevel()
        {
            settings.CloseImmediate();
            RefreshMenu();
            flow.GoTo(ScreenFlow.Page.Menu, StopLevel);
        }

        // ---- Level flow ----

        int FirstUnclearedLevel()
        {
            for (int i = 0; i < database.LevelCount; i++)
                if (!save.IsCleared(i)) return Mathf.Min(i, UnlockedCount - 1);
            return 0;
        }

        void StopLevel()
        {
            StopAllCoroutines();
            Tween.StopAll();
            UiButton.ResetAll();
            busy = false;
            intro.Hide();
            hud.HideTutorial();
            result.Hide();
        }

        void StartLevel(int index, bool fromCurtain = false)
        {
            StopAllCoroutines();
            Tween.StopAll();
            UiButton.ResetAll();

            busy = false;
            levelIndex = Mathf.Clamp(index, 0, database.LevelCount - 1);
            rollCall.Clear();
            result.Hide();
            jar.SetDim(0f);
            stickerPool.ReleaseAll();
            viewsById.Clear();

            var levelDef = database.LevelDefs[levelIndex];
            session = GameSession.Create(levelDef, database.StickerDefs, database.ColorTagIds, budgets, NextRunSeed);
            startingStickers = session.Alive.Count;

            foreach (var placed in session.Stickers)
            {
                var data = database.GetSticker(placed.Def.Id);
                var view = stickerPool.Get(data);
                view.Place(data, placed.Id, SpawnPosition(placed), (float)placed.Rotation);
                viewsById[placed.Id] = view;
            }

            hud.ShowInfo(database.Levels[levelIndex].Hint);
            hud.SetBusy(false);
            RefreshHud(animateProgress: false);

            float curtainDelay = fromCurtain ? tapeDelayAfterCurtain : 0f;
            RefreshTapes(config.tapeFirstDelay + curtainDelay);
            PlayIntro(fromCurtain ? introDelayAfterCurtain : 0f);
            ShowTutorialIfNeeded();
        }

        void PlayIntro(float delay)
        {
            int levelNumber = levelIndex + 1;
            int stickers = startingStickers;
            int moves = session.Budget;
            intro.Hide();
            if (delay <= 0f) intro.Play(levelNumber, stickers, moves);
            else Tween.Delay(delay, () => intro.Play(levelNumber, stickers, moves));
        }

        void ShowTutorialIfNeeded()
        {
            if (save.TutorialSeen) hud.HideTutorial();
            else hud.ShowTutorial(tapes[tapes.Length / 2].transform, tutorialDelay);
        }

        static uint NextRunSeed() => unchecked((uint)Random.Range(int.MinValue, int.MaxValue));

        Vector2 SpawnPosition(PlacedSticker placed)
        {
            int perBlock = config.spawnStickersPerLongRow + config.spawnStickersPerShortRow;
            int inBlock = placed.Id % perBlock;
            bool shortRow = inBlock >= config.spawnStickersPerLongRow;
            int row = placed.Id / perBlock * 2 + (shortRow ? 1 : 0);
            int column = shortRow ? inBlock - config.spawnStickersPerLongRow : inBlock;

            float cell = jar.CellSize;
            float jitter = (float)placed.X;
            float x = jar.Left + ((shortRow ? 1f : 0.5f) + column) * cell + (jitter - 0.5f) * config.spawnJitter * cell;
            float y = jar.Top + (config.spawnHeightAboveJar + row * config.spawnRowSpacing
                                 + jitter * config.spawnStaggerHeight) * cell;
            return new Vector2(x, y);
        }

        void RefreshHud(bool animateProgress = true)
        {
            hud.SetLevel(levelIndex + 1, save.StarsOf(levelIndex));
            hud.SetMoves(session.MovesLeft, session.Budget, false);
            hud.SetStickers(session.Alive.Count, startingStickers, animateProgress);
            hud.SetRerollsLeft(session.RerollsLeft, session.CanReroll && !busy);
        }

        /// <summary>Puts every slot's word on its tape (start of a level); the tapes stick on one after another.</summary>
        void RefreshTapes(float firstDelay)
        {
            var slots = session.Slots;
            int order = 0;
            for (int i = 0; i < tapes.Length; i++)
            {
                if (slots[i] == null) tapes[i].Hide();
                else PutTape(i, slots[i], firstDelay, order++);
            }
        }

        /// <summary>
        /// After a turn or a word swap: tapes whose word is unchanged stay exactly as they are; only the tapes
        /// that lost their word peel away and are replaced by a new one that sticks on with a sound.
        /// </summary>
        IEnumerator SwapTapes()
        {
            var slots = session.Slots;
            var changed = new List<int>();
            for (int i = 0; i < tapes.Length; i++)
            {
                string current = tapes[i].Tag != null ? tapes[i].Tag.Id : null;
                if (current != slots[i]) changed.Add(i);
                else if (current != null) tapes[i].Restore();
            }

            bool anyLeaving = false;
            foreach (int i in changed)
            {
                if (tapes[i].Tag == null) continue;
                tapes[i].PlayDisappear(config.tapeDisappearDuration);
                anyLeaving = true;
            }

            if (anyLeaving) yield return new WaitForSeconds(config.tapeDisappearDuration);

            int order = 0;
            foreach (int i in changed)
            {
                if (slots[i] == null) tapes[i].Hide();
                else PutTape(i, slots[i], 0f, order++);
            }
        }

        void PutTape(int slot, string tagId, float baseDelay, int order)
        {
            float delay = baseDelay + order * config.tapeStagger;
            tapes[slot].Show(database.GetTag(tagId));
            tapes[slot].PlayAppear(delay);
            Tween.Delay(delay + config.tapeSoundOffset, () => sfx.PlayTapeStick(order));
        }

        // ---- Input ----

        void OnStickerTapped(StickerView view)
        {
            if (busy || session == null || !jar.Contains(view.transform.position)) return;
            Haptics.Play(HapticKind.Soft);
            hud.ShowInfoText($"<b>{view.Data.DisplayName.GetLocalizedString()}</b>");
            view.Poke(new Vector2(Random.Range(-config.pokeSideSpeed, config.pokeSideSpeed), config.pokeUpSpeed));
        }

        void OnTapeClicked(TapeView tape)
        {
            if (busy || session == null || session.IsOver) return;
            Haptics.Play(HapticKind.Medium);
            if (!save.TutorialSeen)
            {
                save.TutorialSeen = true;
                hud.HideTutorial();
            }

            StartCoroutine(ResolveChoice(tape));
        }

        IEnumerator ResolveChoice(TapeView tape)
        {
            busy = true;
            hud.SetBusy(true);

            var tag = tape.Tag;
            var hitIds = session.Choose(tag.Id);
            hud.SetMoves(session.MovesLeft, session.Budget, true);
            hud.SetStickers(session.Alive.Count, startingStickers, true);
            tape.PlayPicked();
            foreach (var t in tapes) t.SetWaiting(true, t == tape);

            var views = new List<StickerView>(hitIds.Count);
            foreach (int id in hitIds)
            {
                views.Add(viewsById[id]);
                viewsById.Remove(id);
            }

            string tagText = tag.Label.GetLocalizedString();
            yield return chooseSequence.Run(tag, views, tape, tagText, PraiseFor(views.Count),
                () => hud.ShowFlewOut(tagText, views.Count));

            foreach (var view in views) stickerPool.Release(view);
            rollCall.Clear();
            yield return new WaitForSeconds(UiMotion.Time(config.tapeBounceDuration));
            yield return SwapTapes();

            busy = false;
            hud.SetBusy(false);
            RefreshHud();

            if (session.IsCleared) Win();
            else if (session.IsOutOfMoves) Lose();
        }

        string PraiseFor(int count)
        {
            if (count >= config.awesomeCount) return " " + awesomeText.GetLocalizedString();
            if (count >= config.greatCount) return " " + greatText.GetLocalizedString();
            return string.Empty;
        }

        void Win()
        {
            sfx.PlayWin();
            Haptics.Play(HapticKind.Success);
            save.RecordWin(levelIndex, session.Stars);
            hud.SetLevel(levelIndex + 1, save.StarsOf(levelIndex));
            bool isFinal = levelIndex == database.LevelCount - 1;
            result.ShowWin(session.Stars, session.Used, session.Budget, isFinal);
            winConfetti.PlayAt(jar.Center);
        }

        void Lose()
        {
            Haptics.Play(HapticKind.Warning);
            result.ShowLose(session.Alive.Count);
        }

        void OnStarLanded(int index)
        {
            if (!result.IsVisible) return;
            sfx.PlayStar(index);
            Haptics.Play(HapticKind.Rigid);
        }

        // ---- Buttons ----

        void OnRestartLevel()
        {
            if (!busy) StartLevel(levelIndex);
        }

        void OnNextClicked()
        {
            if (busy) return;
            int next = Mathf.Min(levelIndex + 1, database.LevelCount - 1);
            flow.GoTo(ScreenFlow.Page.Game, () => StartLevel(next, fromCurtain: true));
        }

        void OnRerollClicked()
        {
            if (!config.swapWordsEnabled || busy || session == null || !session.TryReroll()) return;
            StartCoroutine(RerollRoutine());
        }

        IEnumerator RerollRoutine()
        {
            busy = true;
            hud.SetBusy(true);
            RefreshHud();
            hud.ShowRerolled();
            yield return SwapTapes();
            busy = false;
            hud.SetBusy(false);
            RefreshHud();
        }

        // ---- Settings ----

        void OnHudSettingsClicked() => OpenSettings(true);

        void OnMenuSettingsClicked() => OpenSettings(false);

        void OpenSettings(bool inGame)
        {
            settings.SetInGame(inGame);
            settings.SetLanguageCode(CurrentLanguageCode());
            settings.Open();
        }

        void OnSoundToggled()
        {
            sfx.Enabled = !sfx.Enabled;
            save.SoundOn = sfx.Enabled;
            settings.SetSound(sfx.Enabled, true);
        }

        void OnHapticsToggled()
        {
            Haptics.Enabled = !Haptics.Enabled;
            save.HapticsOn = Haptics.Enabled;
            settings.SetHaptics(Haptics.Enabled, true);
            Haptics.Play(HapticKind.Medium);
        }

        void OnReducedMotionToggled()
        {
            UiMotion.Reduced = !UiMotion.Reduced;
            save.ReducedMotion = UiMotion.Reduced;
            settings.SetReducedMotion(UiMotion.Reduced, true);
        }

        void OnSettingsRestart()
        {
            if (busy || flow.Current != ScreenFlow.Page.Game) return;
            confirm.Ask(restartTitle, restartBody, () =>
            {
                settings.CloseImmediate();
                StartLevel(levelIndex);
            });
        }

        void OnSettingsHome()
        {
            if (flow.Current != ScreenFlow.Page.Game) return;
            confirm.Ask(homeTitle, homeBody, LeaveLevel);
        }

        // ---- Language ----

        void ApplySavedLocale()
        {
            string code = save.LanguageCode ?? "en";
            var locale = LocalizationSettings.AvailableLocales.GetLocale(code)
                         ?? LocalizationSettings.AvailableLocales.Locales[0];
            LocalizationSettings.SelectedLocale = locale;
        }

        void OnLanguageClicked()
        {
            var locales = LocalizationSettings.AvailableLocales.Locales;
            if (locales.Count < 2) return;
            int current = locales.IndexOf(LocalizationSettings.SelectedLocale);
            var next = locales[(current + 1) % locales.Count];
            save.LanguageCode = next.Identifier.Code;
            LocalizationSettings.SelectedLocale = next;
        }

        void OnLocaleChanged(Locale locale)
        {
            settings.SetLanguageCode(locale.Identifier.Code);
            result.Refresh();
        }

        static string CurrentLanguageCode()
        {
            var locale = LocalizationSettings.SelectedLocale;
            return locale != null ? locale.Identifier.Code : "en";
        }
    }
}
