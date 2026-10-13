using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using StickerPool.Data;
using StickerPool.Model;
using UnityEditor;

namespace StickerPool.Tests
{
    /// <summary>After a turn only the picked tape (or a tape with nothing left) may change its word.</summary>
    public sealed class TapeSlotTests
    {
        const string DatabasePath = "Assets/StickerPool/Data/GameDatabase.asset";

        [Test]
        public void ChoosingATape_KeepsTheOtherUsefulTapesInPlace()
        {
            var db = AssetDatabase.LoadAssetAtPath<GameDatabase>(DatabasePath);
            var calculator = new BudgetCalculator(db.StickerDefs, db.ColorTagIds);
            int checkedTurns = 0;

            for (int level = 0; level < db.LevelDefs.Count; level++)
            for (uint run = 1; run <= 25; run++)
            {
                var session = GameSession.CreateForRun(db.LevelDefs[level], db.StickerDefs, db.ColorTagIds, calculator, run * 7919u);
                while (!session.IsOver)
                {
                    var before = session.Slots.ToArray();
                    Assert.AreEqual(OfferGenerator.SlotCount, before.Length);
                    Assert.AreEqual(before.Where(t => t != null).Distinct().Count(), before.Count(t => t != null),
                        "duplicate word on the tapes");
                    Assert.That(before.All(t => t == null || session.HitsFor(t).Count > 0), "a tape with nothing to sweep");

                    int pick = (int)((run + (uint)checkedTurns) % (uint)before.Count(t => t != null));
                    string chosen = before.Where(t => t != null).ElementAt(pick);
                    int chosenSlot = System.Array.IndexOf(before, chosen);
                    session.Choose(chosen);
                    var after = session.Slots.ToArray();
                    checkedTurns++;

                    if (session.IsOver)
                    {
                        Assert.That(after.All(t => t == null), "tapes must empty when the level ends");
                        break;
                    }

                    for (int i = 0; i < before.Length; i++)
                    {
                        if (i == chosenSlot) continue;
                        bool stillUseful = before[i] != null && session.HitsFor(before[i]).Count > 0;
                        if (stillUseful)
                            Assert.AreEqual(before[i], after[i], $"level {level + 1} run {run}: tape {i} must stay");
                    }

                    Assert.AreNotEqual(chosen, after[chosenSlot], "the picked tape needs a new word");
                    Assert.AreEqual(after.Where(t => t != null).Distinct().Count(), after.Count(t => t != null));
                    Assert.That(after.Any(t => t != null), "stickers remain, so at least one tape must be offered");
                }
            }

            Assert.Greater(checkedTurns, 100);
        }

        [Test]
        public void EveryTurn_OffersTheBiggestSweepOfTheFreshOffers()
        {
            var db = AssetDatabase.LoadAssetAtPath<GameDatabase>(DatabasePath);
            var calculator = new BudgetCalculator(db.StickerDefs, db.ColorTagIds);

            for (int level = 0; level < db.LevelDefs.Count; level++)
            {
                var session = GameSession.CreateForRun(db.LevelDefs[level], db.StickerDefs, db.ColorTagIds, calculator, 4242u);
                while (!session.IsOver)
                {
                    var fresh = OfferGenerator.Make(session.Level, session.Stickers, session.Alive, session.RerollIndex, session.Run);
                    int freshBest = fresh.Max(t => session.HitsFor(t).Count);
                    int shownBest = session.Slots.Where(t => t != null).Max(t => session.HitsFor(t).Count);
                    Assert.GreaterOrEqual(shownBest, freshBest, $"level {level + 1}: the best sweep must stay available");

                    string greedy = session.Slots.Where(t => t != null).OrderByDescending(t => session.HitsFor(t).Count).First();
                    session.Choose(greedy);
                }
            }
        }
    }
}
