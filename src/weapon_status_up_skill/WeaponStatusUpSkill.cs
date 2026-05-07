namespace EternalJourney.WeaponStatusUpSkill;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Battle.Domain;
using EternalJourney.StatusUpSkill.State;
using Godot;

/// <summary>
/// 武器ステータスアップスキルインターフェース
/// </summary>
public interface IWeaponStatusUpSkill : INode
{
    /// <summary>スキルを発動する</summary>
    void Activate();
}

/// <summary>
/// Weapon ステータスアップスキル（ATK・SPDを一定時間バフする）
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class WeaponStatusUpSkill : Node, IWeaponStatusUpSkill
{
    public override void _Notification(int what) => this.Notify(what);

    public StatusUpSkillLogic Logic { get; set; } = default!;
    public StatusUpSkillLogic.IBinding Binding { get; set; } = default!;

    public Timer BuffTimer { get; set; } = default!;
    public float BuffDuration { get; set; }

    [Dependency] public IBattleRepo BattleRepo => this.DependOn<IBattleRepo>();

    public void Initialize()
    {
        BuffTimer = new Timer();
        BuffDuration = 10.0f;
    }

    public void OnReady()
    {
        AddChild(BuffTimer);
    }

    public void Setup()
    {
        Logic = new StatusUpSkillLogic();
        Binding = Logic.Bind();
    }

    public void OnResolved()
    {
        BuffTimer.WaitTime = BuffDuration;
        BuffTimer.OneShot = true;
        BuffTimer.Timeout += OnBuffTimerTimeout;

        Logic.Set(new StatusUpSkillLogic.StackState());

        Binding
            .Handle((in StatusUpSkillLogic.Output.Activated o) =>
            {
                BattleRepo.WeaponAtkMultiplier = 1.0f + o.StackCount * 0.5f;
                BattleRepo.WeaponSpdMultiplier = 1.0f + o.StackCount * 0.2f;
                BuffTimer.Stop();
                BuffTimer.Start();
            })
            .Handle((in StatusUpSkillLogic.Output.Deactivated _) =>
            {
                BattleRepo.WeaponAtkMultiplier = 1.0f;
                BattleRepo.WeaponSpdMultiplier = 1.0f;
            });
        Logic.Start();
    }

    public void Activate()
    {
        Logic?.Input(new StatusUpSkillLogic.Input.Apply());
    }

    private void OnBuffTimerTimeout()
    {
        Logic.Input(new StatusUpSkillLogic.Input.Remove());
    }

    public void OnTreeExiting()
    {
        Binding.Dispose();
        ((System.IDisposable)Logic).Dispose();
    }
}
