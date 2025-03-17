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
public interface IShip : INode2D
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
public partial class Ship : Node2D, IShip
{
    public override void _Notification(int what) => this.Notify(what);

    #region Nodes
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [Node]
    public IMarker2D EnemyTargetMarker { get; set; } = default!;

    /// <summary>
    /// 衝突判定用エリア
    /// </summary>
    [Node]
    public IArea2D Area2D { get; set; } = default!;
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
        Area2D.CollisionLayer = CollisionEntity.Ship;
        Area2D.CollisionMask = CollisionEntity.Enemy;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void OnResolved()
    {
        EntityTable.Set(0, this);
    }

}
