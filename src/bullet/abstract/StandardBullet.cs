namespace EternalJourney.Bullet.Abstract;

using Chickensoft.AutoInject;
using Chickensoft.Collections;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Battle.Domain;
using EternalJourney.Bullet.Abstract.Base;
using EternalJourney.Bullet.Abstract.State;
using EternalJourney.Common.Traits;
using EternalJourney.Cores.Consts;
using EternalJourney.Enemy.Abstract.Base;
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
    /// スタンダード弾丸ロジック
    /// </summary>
    public StandardBulletLogic StandardBulletLogic { get; set; } = default!;

    /// <summary>
    /// スタンダード弾丸ロジックバインド
    /// </summary>
    public StandardBulletLogic.IBinding StandardBulletBinding { get; set; } = default!;

    [Dependency] public EntityTable<int> EntityTable => this.DependOn<EntityTable<int>>();
    [Dependency] public IBattleRepo BattleRepo => this.DependOn<IBattleRepo>();

    #endregion State

    #region Exports
    /// <summary>
    /// 移動方向
    /// </summary>
    public Vector2 Direction { get; set; } = new Vector2(1, 0);
    #endregion Exports

    #region Nodes
    /// <summary>
    /// 画面外検知用通知ノード
    /// </summary>
    [Node]
    public IVisibleOnScreenNotifier2D VisibleOnScreenNotifier2D { get; set; } = default!;
    #endregion Nodes

    public void Setup()
    {
        StandardBulletLogic = new StandardBulletLogic();
        StandardBulletBinding = StandardBulletLogic.Bind();
        StandardBulletLogic.Set(this as IBaseBullet);
        StandardBulletLogic.Set(BattleRepo);

        // コリジョンレイヤーを弾丸
        CollisionLayer = CollisionEntity.Bullet;
        // コリジョンマスクをエネミー
        CollisionMask = CollisionEntity.Enemy;
        // ステータスセット
        SetStatus(new Status { Spd = 5.0f, MaxDur = 0.1f, CurrentDur = 0.1f });
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void OnResolved()
    {
        StandardBulletBinding
            .When<StandardBulletLogic.State.InFlight>(state =>
            {
                // 射出時の位置を設定(武器の発射口の位置)
                GlobalPosition = state.ShotGlobalPosition;
                // 射出時の方向を設定(武器の向いている方向)
                Direction = new Vector2(1, 0).Rotated(state.ShotGlobalAngle);
                // 弾丸の向きを設定（武器の向いている方向）
                Rotation = state.ShotGlobalAngle;
                SetPhysicsProcess(true);
            })
            .Handle((in StandardBulletLogic.Output.Move output) =>
            {
                GlobalPosition += output.NextPositionDelta;
            })
            // Disappearが出力された場合
            .Handle((in StandardBulletLogic.Output.Collapse _) =>
            {
                // フレーム終わりにRemoveSelf()呼び出し
                CallDeferred(nameof(RemoveSelf));
            });
        // コリジョンイベント設定
        AreaEntered += OnAreaEntered;
        // 画面外イベント
        VisibleOnScreenNotifier2D.ScreenExited += OnScreenExited;
        // ロジック初期化
        StandardBulletLogic.Start();
        // トップレベルオブジェクトとして扱う（親ノードのRotationの影響を受けないようにするため）
        TopLevel = true;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="delta"></param>
    public void OnPhysicsProcess(double delta)
    {
        // PhysicsProcess入力
        StandardBulletLogic.Input(new StandardBulletLogic.Input.PhysicsProcess(Direction, Status.Spd));
    }

    /// <summary>
    /// 自インスタンスをツリーから一時的に取り除く
    /// ※インスタンスは完全には削除されない
    /// </summary>
    public override void RemoveSelf()
    {
        // 親ノードを取得してから、子である自ノードを削除する
        GetParent().RemoveChild(this);
        // 弾丸の初期化
        InitializeBullet();
        // 物理処理無効化
        SetPhysicsProcess(false);
        // OnCollapsedシグナル出力
        EmitSignal(BaseBullet.SignalName.Removed, this);
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
            StandardBulletLogic.Input(new StandardBulletLogic.Input.EnemyHit(baseEnemy));
        }
    }

    /// <summary>
    /// 画面外に出た時の処理
    /// </summary>
    public void OnScreenExited()
    {
        // ミスを入力
        StandardBulletLogic.Input(new StandardBulletLogic.Input.Miss());
    }

    /// <summary>
    /// 射出
    /// </summary>
    /// <param name="shotGlobalPosition"></param>
    /// <param name="shotGlobalAngle"></param>
    public override void Emit(Vector2 shotGlobalPosition, float shotGlobalAngle)
    {
        // Emitを入力
        StandardBulletLogic.Input(new StandardBulletLogic.Input.Emit(shotGlobalPosition, shotGlobalAngle));
    }

    /// <summary>
    /// 弾丸初期化
    /// </summary>
    public void InitializeBullet()
    {
        // グローバル座標の初期化
        GlobalPosition = new Vector2(0, 0);
        // 方向を初期化
        Direction = new Vector2(0, 0);
        // 耐久値を回復
        Status.CurrentDur = Status.MaxDur;
    }
}
