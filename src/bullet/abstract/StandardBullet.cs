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
using EternalJourney.Cores.Models.Bullet;
using EternalJourney.Enemy.Base;
using Godot;

// TODO: TopLevelの設定をエディター画面上で設定しているため、コード上で設定するようにする。
// 現状コード側で設定するとエラーとなる

/// <summary>
/// スタンダード弾丸インターフェース
/// </summary>
public interface IStandardBullet : IBaseBullet
{
}

/// <summary>
/// 統一弾丸クラス（通常弾・貫通弾・爆発弾すべてに対応）
/// 爆風機能はシーン内のノード有無で自動判定
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

    #region OptionalBlastNodes
    /// <summary>
    /// 爆風タイマー（爆風弾シーンのみ存在）
    /// </summary>
    [Node]
    public ITimer BlastTimer { get; set; } = default!;

    /// <summary>
    /// 弾丸コリジョンシェイプ（爆風弾シーンのみ存在）
    /// </summary>
    [Node]
    public ICollisionShape2D BulletCollisionShape2D { get; set; } = default!;

    /// <summary>
    /// 弾丸カラーレクト（爆風弾シーンのみ存在）
    /// </summary>
    [Node]
    public IColorRect BulletColorRect { get; set; } = default!;

    /// <summary>
    /// 爆風コリジョンシェイプ（爆風弾シーンのみ存在）
    /// </summary>
    [Node]
    public ICollisionShape2D BlastCollisionShape2D { get; set; } = default!;

    /// <summary>
    /// 爆風カラーレクト（爆風弾シーンのみ存在）
    /// </summary>
    [Node]
    public IColorRect BlastColorRect { get; set; } = default!;

    #endregion OptionalBlastNodes

    public override void Setup()
    {
        base.Setup();

        // オプショナルノード取得（爆風弾シーンの場合のみ存在）
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
            .When<BulletLogic.State.EmitWait>(state =>
            {
                // 爆風テクスチャ非表示と爆風当たり判定無効化
                CallDeferred(nameof(SetBlastBodyEnabled), false);
                // }
            })
            .When<BulletLogic.State.InFlight>(state =>
            {
                // 射出時の位置を設定(武器の発射口の位置)
                GlobalPosition = state.ShotGlobalPosition;
                // 射出時の方向を設定(武器の向いている方向)
                Direction = new Vector2(1, 0).Rotated(state.ShotGlobalAngle);
                // 弾丸の向きを設定（武器の向いている方向）
                Rotation = state.ShotGlobalAngle;

                // 弾丸テクスチャ非表示と弾丸当たり判定有効化
                CallDeferred(nameof(SetBulletBodyEnabled), true);

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
                SetPhysicsProcess(false);

            })
            .When<BulletLogic.State.Blast>(state =>
            {
                if (CollisionStrategy is ExplosionCollisionStrategy)
                {
                    // 弾丸テクスチャ非表示と弾丸当たり判定無効化
                    CallDeferred(nameof(SetBulletBodyEnabled), false);
                    // 爆風テクスチャ表示と爆風当たり判定有効化
                    CallDeferred(nameof(SetBlastBodyEnabled), true);
                    // 爆風タイマースタート
                    BlastTimer!.Start();
                }
            })
            .Handle((in BulletLogic.Output.RemoveSelf _) =>
            {
                if (CollisionStrategy is ExplosionCollisionStrategy)
                {
                    // 爆風テクスチャ非表示と爆風当たり判定無効化
                    CallDeferred(nameof(SetBlastBodyEnabled), false);
                }
                // フレーム終わりにRemoveSelf()呼び出し
                CallDeferred(nameof(RemoveSelf));
            });

        // コリジョンイベント設定
        AreaEntered += OnAreaEntered;
        // 画面外イベント
        VisibleOnScreenNotifier2D.ScreenExited += OnScreenExited;
        // ロジック初期化
        BulletLogic.Start();

        // 爆風タイマー設定（爆風弾シーンの場合のみ）
        if (BlastTimer != null)
        {
            if (CollisionStrategy is ExplosionCollisionStrategy explosionStrategy)
            {
                BlastTimer.WaitTime = explosionStrategy.BlastDuration;
            }
            else
            {
                BlastTimer.WaitTime = 0.5;
            }
            BlastTimer.OneShot = true;
            BlastTimer.Timeout += OnBlastTimerTimeout;
        }
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

    public override void Configure(BulletConfig config)
    {
        base.Configure(config);
    }

    /// <summary>
    /// 射出
    /// </summary>
    /// <param name="shotGlobalPosition"></param>
    /// <param name="shotGlobalAngle"></param>
    public override void Emit(Vector2 shotGlobalPosition, float shotGlobalAngle)
    {
        BulletLogic.Input(new BulletLogic.Input.Emit(shotGlobalPosition, shotGlobalAngle));
    }

    /// <summary>
    /// コリジョンエリア進入イベント
    /// </summary>
    /// <param name="area"></param>
    private void OnAreaEntered(Area2D area)
    {
        if (area is IBaseEnemy baseEnemy)
        {
            BulletLogic.Input(new BulletLogic.Input.EnemyHit(baseEnemy));
        }
    }

    /// <summary>
    /// 画面外イベント
    /// </summary>
    private void OnScreenExited()
    {
        BulletLogic.Input(new BulletLogic.Input.Miss());
    }

    /// <summary>
    /// 爆風タイマータイムアウトイベント
    /// </summary>
    private void OnBlastTimerTimeout()
    {
        BulletLogic.Input(new BulletLogic.Input.BlastTimerTimeout());
    }

    /// <summary>
    /// 弾丸ボディの表示/当たり判定切替
    /// </summary>
    /// <param name="flag">有効化フラグ</param>
    private void SetBulletBodyEnabled(bool flag)
    {
        if (BulletColorRect == null || BulletCollisionShape2D == null)
        {
            return;
        }

        if (flag)
        {
            BulletColorRect.Show();
            BulletCollisionShape2D.Disabled = false;
        }
        else
        {
            BulletColorRect.Hide();
            BulletCollisionShape2D.Disabled = true;
        }
    }

    /// <summary>
    /// 爆風ボディの表示/当たり判定切替
    /// </summary>
    /// <param name="flag">有効化フラグ</param>
    private void SetBlastBodyEnabled(bool flag)
    {
        if (flag)
        {
            BlastColorRect.Show();
            BlastCollisionShape2D.Disabled = false;
        }
        else
        {
            BlastColorRect.Hide();
            BlastCollisionShape2D.Disabled = true;
        }
    }
}
