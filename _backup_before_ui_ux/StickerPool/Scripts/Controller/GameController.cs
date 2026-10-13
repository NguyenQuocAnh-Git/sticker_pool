using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using StickerPool.Data;
using StickerPool.Model;
using StickerPool.View;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

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

        [Header("Views")]
        [SerializeField] JarView jar;
        [SerializeField] HudView hud;
        [SerializeField] TapeView[] tapes;
        [SerializeField] RollCallView rollCall;
        [SerializeField] ResultPanelView result;
        [SerializeField] SfxPlayer sfx;
        [SerializeField] BurstView popBurst;
        [SerializeField] BurstView winConfetti;

        [Header("Texts")]
        [SerializeField] LocalizedString greatText;
        [SerializeField] LocalizedString awesomeText;

        readonly Dictionary<int, StickerView> viewsById = new Dictionary<int, StickerView>();

        SaveService save;
        StickerObjectPool stickerPool;
        BudgetCalculator budgets;
        ChooseSequence chooseSequence;
        GameSession session;
        int levelIndex;
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
            hud.SoundClicked += OnSoundClicked;
            hud.RerollClicked += OnRerollClicked;
            hud.RestartClicked += OnRestartClicked;
            hud.LanguageClicked += OnLanguageClicked;
            hud.LevelSelected += OnLevelSelected;
            result.NextClicked += OnNextClicked;
            result.AgainClicked += OnRestartClicked;
            result.RetryClicked += OnRestartClicked;
        }

        IEnumerator Start()
        {
            yield return LocalizationSettings.InitializationOperation;
            ApplySavedLocale();
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;

            sfx.Enabled = save.SoundOn;
            hud.SetSound(sfx.Enabled);
            StartLevel(FirstUnclearedLevel());
        }

        void OnDestroy()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            foreach (var tape in tapes) tape.Clicked -= OnTapeClicked;
            hud.SoundClicked -= OnSoundClicked;
            hud.RerollClicked -= OnRerollClicked;
            hud.RestartClicked -= OnRestartClicked;
            hud.LanguageClicked -= OnLanguageClicked;
            hud.LevelSelected -= OnLevelSelected;
            result.NextClicked -= OnNextClicked;
            result.AgainClicked -= OnRestartClicked;
            result.RetryClicked -= OnRestartClicked;
        }

        // ---- Level flow ----

        int FirstUnclearedLevel()
        {
            for (int i = 0; i < database.LevelCount; i++)
                if (!save.IsCleared(i)) return Mathf.Min(i, UnlockedCount - 1);
            return 0;
        }

        void StartLevel(int index)
        {
            StopAllCoroutines();
            Tween.StopAll();

            busy = false;
            levelIndex = Mathf.Clamp(index, 0, database.LevelCount - 1);
            rollCall.Clear();
            result.Hide();
            jar.SetDim(0f);
            stickerPool.ReleaseAll();
            viewsById.Clear();

            var levelDef = database.LevelDefs[levelIndex];
            session = GameSession.Create(levelDef, database.StickerDefs, database.ColorTagIds, budgets, NextRunSeed);

            foreach (var placed in session.Stickers)
            {
                var data = database.GetSticker(placed.Def.Id);
                var view = stickerPool.Get(data);
                view.Place(data, placed.Id, SpawnPosition(placed), (float)placed.Rotation);
                viewsById[placed.Id] = view;
            }

            hud.ShowInfo(database.Levels[levelIndex].Hint);
            RefreshHud();
            RefreshTapes(config.tapeFirstDelay);
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

        void RefreshHud()
        {
            hud.SetMoves(session.MovesLeft, false);
            hud.SetStickersLeft(session.Alive.Count);
            hud.SetRerollsLeft(session.RerollsLeft, session.CanReroll && !busy);
            hud.SetLevels(levelIndex, save.IsCleared, UnlockedCount);
            hud.SetLanguageCode(CurrentLanguageCode());
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
            hud.ShowInfoText($"<b>{view.Data.DisplayName.GetLocalizedString()}</b>");
            view.Poke(new Vector2(Random.Range(-config.pokeSideSpeed, config.pokeSideSpeed), config.pokeUpSpeed));
        }

        void OnTapeClicked(TapeView tape)
        {
            if (busy || session == null || session.IsOver) return;
            StartCoroutine(ResolveChoice(tape));
        }

        IEnumerator ResolveChoice(TapeView tape)
        {
            busy = true;
            hud.SetBusy(true, UnlockedCount);

            var tag = tape.Tag;
            var hitIds = session.Choose(tag.Id);
            hud.SetMoves(session.MovesLeft, true);
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
            yield return new WaitForSeconds(config.tapeBounceDuration);
            yield return SwapTapes();

            busy = false;
            hud.SetBusy(false, UnlockedCount);
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
            save.MarkCleared(levelIndex);
            hud.SetLevels(levelIndex, save.IsCleared, UnlockedCount);
            bool isFinal = levelIndex == database.LevelCount - 1;
            result.ShowWin(session.Stars, session.Used, session.Budget, isFinal);
            winConfetti.PlayAt(jar.Center);
        }

        void Lose() => result.ShowLose(session.Alive.Count);

        // ---- Tools ----

        void OnRestartClicked()
        {
            if (!busy) StartLevel(levelIndex);
        }

        void OnNextClicked()
        {
            if (!busy) StartLevel(levelIndex + 1);
        }

        void OnLevelSelected(int index)
        {
            if (!busy && index < UnlockedCount) StartLevel(index);
        }

        void OnRerollClicked()
        {
            if (busy || session == null || !session.TryReroll()) return;
            StartCoroutine(RerollRoutine());
        }

        IEnumerator RerollRoutine()
        {
            busy = true;
            hud.SetBusy(true, UnlockedCount);
            RefreshHud();
            hud.ShowRerolled();
            yield return SwapTapes();
            busy = false;
            hud.SetBusy(false, UnlockedCount);
            RefreshHud();
        }

        void OnSoundClicked()
        {
            sfx.Enabled = !sfx.Enabled;
            save.SoundOn = sfx.Enabled;
            hud.SetSound(sfx.Enabled);
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
            hud.SetLanguageCode(locale.Identifier.Code);
            result.Refresh();
        }

        static string CurrentLanguageCode()
        {
            var locale = LocalizationSettings.SelectedLocale;
            return locale != null ? locale.Identifier.Code : "en";
        }
    }
}
