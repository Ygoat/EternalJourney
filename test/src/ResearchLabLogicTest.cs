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
}
