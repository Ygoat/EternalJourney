namespace EternalJourney.Battle.Domain;

using Chickensoft.GoDotTest;
using EternalJourney.Cores.Models.Research;
using Godot;
using Shouldly;

public class BattleRepoResearchTest : TestClass
{
    public BattleRepoResearchTest(Node testScene) : base(testScene) { }

    [Test]
    public void TryStartResearchFailsWhenScrapIsShort()
    {
        using var repo = new BattleRepo();
        int started = 0;
        repo.ResearchStarted += () => started++;
        repo.AddScrap(BattleRepo.INITIAL_RESEARCH_COST - 1);

        repo.TryStartResearch().ShouldBeFalse();

        repo.Scrap.Value.ShouldBe(BattleRepo.INITIAL_RESEARCH_COST - 1);
        repo.ResearchCost.Value.ShouldBe(BattleRepo.INITIAL_RESEARCH_COST);
        started.ShouldBe(0);
    }

    [Test]
    public void TryStartResearchSpendsScrapAndRaisesCost()
    {
        using var repo = new BattleRepo();
        int started = 0;
        repo.ResearchStarted += () => started++;
        repo.AddScrap(BattleRepo.INITIAL_RESEARCH_COST + 3);

        repo.TryStartResearch().ShouldBeTrue();

        repo.Scrap.Value.ShouldBe(3);
        repo.ResearchCost.Value.ShouldBe(BattleRepo.INITIAL_RESEARCH_COST + BattleRepo.RESEARCH_COST_STEP);
        started.ShouldBe(1);
    }

    [Test]
    public void ApplyResearchCardStacksSameCard()
    {
        using var repo = new BattleRepo();
        var card = new ResearchCard(ResearchCardType.AtkUp, ResearchRarity.Rare);

        repo.ApplyResearchCard(card);
        repo.ApplyResearchCard(card);
        repo.ApplyResearchCard(new ResearchCard(ResearchCardType.AtkUp, ResearchRarity.Common));

        repo.AcquiredCards.Value.Count.ShouldBe(2);
        repo.AcquiredCards.Value[0].Count.ShouldBe(2);
        repo.Research.Value.AtkRate.ShouldBe(0.5f, 0.0001f);
    }

    [Test]
    public void AtkResearchIncreasesEnemyDamage()
    {
        using var repo = new BattleRepo();
        float before = 100f - repo.ReduceEnemyDurability(100f, 10f, 0f, 0f);
        repo.ApplyResearchCard(new ResearchCard(ResearchCardType.AtkUp, ResearchRarity.Legendary));
        float after = 100f - repo.ReduceEnemyDurability(100f, 10f, 0f, 0f);

        after.ShouldBe(before * 1.6f, 0.001f);
    }
}
