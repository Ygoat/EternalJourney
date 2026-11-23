namespace EternalJourney.Cores.Models.Research;

using System;
using System.Linq;
using Chickensoft.GoDotTest;
using Godot;
using Shouldly;

public class ResearchCardPickerTest : TestClass
{
    public ResearchCardPickerTest(Node testScene) : base(testScene) { }

    [Test]
    public void DrawsRequestedNumberOfCards()
    {
        ResearchCardPicker.Draw(new Random(1), 5).Count.ShouldBe(5);
    }

    [Test]
    public void DrawDoesNotRepeatCardType()
    {
        var rng = new Random(2);
        for (int i = 0; i < 200; i++)
        {
            var cards = ResearchCardPicker.Draw(rng, 5);
            cards.Select(c => c.Type).Distinct().Count().ShouldBe(cards.Count);
        }
    }

    [Test]
    public void MultiShotAppearsOnlyAtEpicOrAbove()
    {
        var rng = new Random(3);
        for (int i = 0; i < 2000; i++)
        {
            foreach (ResearchCard card in ResearchCardPicker.Draw(rng, 5).Where(c => c.Type == ResearchCardType.MultiShot))
            {
                ((int)card.Rarity).ShouldBeGreaterThanOrEqualTo((int)ResearchRarity.Epic);
            }
        }
    }

    [Test]
    public void RarityDistributionFollowsWeights()
    {
        var rng = new Random(4);
        const int n = 20000;
        int[] counts = new int[4];
        for (int i = 0; i < n; i++)
        {
            counts[(int)ResearchCardPicker.DrawRarity(rng)]++;
        }
        ((float)counts[0] / n).ShouldBe(0.60f, 0.02f);
        ((float)counts[1] / n).ShouldBe(0.25f, 0.02f);
        ((float)counts[2] / n).ShouldBe(0.12f, 0.02f);
        ((float)counts[3] / n).ShouldBe(0.03f, 0.01f);
    }
}
