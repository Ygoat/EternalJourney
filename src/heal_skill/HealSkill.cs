namespace EternalJourney.HealSkill;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Battle.Domain;
using EternalJourney.HealSkill.State;
using Godot;

/// <summary>
/// 回復スキルインターフェース
/// </summary>
public interface IHealSkill : INode
{
    /// <summary>
    /// スキルを発動する
    /// </summary>
    void Activate();
}

/// <summary>
/// 回復スキルクラス（自機HPを即時回復する）
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class HealSkill : Node, IHealSkill
{
    public override void _Notification(int what) => this.Notify(what);

    /// <summary>
    /// スキルロジック
    /// </summary>
    public HealSkillLogic Logic { get; set; } = default!;

    /// <summary>
    /// スキルバインド
    /// </summary>
    public HealSkillLogic.IBinding Binding { get; set; } = default!;

    [Dependency] public IBattleRepo BattleRepo => this.DependOn<IBattleRepo>();

    public void Setup()
    {
        Logic = new HealSkillLogic();
        Binding = Logic.Bind();
    }

    public void OnResolved()
    {
        Binding
            .Handle((in HealSkillLogic.Output.Healed o) =>
            {
                BattleRepo.RequestShipHeal(o.Amount);
            });
        Logic.Start();
    }

    /// <summary>
    /// スキルを発動する
    /// </summary>
    public void Activate()
    {
        Logic?.Input(new HealSkillLogic.Input.Apply());
    }

    public void OnTreeExiting()
    {
        Binding.Dispose();
        ((System.IDisposable)Logic).Dispose();
    }
}
