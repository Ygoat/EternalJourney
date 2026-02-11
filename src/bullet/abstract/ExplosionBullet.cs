namespace EternalJourney.Bullet.Abstract;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Battle.Domain;
using EternalJourney.Bullet.Abstract.Base;
using EternalJourney.Bullet.Abstract.State;
using EternalJourney.Bullet.Strategies.Collision;
using EternalJourney.Cores.Consts;
using EternalJourney.Enemy.Base;
using Godot;

public interface IExplosionBullet : IBaseBullet
{
}

[Meta(typeof(IAutoNode))]
public partial class ExplosionBullet : BaseBullet, IExplosionBullet
{
    public override void _Notification(int what) => this.Notify(what);

    /// <summary>
    /// 弾丸ロジック
    /// </summary>
    public BulletLogic BulletLogic { get; set; } = default!;

    /// <summary>
    /// 弾丸ロジックバインド
    /// </summary>
    public BulletLogic.IBinding BulletBinding { get; set; } = default!;

    [Node]
    public ITimer BlastTimer { get; set; } = default!;

    [Node]
    public ICollisionShape2D BulletCollisionShape2D { get; set; } = default!;

    [Node]
    public IColorRect BulletColorRect { get; set; } = default!;

    [Node]
    public ICollisionShape2D BlastCollisionShape2D { get; set; } = default!;

    [Node]
    public IColorRect BlastColorRect { get; set; } = default!;

    [Node]
    public IVisibleOnScreenNotifier2D VisibleOnScreenNotifier2D { get; set; } = default!;

    [Dependency] public IBattleRepo BattleRepo => this.DependOn<IBattleRepo>();

    public override void Setup()
    {
        base.Setup();

        // ステートロジックのインスタンス化
        BulletLogic = new BulletLogic();
        BulletBinding = BulletLogic.Bind();
        BulletLogic.Set(this as IBaseBullet);
        BulletLogic.Set(BattleRepo);
        BulletLogic.Set<IBulletCollisionStrategy>(CollisionStrategy);

        // コリジョンレイヤーの設定(自身の衝突レイヤーをBullet)
        CollisionLayer = CollisionEntity.Bullet;
        // コリジョンマスクの設定(衝突対象とする衝突レイヤーをEnemy)
        CollisionMask = CollisionEntity.Enemy;
    }

    public override void OnResolved()
    {
        base.OnResolved();

        BulletBinding
            .When<BulletLogic.State.EmitWait>(state =>
            {
                // 弾丸テクスチャ非表示と弾丸当たり判定無効化
                CallDeferred(nameof(SetBulletBodyEnabled), false);

                // 爆風テクスチャ非表示と爆風当たり判定無効
                CallDeferred(nameof(SetBlastBodyEnabled), false);
            })
            .When<BulletLogic.State.InFlight>(state =>
            {
                // 射出時の位置を設定(武器の発射口の位置)
                GlobalPosition = state.ShotGlobalPosition;
                // 射出時の方向を設定(武器の向いている方向)
                Direction = new Vector2(1, 0).Rotated(state.ShotGlobalAngle);
                // 弾丸の向きを設定（武器の向いている方向）
                Rotation = state.ShotGlobalAngle;

                // 弾丸テクスチャ表示と弾丸当たり判定有効化
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
                // 弾丸テクスチャ非表示と弾丸当たり判定無効化
                CallDeferred(nameof(SetBulletBodyEnabled), false);

                // 爆風テクスチャ表示と爆風当たり判定有効化
                CallDeferred(nameof(SetBlastBodyEnabled), true);

                // 爆風タイマースタート
                BlastTimer.Start();
            })
            .Handle((in BulletLogic.Output.RemoveSelf _) =>
            {
                // 爆風テクスチャ表示と爆風当たり判定無効化
                CallDeferred(nameof(SetBlastBodyEnabled), false);

                // フレーム終わりにRemoveSelf()呼び出し
                CallDeferred(nameof(RemoveSelf));
            });

        // コリジョンイベント設定
        AreaEntered += OnAreaEntered;
        // 画面外イベント
        VisibleOnScreenNotifier2D.ScreenExited += OnScreenExited;
        // ロジック初期化
        BulletLogic.Start();

        // 爆風タイマー設定（衝突ストラテジーからパラメータ取得）
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
    /// 射出
    /// </summary>
    /// <param name="shotGlobalPosition"></param>
    /// <param name="shotGlobalAngle"></param>
    public override void Emit(Vector2 shotGlobalPosition, float shotGlobalAngle)
    {
        BulletLogic.Input(new BulletLogic.Input.Emit(shotGlobalPosition, shotGlobalAngle));
    }

    /// <summary>
    /// 弾丸/爆風コリジョンエリア進入イベントファンクション
    /// </summary>
    /// <param name="area"></param>
    private void OnAreaEntered(Area2D area)
    {
        if (area is IBaseEnemy enemy)
        {
            BulletLogic.Input(new BulletLogic.Input.EnemyHit(enemy));
        }
    }

    /// <summary>
    /// スクリーン外イベントファンクション
    /// </summary>
    private void OnScreenExited()
    {
        BulletLogic.Input(new BulletLogic.Input.Miss());
    }

    /// <summary>
    /// 爆風タイマータイムアウトイベントファンクション
    /// </summary>
    private void OnBlastTimerTimeout()
    {
        BulletLogic.Input(new BulletLogic.Input.BlastTimerTimeout());
    }

    private void SetBulletBodyEnabled(bool flag)
    {
        if (flag)
        {
            BulletColorRect.Show();
            BulletCollisionShape2D.Disabled = false;
            return;
        }
        BulletColorRect.Hide();
        BulletCollisionShape2D.Disabled = true;
    }

    private void SetBlastBodyEnabled(bool flag)
    {
        if (flag)
        {
            BlastColorRect.Show();
            BlastCollisionShape2D.Disabled = false;
            return;
        }
        BlastColorRect.Hide();
        BlastCollisionShape2D.Disabled = true;
    }
}
