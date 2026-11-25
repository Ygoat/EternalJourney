namespace EternalJourney.Cores.Models.Crews;

using System.Linq;
using Chickensoft.GoDotTest;
using Godot;
using Shouldly;

public class CrewCommanderTest : TestClass
{
    public CrewCommanderTest(Node testScene) : base(testScene) { }

    private static CrewDefinition Worker(int id, CrewPersonality p) =>
        new(id, $"W{id}", CrewRole.Worker, p, new CrewPassive(CrewPassiveKind.Attack, null, 0f));

    private static readonly CrewDefinition[] Workers =
    {
        Worker(1, CrewPersonality.Brave),
        Worker(2, CrewPersonality.Coward),
        Worker(3, CrewPersonality.Steady),
        Worker(4, CrewPersonality.Brave),
    };

    private static readonly int[] AllManned = { 0, 1, 2, 3 };
    private static readonly int[] NoThreat = { 0, 0, 0, 0 };

    private static int Repairers(int[] plan) => plan.Count(s => s == CrewCommander.REPAIR);

    [Test]
    public void CowardCommanderSendsRepairEarlierThanBrave()
    {
        Repairers(CrewCommander.Plan(0.65f, NoThreat, Workers, CrewPersonality.Coward, AllManned)).ShouldBe(1);
        Repairers(CrewCommander.Plan(0.65f, NoThreat, Workers, CrewPersonality.Brave, AllManned)).ShouldBe(0);
        Repairers(CrewCommander.Plan(0.2f, NoThreat, Workers, CrewPersonality.Brave, AllManned)).ShouldBe(1);
    }

    [Test]
    public void CowardWorkerGoesToRepairFirst()
    {
        int[] plan = CrewCommander.Plan(0.6f, NoThreat, Workers, CrewPersonality.Coward, AllManned);
        plan[1].ShouldBe(CrewCommander.REPAIR);
        Repairers(plan).ShouldBe(1);
    }

    [Test]
    public void SecondRepairerBelowHalfThresholdAndReturnWithHysteresis()
    {
        // 臆病な指揮官: しきい値 0.7、その半分は 0.35
        int[] plan = CrewCommander.Plan(0.3f, NoThreat, Workers, CrewPersonality.Coward, AllManned);
        Repairers(plan).ShouldBe(2);

        // 0.4 は 0.35 を超えたが 0.35+0.15 未満なので2人のまま
        Repairers(CrewCommander.Plan(0.4f, NoThreat, Workers, CrewPersonality.Coward, plan)).ShouldBe(2);

        // 0.86 は 0.7+0.15 を超えたので全員が銃座に戻る
        Repairers(CrewCommander.Plan(0.86f, NoThreat, Workers, CrewPersonality.Coward, plan)).ShouldBe(0);
    }

    [Test]
    public void BusiestStationsAreMannedAndAssignmentsStayStable()
    {
        int[] threats = { 0, 5, 0, 3 };
        int[] plan = CrewCommander.Plan(0.6f, threats, Workers, CrewPersonality.Coward, AllManned);

        // 修理に1人回るので敵のいない銃座が1つ空く。敵の多い 1 と 3 は必ず埋まる
        plan.ShouldContain(1);
        plan.ShouldContain(3);
        // 修理に回らず、操作対象の銃座にいた者はそのまま残る
        plan[3].ShouldBe(3);
    }
}
