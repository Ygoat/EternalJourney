namespace EternalJourney.RegenSkill;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Battle.Domain;
using EternalJourney.RegenSkill.State;
using Godot;

/// <summary>
/// リジェネスキルインターフェース
/// </summary>
public interface IRegenSkill : INode
{
    /// <summary>
    /// スキルを発動する
    /// </summary>
    public void Activate();
}

/// <summary>
/// リジェネスキルクラス（毎秒固定量HP回復）
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class RegenSkill : Node, IRegenSkill
{
    public override void _Notification(int what) => this.Notify(what);

    /// <summary>スキルロジック</summary>
    public RegenSkillLogic Logic { get; set; } = default!;

    /// <summary>スキルバインド</summary>
    public RegenSkillLogic.IBinding Binding { get; set; } = default!;

    /// <summary>1秒ごとに回復を発行するタイマー</summary>
    public Timer HealTimer { get; set; } = default!;

    /// <summary>バフ持続タイマー</summary>
    public Timer DurationTimer { get; set; } = default!;

    /// <summary>バフ持続時間（秒）</summary>
    public float DurationSec { get; set; } = 10.0f;

    [Dependency] public IBattleRepo BattleRepo => this.DependOn<IBattleRepo>();

    public void Initialize()
    {
        HealTimer = new Timer();
        DurationTimer = new Timer();
    }

    public void OnReady()
    {
        AddChild(HealTimer);
        AddChild(DurationTimer);
    }

    public void Setup()
    {
        Logic = new RegenSkillLogic();
        Binding = Logic.Bind();
    }

    public void OnResolved()
    {
        HealTimer.WaitTime = 1.0;
        DurationTimer.WaitTime = DurationSec;
        HealTimer.Timeout += () => Logic.Input(new RegenSkillLogic.Input.Tick());
        DurationTimer.Timeout += () => Logic.Input(new RegenSkillLogic.Input.Remove());

        Binding
            .Handle((in RegenSkillLogic.Output.Activated _) =>
            {
                HealTimer.Stop();
                HealTimer.Start();
                DurationTimer.Stop();
                DurationTimer.Start();
            })
            .Handle((in RegenSkillLogic.Output.TikHeal o) =>
            {
                BattleRepo.RequestShipHeal(o.Amount);
            })
            .Handle((in RegenSkillLogic.Output.Deactivated _) =>
            {
                HealTimer.Stop();
                DurationTimer.Stop();
            });
        Logic.Start();
    }

    /// <summary>
    /// スキルを発動する
    /// </summary>
    public void Activate()
    {
        Logic?.Input(new RegenSkillLogic.Input.Apply());
    }

    public void OnTreeExiting()
    {
        Binding.Dispose();
        ((System.IDisposable)Logic).Dispose();
    }
}
