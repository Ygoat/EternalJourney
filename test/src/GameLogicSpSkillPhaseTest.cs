namespace EternalJourney.Game.State;

using Chickensoft.GoDotTest;
using EternalJourney.Game.Domain;
using Godot;
using Shouldly;

public class GameLogicSpSkillPhaseTest : TestClass
{
    public GameLogicSpSkillPhaseTest(Node testScene) : base(testScene) { }

    [Test]
    public void SpSkillSelectionComesBetweenSkillSelectionAndBattleInit()
    {
        using var repo = new GameRepo();
        var logic = new GameLogic();
        logic.Set<IGameRepo>(repo);
        int spScreenShown = 0;
        using var binding = logic.Bind();
        binding.OnOutput((in GameLogic.Output.ShowSelectSpSkill _) => spScreenShown++);
        logic.Start<GameState.SelectSkillPhase>();

        repo.NotifySkillSelected();
        logic.State.ShouldBeOfType<GameState.SelectSpSkillPhase>();
        spScreenShown.ShouldBe(1);

        repo.NotifySpSkillSelected();
        logic.State.ShouldBeOfType<GameState.SelectCrewPhase>();

        repo.NotifyCrewSelected();
        logic.State.ShouldBeOfType<GameState.BattleInitPhase>();
        logic.Dispose();
    }
}
