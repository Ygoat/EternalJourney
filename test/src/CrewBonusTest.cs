namespace EternalJourney.Cores.Models.Crews;

using Chickensoft.GoDotTest;
using Godot;
using Shouldly;

public class CrewBonusTest : TestClass
{
    public CrewBonusTest(Node testScene) : base(testScene) { }

    private static CrewDefinition Crew(CrewRole role, CrewPassiveKind kind, WeaponKind? target, float value) =>
        new(1, "X", role, CrewPersonality.Steady, new CrewPassive(kind, target, value));

    [Test]
    public void CommanderAndWorkerPassivesCombine()
    {
        CrewDefinition commander = Crew(CrewRole.Commander, CrewPassiveKind.Attack, null, 0.1f);
        CrewDefinition worker = Crew(CrewRole.Worker, CrewPassiveKind.Attack, WeaponKind.Laser, 0.2f);

        CrewBonus.For(WeaponKind.Laser, commander, worker).AtkMultiplier.ShouldBe(1.3f, 0.0001f);
        // 作業クルーのパッシブは対象外の武器には効かない
        CrewBonus.For(WeaponKind.Bullet, commander, worker).AtkMultiplier.ShouldBe(1.1f, 0.0001f);
    }

    [Test]
    public void FireRateAndReloadConvertToMultipliers()
    {
        CrewDefinition worker = Crew(CrewRole.Worker, CrewPassiveKind.FireRate, WeaponKind.Bullet, 0.25f);
        CrewDefinition commander = Crew(CrewRole.Commander, CrewPassiveKind.ReloadSpeed, null, 0.8f);

        CrewBonus bonus = CrewBonus.For(WeaponKind.Bullet, commander, worker);
        bonus.FireIntervalMultiplier.ShouldBe(0.8f, 0.0001f);
        bonus.LockTimeMultiplier.ShouldBe(CrewBonus.MIN_LOCK_TIME_MULTIPLIER, 0.0001f);
        CrewBonus.For(WeaponKind.Bullet, null, null).ShouldBe(CrewBonus.None);
    }

    [Test]
    public void UnmannedStationUsesAutoModePenalty()
    {
        using var repo = new EternalJourney.Crew.Domain.CrewRepo();
        CrewBonus auto = repo.BonusFor(0, WeaponKind.Bullet);
        auto.AtkMultiplier.ShouldBe(0.7f, 0.0001f);
        // 発射速度 0.8 倍 = 発射間隔 1.25 倍
        auto.FireIntervalMultiplier.ShouldBe(1.25f, 0.0001f);

        repo.SetManned(0, Crew(CrewRole.Worker, CrewPassiveKind.Attack, WeaponKind.Laser, 0.2f));
        repo.BonusFor(0, WeaponKind.Bullet).ShouldBe(CrewBonus.None);
        // 銃座のない武器（敵の武器）は補正なし
        repo.BonusFor(-1, WeaponKind.Bullet).ShouldBe(CrewBonus.None);
    }

    [Test]
    public void RepairMultiplierAddsRepairPassives()
    {
        CrewDefinition commander = Crew(CrewRole.Commander, CrewPassiveKind.RepairSpeed, null, 0.5f);
        CrewDefinition worker = Crew(CrewRole.Worker, CrewPassiveKind.RepairSpeed, null, 0.5f);
        CrewBonus.RepairMultiplier(commander, worker).ShouldBe(2f, 0.0001f);
    }
}
