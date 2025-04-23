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
    protected Status Status { get; set; } = new Status();

    /// <summary>
    /// 耐久値モジュール
    /// </summary>
    [Node]
    protected IDurabilityModule DurabilityModule { get; set; } = default!;

    /// <summary>
    /// ステータス設定
    /// </summary>
    /// <param name="status"></param>
    protected void SetStatus(Status status)
    {
        Status = status;
        DurabilityModule.SetDurability(Status.MaxDur);
    }
}
