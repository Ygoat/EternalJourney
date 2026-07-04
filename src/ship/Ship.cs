namespace EternalJourney.Ship;

using Chickensoft.AutoInject;
using Chickensoft.Collections;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Battle.Domain;
using EternalJourney.Common.BaseEntity;
using EternalJourney.Cores.Consts;
using EternalJourney.Ship.State;
using Godot;

/// <summary>
/// 宇宙船インターフェース
/// </summary>
public interface IShip : IBaseEntity
{
    /// <summary>
    /// 敵ターゲットマーカ―
    /// </summary>
    public IMarker2D EnemyTargetMarker { get; set; }

    /// <summary>
    /// 宇宙船の中心マーカー
    /// </summary>
    public IMarker2D CenterMarker { get; set; }
};

/// <summary>
/// 宇宙船クラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class Ship : BaseEntity, IShip
{
    public override void _Notification(int what) => this.Notify(what);

    #region State
    /// <summary>
    /// 船ロジック
    /// </summary>
    public IShipLogic ShipLogic { get; set; } = default!;

    /// <summary>
    /// 船ロジックバインド
    /// </summary>
    public ShipLogic.IBinding ShipBinding { get; set; } = default!;
    #endregion State

    #region Nodes
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [Node]
    public IMarker2D EnemyTargetMarker { get; set; } = default!;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [Node]
    public IMarker2D CenterMarker { get; set; } = default!;

    /// <summary>
    /// 舟のコリジョンシェイプ
    /// </summary>
    [Node]
    public ICollisionShape2D CollisionShape2D { get; set; } = default!;

    #endregion Nodes

    #region Dependencies
    /// <summary>
    /// エンティティテーブル
    /// </summary>
    [Dependency]
    public EntityTable<int> EntityTable => this.DependOn<EntityTable<int>>();

    /// <summary>
    /// バトルリポジトリ
    /// </summary>
    [Dependency]
    public IBattleRepo BattleRepo => this.DependOn<IBattleRepo>();
    #endregion Dependencies

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override void Setup()
    {
        base.Setup();

        CollisionLayer = CollisionEntity.Ship;
        // 敵弾検知のためBulletを追加
        CollisionMask = CollisionEntity.Enemy | CollisionEntity.Bullet;

        Status.MaxDur = 500f;
        Status.CurrentDur = 500f;

        ShipLogic = new ShipLogic();
        ShipLogic.Set(this as IShip);
        ShipBinding = ShipLogic.Bind();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override void OnResolved()
    {
        base.OnResolved();

        EntityTable.Set(0, this);

        ShipLogic.Set(BattleRepo);
        ShipBinding
            .Handle((in ShipLogic.Output.HpChanged o) =>
                BattleRepo.NotifyShipHpChanged(o.CurrentHp, o.MaxHp))
            .Handle((in ShipLogic.Output.Dead _) =>
            {
                BattleRepo.NotifyGameOver();
            });

        ShipLogic.Start();
    }

    public void OnTreeExiting()
    {
        ShipBinding.Dispose();
        ((System.IDisposable)ShipLogic).Dispose();
    }
}
