namespace EternalJourney.SpSkillEffectArea.State;

using Chickensoft.GoDotTest;
using EternalJourney.Battle.Domain;
using EternalJourney.Cores.Models.Skill;
using EternalJourney.Game.Domain;
using Godot;
using Shouldly;

public class SpSkillLogicTest : TestClass
{
    public SpSkillLogicTest(Node testScene) : base(testScene) { }

    private static int _bursts;
    private static SpSkillLogic.Output.Burst _lastBurst;

    private static SpSkillLogic Create(BattleRepo battleRepo, SpSkillType type)
    {
        var gameRepo = new GameRepo();
        gameRepo.SetSelectedSpSkill(type);
        var logic = new SpSkillLogic();
        logic.Set<IBattleRepo>(battleRepo);
        logic.Set<IGameRepo>(gameRepo);
        _bursts = 0;
        var binding = logic.Bind();
        binding.OnOutput((in SpSkillLogic.Output.Burst o) => { _bursts++; _lastBurst = o; });
        logic.Start<SpSkillState.Ready>();
        return logic;
    }

    private static void FillAndActivate(BattleRepo repo)
    {
        for (int i = 0; i < 20; i++)
        {
            repo.OnEnemyDestroyed(null!);
        }
        repo.ActivateSP();
    }

    [Test]
    public void BigDamageBurstsWithoutDebuff()
    {
        using var repo = new BattleRepo();
        var logic = Create(repo, SpSkillType.BigDamage);

        FillAndActivate(repo);

        _bursts.ShouldBe(1);
        _lastBurst.Damage.ShouldBe(300f, 0.001f);
        _lastBurst.Stun.ShouldBeFalse();
        repo.EnemyAtkMultiplier.ShouldBe(1f);
        logic.State.ShouldBeOfType<SpSkillState.Ready>();
        logic.Dispose();
    }

    [Test]
    public void AttackDownDebuffExpiresAfterDuration()
    {
        using var repo = new BattleRepo();
        var logic = Create(repo, SpSkillType.AttackDown);

        FillAndActivate(repo);

        repo.EnemyAtkMultiplier.ShouldBe(0.5f);
        logic.Input(new SpSkillLogic.Input.Process(9.5));
        repo.EnemyAtkMultiplier.ShouldBe(0.5f);
        logic.Input(new SpSkillLogic.Input.Process(0.6));
        repo.EnemyAtkMultiplier.ShouldBe(1f);
        logic.State.ShouldBeOfType<SpSkillState.Ready>();
        logic.Dispose();
    }

    [Test]
    public void ReactivationDuringDebuffRestartsDuration()
    {
        using var repo = new BattleRepo();
        var logic = Create(repo, SpSkillType.FireRateDown);

        FillAndActivate(repo);
        logic.Input(new SpSkillLogic.Input.Process(8.0));
        FillAndActivate(repo);
        logic.Input(new SpSkillLogic.Input.Process(8.0));

        _bursts.ShouldBe(2);
        repo.EnemyFireIntervalMultiplier.ShouldBe(2f);
        logic.Dispose();
    }
}
