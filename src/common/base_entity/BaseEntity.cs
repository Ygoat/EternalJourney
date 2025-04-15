namespace EternalJourney.Common.BaseEntity;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Common.DurabilityModule;
using EternalJourney.Common.Traits;
using Godot;

/// <summary>
/// ベースエンティティインターフェース
/// </summary>
public interface IBaseEntity : INode2D
{
}

/// <summary>
/// ベースエンティティクラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class BaseEntity : Node2D, IBaseEntity
{
    public override void _Notification(int what) => this.Notify(what);

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [Export]
    protected Status Status { get; set; } = new Status();

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [Node]
    protected IDurabilityModule DurabilityModule { get; set; } = default!;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="status"></param>
    protected void SetStatus(Status status)
    {
        Status = status;
        DurabilityModule.SetDurability(Status.MaxDur);
    }
}
