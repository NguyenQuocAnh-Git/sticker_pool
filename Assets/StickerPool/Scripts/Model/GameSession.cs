using System;
using System.Collections.Generic;

namespace StickerPool.Model
{
    /// <summary>State of one play of a level. Pure rules, no engine types.</summary>
    public sealed class GameSession
    {
        const int MaxLayoutAttempts = 80;
        const int RerollsPerLevel = 1;

        readonly SortedSet<int> alive = new SortedSet<int>();
        string[] slots = new string[OfferGenerator.SlotCount];

        GameSession(LevelDef level, IReadOnlyList<PlacedSticker> stickers, uint run, int budget)
        {
            Level = level;
            Stickers = stickers;
            Run = run;
            Budget = budget;
            RerollsLeft = RerollsPerLevel;
            foreach (var s in stickers) alive.Add(s.Id);
            slots = NewSlots();
        }

        public LevelDef Level { get; }
        public IReadOnlyList<PlacedSticker> Stickers { get; }
        public uint Run { get; }
        public int Budget { get; }
        public int Used { get; private set; }
        public int RerollsLeft { get; private set; }
        public int RerollIndex { get; private set; }

        public IReadOnlyCollection<int> Alive => alive;
        public int MovesLeft => Budget - Used;
        public bool IsCleared => alive.Count == 0;
        public bool IsOutOfMoves => !IsCleared && Used >= Budget;
        public bool IsOver => IsCleared || Used >= Budget;

        /// <summary>3 stars with 2+ spare moves, 2 with one, otherwise 1.</summary>
        public int Stars => MovesLeft >= 2 ? 3 : MovesLeft == 1 ? 2 : 1;

        /// <summary>
        /// Same move budget every attempt: reshuffle until this layout's greedy solution fits it.
        /// </summary>
        public static GameSession Create(LevelDef level, IReadOnlyList<StickerDef> items,
            IReadOnlyList<string> colorTags, BudgetCalculator budgets, Func<uint> nextRunSeed)
        {
            int solveMoves = budgets.SolveMoves(level);
            uint run = 0;
            List<PlacedSticker> stickers = null;
            for (int attempt = 0; attempt < MaxLayoutAttempts; attempt++)
            {
                run = nextRunSeed();
                stickers = LevelGenerator.Generate(level, items, colorTags, run);
                if (GreedySolver.CountMoves(level, stickers, run) <= solveMoves) break;
            }

            return new GameSession(level, stickers, run, solveMoves + level.SpareMoves);
        }

        /// <summary>Same as <see cref="Create"/> but for an explicit run seed (tests, replays).</summary>
        public static GameSession CreateForRun(LevelDef level, IReadOnlyList<StickerDef> items,
            IReadOnlyList<string> colorTags, BudgetCalculator budgets, uint run)
        {
            var stickers = LevelGenerator.Generate(level, items, colorTags, run);
            return new GameSession(level, stickers, run, budgets.SolveMoves(level) + level.SpareMoves);
        }

        /// <summary>The word on each of the three tapes (null = empty slot). Tapes keep their slot between turns.</summary>
        public IReadOnlyList<string> Slots => slots;

        public List<int> HitsFor(string tagId) => OfferGenerator.HitsFor(Stickers, alive, tagId);

        /// <summary>
        /// Spends a move and removes every sticker carrying the tag. Returns their ids.
        /// Only the picked tape (and any tape left with nothing to sweep) gets a new word.
        /// </summary>
        public List<int> Choose(string tagId)
        {
            var hits = HitsFor(tagId);
            Used++;
            RerollIndex = 0;
            foreach (int id in hits) alive.Remove(id);

            for (int i = 0; i < slots.Length; i++)
                if (slots[i] == tagId) slots[i] = null;
            slots = IsOver ? new string[OfferGenerator.SlotCount] : OfferGenerator.Refill(Level, Stickers, alive, 0, Run, slots);
            return hits;
        }

        public bool CanReroll => RerollsLeft > 0 && !IsOver;

        /// <summary>Swaps all three words for a fresh set.</summary>
        public bool TryReroll()
        {
            if (!CanReroll) return false;
            RerollsLeft--;
            RerollIndex++;
            slots = NewSlots();
            return true;
        }

        string[] NewSlots()
        {
            var result = new string[OfferGenerator.SlotCount];
            if (IsOver) return result;
            var offers = OfferGenerator.Make(Level, Stickers, alive, RerollIndex, Run);
            for (int i = 0; i < result.Length && i < offers.Count; i++) result[i] = offers[i];
            return result;
        }
    }
}
