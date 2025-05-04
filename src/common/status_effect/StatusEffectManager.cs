namespace EternalJourney.Common.StatusEffect;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;

/// <summary>
/// 状態異常マネージャーインターフェース
/// </summary>
public interface IStatusEffectManager : INode
{
    public PoisonEffect PoisonEffect { get; }
    public abstract void ApplyEffect(IStatusEffect statusEffect);
    public abstract void RemoveEffect(IStatusEffect statusEffect);
}

/// <summary>
/// 状態異常マネージャークラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class StatusEffectManager : Node, IStatusEffectManager
{
    public override void _Notification(int what) => this.Notify(what);

    public PoisonEffect PoisonEffect { get; private set; } = default!;

    public virtual void Setup()
    {
        PoisonEffect = new PoisonEffect();
    }

    public virtual void OnResolved()
    {
        AddChild(PoisonEffect);
    }

    /// <summary>
    /// 状態異常適用
    /// </summary>
    /// <param name="statusEffect"></param>
    public void ApplyEffect(IStatusEffect statusEffect)
    {
        statusEffect.Apply();
        GD.Print("apply");
    }

    /// <summary>
    /// 状態異常適用
    /// </summary>
    /// <param name="statusEffect"></param>
    public void RemoveEffect(IStatusEffect statusEffect)
    {
        statusEffect.Remove();
    }
}
