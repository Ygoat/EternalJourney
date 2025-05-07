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
public interface IBaseEntity : IArea2D
{
    public Status Status { get; set; }
}

/// <summary>
/// ベースエンティティクラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class BaseEntity : Area2D, IBaseEntity
{
    public override void _Notification(int what) => this.Notify(what);

    /// <summary>
    /// ステータス
    /// </summary>
    [Export]
    public Status Status { get; set; } = new Status();

    /// <summary>
    /// 耐久値モジュール
    /// </summary>
    [Node]
    protected IDurabilityModule DurabilityModule { get; set; } = default!;
}
