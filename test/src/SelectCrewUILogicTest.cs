namespace EternalJourney.SelectUI.State;

using System;
using System.Linq;
using Chickensoft.GoDotTest;
using EternalJourney.Cores.Models.Crews;
using EternalJourney.Game.Domain;
using Godot;
using Shouldly;

public class SelectCrewUILogicTest : TestClass
{
    public SelectCrewUILogicTest(Node testScene) : base(testScene) { }

    private static SelectCrewUILogic Create(GameRepo repo, out Func<SelectCrewUILogic.Output.SelectionChanged> last)
    {
        var logic = new SelectCrewUILogic();
        logic.Set<IGameRepo>(repo);
        logic.Set(new Random(3));
        SelectCrewUILogic.Output.SelectionChanged latest = default;
        var binding = logic.Bind();
        binding.OnOutput((in SelectCrewUILogic.Output.SelectionChanged o) => latest = o);
        logic.Start<SelectCrewUIState.Idle>();
        last = () => latest;
        return logic;
    }

    [Test]
    public void OnlyOneCommanderCanBeSelected()
    {
        using var repo = new GameRepo();
        var logic = Create(repo, out var last);

        logic.Input(new SelectCrewUILogic.Input.CommanderToggled(0, true));
        logic.Input(new SelectCrewUILogic.Input.CommanderToggled(2, true));
        last().CommanderIndex.ShouldBe(2);
        logic.Dispose();
    }

    [Test]
    public void RandomSelectsCommanderAndFourWorkers()
    {
        using var repo = new GameRepo();
        var logic = Create(repo, out var last);

        logic.Input(new SelectCrewUILogic.Input.RandomPressed());

        last().CommanderIndex.ShouldBeInRange(0, CrewCatalog.Commanders.Count - 1);
        last().WorkerSelected.Count(s => s).ShouldBe(CrewCatalog.WORKER_COUNT);
        last().CanConfirm.ShouldBeTrue();
        logic.Dispose();
    }

    [Test]
    public void FifthWorkerIsRejectedAndConfirmNeedsFour()
    {
        using var repo = new GameRepo();
        int notified = 0;
        repo.CrewSelected += () => notified++;
        var logic = Create(repo, out var last);

        logic.Input(new SelectCrewUILogic.Input.CommanderToggled(1, true));
        for (int i = 0; i < 3; i++)
        {
            logic.Input(new SelectCrewUILogic.Input.WorkerToggled(i, true));
        }
        last().CanConfirm.ShouldBeFalse();
        logic.Input(new SelectCrewUILogic.Input.ConfirmPressed());
        notified.ShouldBe(0);

        logic.Input(new SelectCrewUILogic.Input.WorkerToggled(5, true));
        logic.Input(new SelectCrewUILogic.Input.WorkerToggled(6, true));
        last().WorkerSelected[6].ShouldBeFalse();
        last().CanConfirm.ShouldBeTrue();

        logic.Input(new SelectCrewUILogic.Input.ConfirmPressed());
        notified.ShouldBe(1);
        repo.SelectedCommander.ShouldBe(CrewCatalog.Commanders[1]);
        repo.SelectedWorkers.ShouldBe(new[]
        {
            CrewCatalog.Workers[0], CrewCatalog.Workers[1], CrewCatalog.Workers[2], CrewCatalog.Workers[5],
        });
        logic.Dispose();
    }
}
