namespace EternalJourney.Cores.Models.Research;

using Chickensoft.GoDotTest;
using Godot;
using Shouldly;

public class ResearchBonusTest : TestClass
{
    public ResearchBonusTest(Node testScene) : base(testScene) { }

    [Test]
    public void AtkUpAccumulatesByRarity()
    {
        ResearchBonus bonus = new ResearchBonus()
            .Apply(new ResearchCard(ResearchCardType.AtkUp, ResearchRarity.Common))
            .Apply(new ResearchCard(ResearchCardType.AtkUp, ResearchRarity.Legendary));

        bonus.AtkRate.ShouldBe(0.7f, 0.0001f);
    }

    [Test]
    public void MultiShotAndPierceAreIntegers()
    {
        ResearchBonus bonus = new ResearchBonus()
            .Apply(new ResearchCard(ResearchCardType.MultiShot, ResearchRarity.Legendary))
            .Apply(new ResearchCard(ResearchCardType.PierceUp, ResearchRarity.Rare));

        bonus.ExtraShots.ShouldBe(2);
        bonus.PierceBonus.ShouldBe(2);
    }

    [Test]
    public void CooldownMultiplierHasLowerBound()
    {
        ResearchBonus bonus = new();
        for (int i = 0; i < 5; i++)
        {
            bonus = bonus.Apply(new ResearchCard(ResearchCardType.CooldownDown, ResearchRarity.Legendary));
        }

        bonus.CooldownMultiplier.ShouldBe(ResearchBonus.MIN_INTERVAL_MULTIPLIER, 0.0001f);
    }

    [Test]
    public void FireIntervalMultiplierDecreases()
    {
        ResearchBonus bonus = new ResearchBonus()
            .Apply(new ResearchCard(ResearchCardType.FireRateUp, ResearchRarity.Epic));

        bonus.FireIntervalMultiplier.ShouldBe(0.75f, 0.0001f);
    }

    [Test]
    public void DefaultBonusHasNoEffect()
    {
        ResearchBonus bonus = new();

        bonus.FireIntervalMultiplier.ShouldBe(1f);
        bonus.CooldownMultiplier.ShouldBe(1f);
        bonus.ReachMultiplier.ShouldBe(1f);
        bonus.ExtraShots.ShouldBe(0);
    }
}
