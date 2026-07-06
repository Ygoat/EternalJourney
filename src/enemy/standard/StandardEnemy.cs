namespace EternalJourney.Enemy.Standard;

using Chickensoft.AutoInject;
using Chickensoft.Collections;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Battle.Domain;
using EternalJourney.Bullet.Abstract.Base;
using EternalJourney.Cores.Consts;
using EternalJourney.Enemy.Base;
using EternalJourney.Enemy.Standard.State;
using EternalJourney.Enemy.Strategies.Movement;
using EternalJourney.Ship;
using EternalJourney.Weapon.Abstract;
using Godot;

/// <summary>
/// スタンダードエネミーインタフェース
/// </summary>
public interface IStandardEnemy : IBaseEnemy
{
    /// <summary>
    /// スコア値
    /// </summary>
    int ScoreValue { get; }
}

/// <summary>
/// スタンダードエネミークラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class StandardEnemy : BaseEnemy, IStandardEnemy
{
    public override void _Notification(int what) => this.Notify(what);

    #region State
    /// <summary>
    /// エネミーロジック
    /// </summary>
    public IStandardEnemyLogic StandardEnemyLogic { get; set; } = default!;

    /// <summary>
    /// エネミーロジックバインド
    /// </summary>
    public StandardEnemyLogic.IBinding StandardEnemyBinding { get; set; } = default!;

    [Dependency] public EntityTable<int> EntityTable => this.DependOn<EntityTable<int>>();

    /// <summary>
    /// 移動戦略
    /// </summary>
    private IMovementStrategy _movementStrategy = new LinearMovementStrategy();

    /// <summary>
    /// 経過時間
    /// </summary>
    private float _elapsedTime;

    private const float BodyRotationSpeed = 0.03f;

    /// <summary>
    /// スコア値
    /// </summary>
    [Export]
    public int ScoreValue { get; set; } = 10;
    #endregion State

    #region Exports
    /// <summary>
    /// 移動戦略リソース
    /// </summary>
    [Export]
    public EnemyMovementStrategyResource? MovementStrategyResource { get; set; }

    /// <summary>
    /// 標的対象位置
    /// </summary>
    public Vector2 TargetPosition { get; set; } = default!;

    /// <summary>
    /// 進行方向
    /// </summary>
    public Vector2 Direction { get; set; } = default!;
    #endregion Exports

    #region Nodes
    /// <summary>
    /// 画面外検知用
    /// </summary>
    [Node]
    public IVisibleOnScreenNotifier2D VisibleOnScreenNotifier2D { get; set; } = default!;

    [Node]
    public IColorRect ColorRect { get; set; } = default!;

    [Node]
    public IStandardWeapon StandardWeapon { get; set; } = default!;
    #endregion Nodes

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override void OnReady()
    {
        base.OnReady();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override void Setup()
    {
        base.Setup();

        // エネミーロジック
        StandardEnemyLogic = new StandardEnemyLogic();
        StandardEnemyLogic.Set(this as IStandardEnemy);
        // エネミーロジックバインド
        StandardEnemyBinding = StandardEnemyLogic.Bind();
        // コリジョンレイヤをエネミーに設定
        CollisionLayer = CollisionEntity.Enemy;
        // コリジョンマスクを船と弾丸に設定
        CollisionMask = CollisionEntity.Ship | CollisionEntity.Bullet;

        TopLevel = true;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override void OnResolved()
    {
        base.OnResolved();

        // DI解決後に依存を設定
        StandardEnemyLogic.Set(BattleRepo);
        StandardEnemyLogic.Set(EntityTable.Get<IShip>(0)!);
        // ターゲット位置を設定（Configure呼び出し時点でShipは登録済み）
        TargetPosition = EntityTable.Get<IShip>(0)!.EnemyTargetMarker.GlobalPosition;
        // WeaponのターゲットをShipに設定
        StandardWeapon.SetTargetMask(CollisionEntity.Ship);
        // 敵所有フラグを設定
        StandardWeapon.SetPlayerOwned(false);

        StandardEnemyBinding
            .Handle((in StandardEnemyLogic.Output.StartInvade output) =>
            {
                // スポーン時の位置を設定
                GlobalPosition = output.SpawnGlobalPosition;
                // スポーン時の方向を設定
                Direction = TargetPosition - GlobalPosition;
                // エネミーの向きを設定
                Rotation = output.SpawnGlobalAngle;
                SetPhysicsProcess(true);
            })
            .Handle((in StandardEnemyLogic.Output.CurrentDurChange output) =>
            {
                Status.CurrentDur = output.CurrentDur;
            })
            .Handle((in StandardEnemyLogic.Output.Move output) =>
            {
                GlobalPosition += output.NextPositionDelta;
            })
            .Handle((in StandardEnemyLogic.Output.UpdateColor output) =>
            {
                ColorRect.Color = output.Color;
            })
            .Handle((in StandardEnemyLogic.Output.Destroyed _) =>
            {
                CallDeferred(nameof(RemoveSelf));
            });
        // エリアエンターイベント設定
        AreaEntered += OnAreaEntered;
        // スクリーン外イベント設定
        VisibleOnScreenNotifier2D.ScreenExited += OnScreenExited;
        // エネミーロジック初期状態開始
        StandardEnemyLogic.Start();
    }

    /// <summary>
    /// プールから取得された時のコールバック（IPoolable実装）
    /// </summary>
    public override void OnAcquired()
    {
        base.OnAcquired();

        // 経過時間をリセット
        _elapsedTime = 0f;

        // 移動戦略を毎回新規生成（プール内での状態共有を避けるため）
        _movementStrategy = MovementStrategyResource?.CreateStrategy() ?? new LinearMovementStrategy();
        _movementStrategy.Initialize(TargetPosition);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="delta"></param>
    public override void OnPhysicsProcess(double delta)
    {
        base.OnPhysicsProcess(delta);

        // 経過時間を更新
        _elapsedTime += (float)delta;

        // 船体をShipのEnemyTargetMarkerに向けて回転
        IShip? ship = EntityTable.Get<IShip>(0);
        if (ship != null)
        {
            Vector2 dir = GlobalPosition.DirectionTo(ship.EnemyTargetMarker.GlobalPosition);
            GlobalRotation = Mathf.LerpAngle(GlobalRotation, dir.Angle(), BodyRotationSpeed);
        }

        // PhysicsProcess入力（移動戦略を使用）
        StandardEnemyLogic.Input(new StandardEnemyLogic.Input.PhysicsProcess(
            Direction,
            Status.Spd,
            _elapsedTime,
            _movementStrategy,
            GlobalPosition));
    }

    /// <summary>
    /// エリアエンターイベントファンクション
    /// </summary>
    /// <param name="area"></param>
    public void OnAreaEntered(Area2D area)
    {
        if (area is IBaseBullet baseBullet)
        {
            StandardEnemyLogic.Input(new StandardEnemyLogic.Input.BulletHit(baseBullet));
        }
        // TakeDamage入力
    }

    /// <summary>
    /// 敵スポーン
    /// </summary>
    public override void Spawn(Vector2 spawnGlobalPosition, float spawnGlobalAngle)
    {
        // TargetDiscover入力
        StandardEnemyLogic.Input(new StandardEnemyLogic.Input.Spawn(spawnGlobalPosition, spawnGlobalAngle));
    }

    /// <summary>
    /// スタン開始：移動を停止する
    /// </summary>
    protected override void OnStunStart() => SetPhysicsProcess(false);

    /// <summary>
    /// スタン終了：移動を再開する
    /// </summary>
    protected override void OnStunEnd() => SetPhysicsProcess(true);

    /// <summary>
    /// エリア外に出た時消す
    /// TODO:それ用のstateを作成したい
    /// </summary>
    public void OnScreenExited()
    {
        // OutOfArea入力
        StandardEnemyLogic.Input(new StandardEnemyLogic.Input.OutOfArea());
    }

    /// <summary>
    /// 自インスタンスをツリーから一時的に取り除く
    /// ※インスタンスは完全には削除されない
    /// </summary>
    public override void RemoveSelf()
    {
        // 親ノードを取得してから、子である自ノードを削除する
        GetParent().RemoveChild(this);
        // エネミーの初期化
        InitializeEnemy();
        // 物理処理無効化
        SetPhysicsProcess(false);
        // シグナル出力
        EmitSignal(BaseEnemy.SignalName.Removed, this);
    }

    public void OnTreeExiting()
    {
        StandardEnemyBinding.Dispose();
        ((System.IDisposable)StandardEnemyLogic).Dispose();
    }

    /// <summary>
    /// エネミー初期化
    /// </summary>
    public void InitializeEnemy()
    {
        // グローバル座標の初期化
        GlobalPosition = new Vector2(0, 0);
        // 方向を初期化
        Direction = new Vector2(0, 0);
        // 耐久値を回復
        Status.CurrentDur = Status.MaxDur;
        // 状態異常解除
        StatusEffectReceiverManager.RemoveAll();
        // 経過時間リセット
        _elapsedTime = 0f;
        // 移動戦略リセット
        _movementStrategy.Reset();
    }
}
