namespace EternalJourney.Crew.State;

using Chickensoft.GoDotTest;
using EternalJourney.Battle.Domain;
using EternalJourney.Cores.Models.Crews;
using EternalJourney.Crew.Domain;
using Godot;
using Shouldly;

public class CrewMemberLogicTest : TestClass
{
    public CrewMemberLogicTest(Node testScene) : base(testScene) { }

    private static CrewMemberLogic Create(BattleRepo battleRepo, CrewRepo crewRepo, CrewDefinition worker)
    {
        var logic = new CrewMemberLogic();
        logic.Set<IBattleRepo>(battleRepo);
        logic.Set<ICrewRepo>(crewRepo);
        logic.Set(worker);
        logic.Start<CrewMemberState.Standby>();
        return logic;
    }

    [Test]
    public void ManningAfterArrivalAndLeavingClearsStation()
    {
        using var battleRepo = new BattleRepo();
        using var crewRepo = new CrewRepo();
        CrewDefinition worker = CrewCatalog.Workers[0];
        var logic = Create(battleRepo, crewRepo, worker);

        logic.Input(new CrewMemberLogic.Input.Assign(2));
        logic.State.ShouldBeOfType<CrewMemberState.Moving>();
        crewRepo.IsManned(2).ShouldBeFalse();

        logic.Input(new CrewMemberLogic.Input.Arrived());
        logic.State.ShouldBeOfType<CrewMemberState.Manning>();
        crewRepo.WorkerAt(2).ShouldBe(worker);

        logic.Input(new CrewMemberLogic.Input.Assign(CrewCommander.REPAIR));
        crewRepo.IsManned(2).ShouldBeFalse();
        logic.Dispose();
    }

    [Test]
    public void RepairingRequestsShipRepairEveryTick()
    {
        using var battleRepo = new BattleRepo();
        using var crewRepo = new CrewRepo();
        float repaired = 0f;
        battleRepo.ShipHealRequested += amount => repaired += amount;
        var logic = Create(battleRepo, crewRepo, CrewCatalog.Workers[0]);

        logic.Input(new CrewMemberLogic.Input.Place(CrewCommander.REPAIR));
        logic.State.ShouldBeOfType<CrewMemberState.Repairing>();
        logic.Input(new CrewMemberLogic.Input.Tick(1.0));

        repaired.ShouldBe(CrewMemberLogic.REPAIR_PER_SECOND, 0.001f);
        logic.Dispose();
    }
}
