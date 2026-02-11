namespace EternalJourney.Bullet.Abstract;

using Chickensoft.AutoInject;
using Chickensoft.Collections;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Battle.Domain;
using EternalJourney.Bullet.Abstract.Base;
using EternalJourney.Bullet.Abstract.State;
using EternalJourney.Bullet.Strategies.Collision;
using EternalJourney.Cores.Consts;
using EternalJourney.Enemy.Base;
using Godot;


/// <summary>
/// スタンダード弾丸インターフェース
/// </summary>
public interface IStandardBullet : IBaseBullet
{
}

/// <summary>
/// スタンダード弾丸クラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class StandardBullet : BaseBullet, IStandardBullet
{
    public override void _Notification(int what) => this.Notify(what);

    #region State
    /// <summary>
    /// 弾丸ロジック
    /// </summary>
    public BulletLogic BulletLogic { get; set; } = default!;

    /// <summary>
    /// 弾丸ロジックバインド
    /// </summary>
    public BulletLogic.IBinding BulletBinding { get; set; } = default!;

    [Dependency] public EntityTable<int> EntityTable => this.DependOn<EntityTable<int>>();
    [Dependency] public IBattleRepo BattleRepo => this.DependOn<IBattleRepo>();

    #endregion State

    #region Nodes
    /// <summary>
    /// 画面外検知用通知ノード
    /// </summary>
    [Node]
    public IVisibleOnScreenNotifier2D VisibleOnScreenNotifier2D { get; set; } = default!;
    #endregion Nodes

    public override void Setup()
    {
        base.Setup();

        BulletLogic = new BulletLogic();
        BulletBinding = BulletLogic.Bind();
        BulletLogic.Set(this as IBaseBullet);
        BulletLogic.Set(BattleRepo);
        BulletLogic.Set<IBulletCollisionStrategy>(CollisionStrategy);

        // コリジョンレイヤーを弾丸
        CollisionLayer = CollisionEntity.Bullet;
        // コリジョンマスクをエネミー
        CollisionMask = CollisionEntity.Enemy;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override void OnResolved()
    {
        base.OnResolved();

        BulletBinding
            .When<BulletLogic.State.InFlight>(state =>
            {
                // 射出時の位置を設定(武器の発射口の位置)
                GlobalPosition = state.ShotGlobalPosition;
                // 射出時の方向を設定(武器の向いている方向)
                Direction = new Vector2(1, 0).Rotated(state.ShotGlobalAngle);
                // 弾丸の向きを設定（武器の向いている方向）
                Rotation = state.ShotGlobalAngle;
                SetPhysicsProcess(true);
            })
            .Handle((in BulletLogic.Output.Move output) =>
            {
                GlobalPosition += output.NextPositionDelta;
            })
            .Handle((in BulletLogic.Output.CurrentDurChange output) =>
            {
                Status.CurrentDur = output.CurrentDur;
            })
            .Handle((in BulletLogic.Output.Collapse _) =>
            {
                // フレーム終わりにRemoveSelf()呼び出し
                CallDeferred(nameof(RemoveSelf));
            });
        // コリジョンイベント設定
        AreaEntered += OnAreaEntered;
        // 画面外イベント
        VisibleOnScreenNotifier2D.ScreenExited += OnScreenExited;
        // ロジック初期化
        BulletLogic.Start();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="delta"></param>
    public void OnPhysicsProcess(double delta)
    {
        // 経過時間を更新
        ElapsedTime += (float)delta;
        // PhysicsProcess入力（移動ストラテジーを渡す）
        BulletLogic.Input(new BulletLogic.Input.PhysicsProcess(
            Direction, Status.Spd, ElapsedTime, MovementStrategy, GlobalPosition
        ));
    }

    /// <summary>
    /// 衝突時の処理
    /// </summary>
    /// <param name="area"></param>
    public void OnAreaEntered(Area2D area)
    {
        if (area is IBaseEnemy baseEnemy)
        {
            // ヒットを入力
            BulletLogic.Input(new BulletLogic.Input.EnemyHit(baseEnemy));
        }
    }

    /// <summary>
    /// 画面外に出た時の処理
    /// </summary>
    public void OnScreenExited()
    {
        // ミスを入力
        BulletLogic.Input(new BulletLogic.Input.Miss());
    }

    /// <summary>
    /// 射出
    /// </summary>
    /// <param name="shotGlobalPosition"></param>
    /// <param name="shotGlobalAngle"></param>
    public override void Emit(Vector2 shotGlobalPosition, float shotGlobalAngle)
    {
        // Emitを入力
        BulletLogic.Input(new BulletLogic.Input.Emit(shotGlobalPosition, shotGlobalAngle));
    }

}
