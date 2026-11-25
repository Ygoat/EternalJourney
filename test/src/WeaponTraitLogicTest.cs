namespace EternalJourney.WeaponTrait.State;

using Chickensoft.GoDotTest;
using EternalJourney.Cores.Models.Weapon;
using Godot;
using Shouldly;

public class WeaponTraitLogicTest : TestClass
{
    public WeaponTraitLogicTest(Node testScene) : base(testScene) { }

    private static WeaponTraitLogic.Output.GaugeChanged _last;

    private static WeaponTraitLogic Create(WeaponTraitSettings settings)
    {
        var logic = new WeaponTraitLogic();
        var binding = logic.Bind();
        binding.OnOutput((in WeaponTraitLogic.Output.GaugeChanged o) => _last = o);
        logic.Start<WeaponTraitState.Disabled>();
        logic.Input(new WeaponTraitLogic.Input.Configure(settings));
        return logic;
    }

    private static void Fire(WeaponTraitLogic logic, int times)
    {
        for (int i = 0; i < times; i++)
        {
            logic.Input(new WeaponTraitLogic.Input.Fired());
        }
    }

    private static void Tick(WeaponTraitLogic logic, double seconds, bool engaged = true, double step = 0.1)
    {
        for (double t = 0; t < seconds - 1e-6; t += step)
        {
            logic.Input(new WeaponTraitLogic.Input.Tick(step, engaged));
        }
    }

    [Test]
    public void ReloadLocksWhenEmptyAndRefillsAfterLockTime()
    {
        var logic = Create(new WeaponTraitSettings(WeaponTraitType.Reload, Capacity: 12, CostPerShot: 1, LockTime: 2));

        Fire(logic, 11);
        logic.State.ShouldBeOfType<WeaponTraitState.Ready>();
        Fire(logic, 1);
        logic.State.ShouldBeOfType<WeaponTraitState.Locked>();
        _last.Locked.ShouldBeTrue();

        Tick(logic, 1.9);
        logic.State.ShouldBeOfType<WeaponTraitState.Locked>();
        Tick(logic, 0.2);
        logic.State.ShouldBeOfType<WeaponTraitState.Ready>();
        _last.Ratio.ShouldBe(1f, 0.001f);
        logic.Dispose();
    }

    [Test]
    public void OverheatLocksAtCapacityAndCoolsAfterDelay()
    {
        var logic = Create(new WeaponTraitSettings(WeaponTraitType.Overheat, Capacity: 100, CostPerShot: 25,
            RecoverRate: 50, RecoverDelay: 0.3f, LockTime: 2));

        Fire(logic, 2);
        Tick(logic, 0.2);
        _last.Ratio.ShouldBe(0.5f, 0.001f); // 冷却待ち中は下がらない
        Tick(logic, 0.4);
        _last.Ratio.ShouldBeLessThan(0.5f);

        Fire(logic, 4);
        logic.State.ShouldBeOfType<WeaponTraitState.Locked>();
        Tick(logic, 2.1);
        logic.State.ShouldBeOfType<WeaponTraitState.Ready>();
        _last.Ratio.ShouldBe(0f, 0.001f);
        logic.Dispose();
    }

    [Test]
    public void ChargeFillsOnlyWhileEngagedAndFiresOnce()
    {
        var settings = new WeaponTraitSettings(WeaponTraitType.Charge, Capacity: 1, CostPerShot: 1, LockTime: 1.5f, DamageMultiplier: 3);
        var logic = Create(settings);

        logic.State.ShouldBeOfType<WeaponTraitState.Locked>();
        Tick(logic, 2.0, engaged: false);
        logic.State.ShouldBeOfType<WeaponTraitState.Locked>();
        _last.Ratio.ShouldBe(0f);

        Tick(logic, 1.6, engaged: true);
        logic.State.ShouldBeOfType<WeaponTraitState.Ready>();
        Fire(logic, 1);
        logic.State.ShouldBeOfType<WeaponTraitState.Locked>();
        settings.ShotDamageMultiplier.ShouldBe(3f);
        logic.Dispose();
    }

    [Test]
    public void BatteryResumesAtThreshold()
    {
        var logic = Create(new WeaponTraitSettings(WeaponTraitType.Battery, Capacity: 100, CostPerShot: 10,
            RecoverRate: 20, RecoverDelay: 0.5f, ResumeRatio: 0.3f));

        Fire(logic, 10);
        logic.State.ShouldBeOfType<WeaponTraitState.Locked>();
        // 0.5秒待ってから毎秒20回復 → 30% まで 1.5 秒
        Tick(logic, 1.8);
        logic.State.ShouldBeOfType<WeaponTraitState.Locked>();
        Tick(logic, 0.3);
        logic.State.ShouldBeOfType<WeaponTraitState.Ready>();
        logic.Dispose();
    }

    [Test]
    public void NoneIsDisabledAndIgnoresShots()
    {
        var logic = Create(WeaponTraitSettings.None);

        logic.State.ShouldBeOfType<WeaponTraitState.Disabled>();
        Fire(logic, 100);
        logic.State.ShouldBeOfType<WeaponTraitState.Disabled>();

        // 有効な特性から None に切り替えても Disabled に戻る
        logic.Input(new WeaponTraitLogic.Input.Configure(new WeaponTraitSettings(WeaponTraitType.Reload, Capacity: 1)));
        Fire(logic, 1);
        logic.State.ShouldBeOfType<WeaponTraitState.Locked>();
        logic.Input(new WeaponTraitLogic.Input.Configure(WeaponTraitSettings.None));
        logic.State.ShouldBeOfType<WeaponTraitState.Disabled>();
        logic.Dispose();
    }

    [Test]
    public void CapacityScaleSkipsCharge()
    {
        new WeaponTraitSettings(WeaponTraitType.Reload, Capacity: 24).WithCapacityScale(0.5f).Capacity.ShouldBe(12f);
        new WeaponTraitSettings(WeaponTraitType.Battery, Capacity: 200).WithCapacityScale(0.5f).Capacity.ShouldBe(100f);
        new WeaponTraitSettings(WeaponTraitType.Charge, Capacity: 1).WithCapacityScale(0.5f).Capacity.ShouldBe(1f);
    }
}
