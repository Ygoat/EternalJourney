namespace EternalJourney.ResearchUI.State;

using System;
using System.Collections.Generic;
using System.Linq;
using Chickensoft.GoDotTest;
using EternalJourney.Battle.Domain;
using Godot;
using Shouldly;

public class ResearchUILogicAutoTest : TestClass
{
    public ResearchUILogicAutoTest(Node testScene) : base(testScene) { }

    [Test]
    public void AutoModePicksHighestRarityAfterDelay()
    {
        using var repo = new BattleRepo();
        var logic = new ResearchUILogic();
        logic.Set<IBattleRepo>(repo);
        logic.Set(new ResearchUILogic.Settings(Duration: 0.1f, CardCount: 5, AutoPickDelay: 0.5f));
        logic.Set(new Random(21));
        IReadOnlyList<ResearchUILogic.CardView> shown = Array.Empty<ResearchUILogic.CardView>();
        using var binding = logic.Bind();
        binding.OnOutput((in ResearchUILogic.Output.CardsShown o) => shown = o.Cards);
        logic.Start<ResearchUIState.Hidden>();

        repo.SetAutoMode(true);
        repo.AddScrap(BattleRepo.INITIAL_RESEARCH_COST);
        repo.TryStartResearch().ShouldBeTrue();
        logic.Input(new ResearchUILogic.Input.Process(0.2));
        logic.State.ShouldBeOfType<ResearchUIState.Choosing>();

        // 待ち時間の前は選ばない
        logic.Input(new ResearchUILogic.Input.Process(0.3));
        logic.State.ShouldBeOfType<ResearchUIState.Choosing>();

        logic.Input(new ResearchUILogic.Input.Process(0.3));
        logic.State.ShouldBeOfType<ResearchUIState.Hidden>();
        repo.AcquiredCards.Value.Count.ShouldBe(1);
        repo.AcquiredCards.Value[0].Rarity.ShouldBe(shown.Max(c => c.Rarity));
        logic.Dispose();
    }

    [Test]
    public void ManualModeWaitsForSelection()
    {
        using var repo = new BattleRepo();
        var logic = new ResearchUILogic();
        logic.Set<IBattleRepo>(repo);
        logic.Set(new ResearchUILogic.Settings(Duration: 0.1f, CardCount: 5, AutoPickDelay: 0.5f));
        logic.Set(new Random(21));
        logic.Start<ResearchUIState.Hidden>();

        repo.AddScrap(BattleRepo.INITIAL_RESEARCH_COST);
        repo.TryStartResearch();
        logic.Input(new ResearchUILogic.Input.Process(0.2));
        logic.Input(new ResearchUILogic.Input.Process(5.0));

        logic.State.ShouldBeOfType<ResearchUIState.Choosing>();
        repo.AcquiredCards.Value.Count.ShouldBe(0);
        logic.Dispose();
    }
}
