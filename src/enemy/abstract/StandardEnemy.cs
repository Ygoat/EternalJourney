namespace EternalJourney.Enemy.Abstract;

using Chickensoft.AutoInject;
using Chickensoft.Collections;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Battle.Domain;
using EternalJourney.Bullet.Abstract.Base;
using EternalJourney.Common.Traits;
using EternalJourney.Cores.Consts;
using EternalJourney.Enemy.Abstract.Base;
using EternalJourney.Enemy.Abstract.State;
using EternalJourney.Ship;
using Godot;

/// <summary>
/// スタンダードエネミーインタフェース
/// </summary>
public interface IStandardEnemy : IBaseEnemy
{
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
    public StandardEnemyLogic StandardEnemyLogic { get; set; } = default!;

    /// <summary>
    /// エネミーロジックバインド
    /// </summary>
    public StandardEnemyLogic.IBinding StandardEnemyBinding { get; set; } = default!;

    [Dependency] public EntityTable<int> EntityTable => this.DependOn<EntityTable<int>>();
    [Dependency] public IBattleRepo BattleRepo => this.DependOn<IBattleRepo>();
    #endregion State

    #region Exports
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
    #endregion Nodes

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void OnReady()
    {
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override void Setup()
    {
        base.Setup();

        // エネミーロジック
        StandardEnemyLogic = new StandardEnemyLogic();
        StandardEnemyLogic.Set(this as IBaseEnemy);
        StandardEnemyLogic.Set(BattleRepo);
        // エネミーロジックバインド
        StandardEnemyBinding = StandardEnemyLogic.Bind();
        // コリジョンレイヤをエネミーに設定
        CollisionLayer = CollisionEntity.Enemy;
        // コリジョンマスクを船と弾丸に設定
        CollisionMask = CollisionEntity.Ship | CollisionEntity.Bullet;
        // ステータスセット
        SetStatus(new Status { Spd = 1.0f, MaxDur = 0.1f });

        // ターゲット位置
        TargetPosition = EntityTable.Get<IShip>(0)!.EnemyTargetMarker.GlobalPosition;
        TopLevel = true;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override void OnResolved()
    {
        base.OnResolved();

        StandardEnemyBinding
            .When<StandardEnemyLogic.State.Invading>(state =>
            {
                // スポーン時の位置を設定
                GlobalPosition = state.SpawnGlobalPosition;
                // スポーン時の方向を設定
                Direction = TargetPosition - GlobalPosition;
                // エネミーの向きを設定
                Rotation = state.SpawnGlobalAngle;
                SetPhysicsProcess(true);
            })
            .Handle((in StandardEnemyLogic.Output.Move output) =>
            {
                GlobalPosition += output.NextPositionDelta;
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
    /// <inheritdoc/>
    /// </summary>
    /// <param name="delta"></param>
    public void OnPhysicsProcess(double delta)
    {
        // PhysicsProcess入力
        StandardEnemyLogic.Input(new StandardEnemyLogic.Input.PhysicsProcess(Direction, Status.Spd));
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
        StatusEffectManager.ApplyEffect(StatusEffectManager.PoisonEffect);
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
    }
}
