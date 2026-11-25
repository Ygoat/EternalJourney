namespace EternalJourney.SelectUI.State;

using System;
using System.Linq;
using Chickensoft.GoDotTest;
using EternalJourney.Game.Domain;
using EternalJourney.Skills;
using Godot;
using Shouldly;

public class SelectSkillUILogicTest : TestClass
{
    public SelectSkillUILogicTest(Node testScene) : base(testScene) { }

    private static SelectSkillUILogic Create(GameRepo repo, out Func<SelectSkillUILogic.Output.SelectionChanged> last)
    {
        var logic = new SelectSkillUILogic();
        logic.Set<IGameRepo>(repo);
        logic.Set(new Random(9));
        SelectSkillUILogic.Output.SelectionChanged latest = default;
        var binding = logic.Bind();
        binding.OnOutput((in SelectSkillUILogic.Output.SelectionChanged o) => latest = o);
        logic.Start<SelectSkillUIState.Idle>();
        last = () => latest;
        return logic;
    }

    [Test]
    public void FifthSkillIsRejected()
    {
        using var repo = new GameRepo();
        var logic = Create(repo, out var last);

        for (int i = 0; i < 5; i++)
        {
            logic.Input(new SelectSkillUILogic.Input.SkillToggled(i, true));
        }

        last().Selected.Count(s => s).ShouldBe(SelectSkillUILogic.MAX_SKILLS);
        last().Selected[4].ShouldBeFalse();

        logic.Input(new SelectSkillUILogic.Input.SelectButtonPressed());
        repo.SelectedSkills.ShouldBe(SkillRegistry.SelectableSkills.Take(4).ToList());
        logic.Dispose();
    }

    [Test]
    public void RandomSelectsExactlyFour()
    {
        using var repo = new GameRepo();
        var logic = Create(repo, out var last);

        logic.Input(new SelectSkillUILogic.Input.RandomPressed());

        last().Selected.Count(s => s).ShouldBe(SelectSkillUILogic.MAX_SKILLS);
        logic.Dispose();
    }
}
