namespace EternalJourney.Common.StatusEffect;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;

/// <summary>
/// 状態異常マネージャーインターフェース
/// </summary>
public interface IStatusEffectReceiverManager : INode
{
    public PoisonEffect PoisonEffect { get; }
}

/// <summary>
/// 状態異常マネージャークラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class StatusEffectReceiverManager : Node, IStatusEffectReceiverManager
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

    public virtual void RemoveAll()
    {
        PoisonEffect.Remove();
    }
}
