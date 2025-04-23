namespace EternalJourney.Ship;

using Chickensoft.AutoInject;
using Chickensoft.Collections;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Cores.Consts;
using Godot;

/// <summary>
/// 宇宙船インターフェース
/// </summary>
public interface IShip : IArea2D
{
    /// <summary>
    /// 敵ターゲットマーカ―
    /// </summary>
    public IMarker2D EnemyTargetMarker { get; set; }
};

/// <summary>
/// 宇宙船クラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class Ship : Area2D, IShip
{
    public override void _Notification(int what) => this.Notify(what);

    #region Nodes
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [Node]
    public IMarker2D EnemyTargetMarker { get; set; } = default!;

    #endregion Nodes

    #region Dependencies
    /// <summary>
    /// エンティティテーブル
    /// </summary>
    [Dependency]
    public EntityTable<int> EntityTable => this.DependOn<EntityTable<int>>();
    #endregion Dependencies

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Setup()
    {
        CollisionLayer = CollisionEntity.Ship;
        CollisionMask = CollisionEntity.Enemy;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void OnResolved()
    {
        EntityTable.Set(0, this);
    }

}
