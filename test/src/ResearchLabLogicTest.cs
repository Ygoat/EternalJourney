namespace EternalJourney.ResearchLab.State;

using Chickensoft.GoDotTest;
using EternalJourney.Battle.Domain;
using Godot;
using Shouldly;

public class ResearchLabLogicTest : TestClass
{
    public ResearchLabLogicTest(Node testScene) : base(testScene) { }

    [Test]
    public void OutputsCostTextAndAvailability()
    {
        using var repo = new BattleRepo();
        var logic = new ResearchLabLogic();
        logic.Set<IBattleRepo>(repo);
        ResearchLabLogic.Output.AvailabilityChanged last = default;
        using var binding = logic.Bind();
        binding.OnOutput((in ResearchLabLogic.Output.AvailabilityChanged o) => last = o);

        logic.Start<ResearchLabState.Watching>();

        last.CostText.ShouldBe($"LAB {BattleRepo.INITIAL_RESEARCH_COST}");
        last.CanResearch.ShouldBeFalse();

        repo.AddScrap(BattleRepo.INITIAL_RESEARCH_COST);

        last.CanResearch.ShouldBeTrue();
        logic.Dispose();
    }

    [Test]
    public void AutoModeStartsResearchOnceUntilFinished()
    {
        using var repo = new BattleRepo();
        int started = 0;
        repo.ResearchStarted += () => started++;
        var logic = new ResearchLabLogic();
        logic.Set<IBattleRepo>(repo);
        logic.Start<ResearchLabState.Watching>();

        repo.SetAutoMode(true);
        // コストの3倍あっても、研究中は次の研究を始めない
        repo.AddScrap(BattleRepo.INITIAL_RESEARCH_COST * 3);
        started.ShouldBe(1);
        repo.Scrap.Value.ShouldBe(BattleRepo.INITIAL_RESEARCH_COST * 2);

        // カードを選んで研究 UI が閉じたら、次の研究が自動で始まる
        repo.ApplyResearchCard(new EternalJourney.Cores.Models.Research.ResearchCard(
            EternalJourney.Cores.Models.Research.ResearchCardType.AtkUp,
            EternalJourney.Cores.Models.Research.ResearchRarity.Common));
        repo.NotifyResearchFinished();
        started.ShouldBe(2);
        logic.Dispose();
    }
}
