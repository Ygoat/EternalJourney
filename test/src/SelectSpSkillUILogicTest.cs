namespace EternalJourney.SelectUI.State;

using Chickensoft.GoDotTest;
using EternalJourney.Cores.Models.Skill;
using EternalJourney.Game.Domain;
using Godot;
using Shouldly;

public class SelectSpSkillUILogicTest : TestClass
{
    public SelectSpSkillUILogicTest(Node testScene) : base(testScene) { }

    private static (SelectSpSkillUILogic logic, GameRepo repo) Create(out System.Func<SelectSpSkillUILogic.Output.SelectionChanged> last)
    {
        var repo = new GameRepo();
        var logic = new SelectSpSkillUILogic();
        logic.Set<IGameRepo>(repo);
        SelectSpSkillUILogic.Output.SelectionChanged latest = default;
        var binding = logic.Bind();
        binding.OnOutput((in SelectSpSkillUILogic.Output.SelectionChanged o) => latest = o);
        logic.Start<SelectSpSkillUIState.Idle>();
        last = () => latest;
        return (logic, repo);
    }

    [Test]
    public void StartsWithNoSelectionAndCannotConfirm()
    {
        var (logic, repo) = Create(out var last);
        int notified = 0;
        repo.SpSkillSelected += () => notified++;

        last().SelectedIndex.ShouldBe(SelectSpSkillUILogic.NO_SELECTION);
        last().CanConfirm.ShouldBeFalse();
        last().Description.ShouldBe(SelectSpSkillUILogic.NOT_SELECTED_DESCRIPTION);

        logic.Input(new SelectSpSkillUILogic.Input.ConfirmPressed());
        notified.ShouldBe(0);
        logic.Dispose();
    }

    [Test]
    public void SelectingAnotherReplacesPreviousSelection()
    {
        var (logic, _) = Create(out var last);

        logic.Input(new SelectSpSkillUILogic.Input.SpSkillToggled(0, true));
        logic.Input(new SelectSpSkillUILogic.Input.SpSkillToggled(2, true));

        last().SelectedIndex.ShouldBe(2);
        last().CanConfirm.ShouldBeTrue();
        last().Description.ShouldBe(SpSkillCatalog.All[2].Description);

        // 選択中のものを押し直すと未選択に戻る
        logic.Input(new SelectSpSkillUILogic.Input.SpSkillToggled(2, false));
        last().SelectedIndex.ShouldBe(SelectSpSkillUILogic.NO_SELECTION);
        logic.Dispose();
    }

    [Test]
    public void ConfirmSavesSelectedSpSkill()
    {
        var (logic, repo) = Create(out _);
        int notified = 0;
        repo.SpSkillSelected += () => notified++;

        logic.Input(new SelectSpSkillUILogic.Input.SpSkillToggled(1, true));
        logic.Input(new SelectSpSkillUILogic.Input.ConfirmPressed());

        repo.SelectedSpSkill.ShouldBe(SpSkillCatalog.All[1].Type);
        notified.ShouldBe(1);
        logic.Dispose();
    }
}
