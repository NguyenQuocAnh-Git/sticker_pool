using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using StickerPool.Data;
using StickerPool.Model;
using UnityEditor;
using UnityEngine;

namespace StickerPool.Tests
{
    /// <summary>
    /// Compares the C# rules with values produced by the HTML prototype's own JavaScript
    /// (ParityGolden.json, made by running prototype/index.html logic in Node).
    /// </summary>
    public sealed class PrototypeParityTests
    {
        const string GoldenPath = "Assets/StickerPool/Tests/EditMode/ParityGolden.json";
        const string DatabasePath = "Assets/StickerPool/Data/GameDatabase.asset";
        const double Tolerance = 1e-12;

        static readonly string[] ColorNames = { "Đỏ", "Cam", "Vàng", "Xanh lá", "Xanh dương", "Nâu" };

        JObject golden;

        [OneTimeSetUp]
        public void LoadGolden()
        {
            golden = JObject.Parse(File.ReadAllText(GoldenPath));
        }

        [Test]
        public void ModelMatchesPrototype_BudgetsStickersAndOffers()
        {
            var items = golden["items"]
                .Select(it => new StickerDef((string)it["name"], (string)it["color"],
                    it["tags"].Select(t => (string)t).ToList()))
                .ToList();

            RunParity(items, ColorNames.ToList(), BuildLevelDefsFromGolden());
        }

        [Test]
        public void ShippedDatabaseMatchesPrototype_BudgetsStickersAndOffers()
        {
            var db = AssetDatabase.LoadAssetAtPath<GameDatabase>(DatabasePath);
            Assert.IsNotNull(db, "GameDatabase asset is missing at " + DatabasePath);
            Assert.AreEqual(golden["items"].Count(), db.StickerDefs.Count, "sticker count");
            Assert.AreEqual(golden["levels"].Count(), db.LevelDefs.Count, "level count");

            for (int i = 0; i < db.StickerDefs.Count; i++)
            {
                var gold = golden["items"][i];
                Assert.AreEqual(gold["tags"].Count(), db.StickerDefs[i].Tags.Count,
                    $"tag count of sticker #{i} ({db.StickerDefs[i].Id})");
            }

            RunParity(db.StickerDefs, db.ColorTagIds, db.LevelDefs);
        }

        List<LevelDef> BuildLevelDefsFromGolden()
        {
            var list = new List<LevelDef>();
            int number = 1;
            foreach (var lv in golden["levels"])
            {
                list.Add(new LevelDef(number++, (int)lv["n"], (int)lv["k"],
                    lv["tags"].Select(t => (string)t).ToList(),
                    lv["focus"].Type == JTokenType.Null ? null : (string)lv["focus"],
                    (int)lv["slack"], (int)lv["seed"]));
            }

            return list;
        }

        void RunParity(IReadOnlyList<StickerDef> items, IReadOnlyList<string> colors, IReadOnlyList<LevelDef> levels)
        {
            var calculator = new BudgetCalculator(items, colors);

            for (int li = 0; li < levels.Count; li++)
            {
                var level = levels[li];
                var gold = golden["levels"][li];
                string label = $"level {li + 1}";

                Assert.AreEqual((int)gold["n"], level.StickerCount, label + " sticker count");
                Assert.AreEqual((int)gold["slack"], level.SpareMoves, label + " spare moves");
                Assert.AreEqual((int)gold["seed"], level.Seed, label + " seed");
                Assert.AreEqual(((JArray)gold["tags"]).Count, level.OfferableTags.Count, label + " offerable tags");

                Assert.AreEqual((int)gold["budget"], calculator.SolveMoves(level), label + " budget");

                // The prototype names tags in Vietnamese; the offerable lists line up one to one with ours.
                var tagNames = new Dictionary<string, string>();
                for (int t = 0; t < level.OfferableTags.Count; t++)
                    tagNames[(string)gold["tags"][t]] = level.OfferableTags[t];

                foreach (var runGold in gold["runs"])
                {
                    uint run = (uint)(double)runGold["run"];
                    string runLabel = $"{label} run {run}";
                    var placed = LevelGenerator.Generate(level, items, colors, run);
                    var expected = (JArray)runGold["stickers"];

                    Assert.AreEqual(expected.Count, placed.Count, runLabel + " count");
                    for (int i = 0; i < placed.Count; i++)
                    {
                        Assert.AreEqual((int)expected[i][0], IndexOf(items, placed[i].Def), $"{runLabel} sticker #{i}");
                        Assert.AreEqual((double)expected[i][1], placed[i].X, Tolerance, $"{runLabel} x #{i}");
                        Assert.AreEqual((double)expected[i][2], placed[i].Rotation, Tolerance, $"{runLabel} rot #{i}");
                    }

                    Assert.AreEqual((int)runGold["greedy"], GreedySolver.CountMoves(level, placed, run), runLabel + " greedy");

                    var all = new SortedSet<int>(placed.Select(p => p.Id));
                    var partial = new SortedSet<int>(placed.Select(p => p.Id).Where(id => id % 3 != 0));
                    var cases = new (SortedSet<int> alive, int reroll)[]
                    {
                        (all, 0), (all, 1), (partial, 0), (partial, 2),
                    };

                    for (int c = 0; c < cases.Length; c++)
                    {
                        var offers = OfferGenerator.Make(level, placed, cases[c].alive, cases[c].reroll, run);
                        var expectedOffers = ((JArray)runGold["offers"][c]).Select(t => tagNames[(string)t]).ToList();
                        CollectionAssert.AreEqual(expectedOffers, offers, $"{runLabel} offers case {c}");
                    }
                }
            }
        }

        static int IndexOf(IReadOnlyList<StickerDef> items, StickerDef def)
        {
            for (int i = 0; i < items.Count; i++)
                if (ReferenceEquals(items[i], def)) return i;
            return -1;
        }
    }
}
