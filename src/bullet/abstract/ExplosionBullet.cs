namespace EternalJourney.Bullet.Abstract;

using System;
using System.Net.Http.Headers;
using System.Xml;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Bullet.Abstract.Base;
using EternalJourney.Bullet.Abstract.State;
using EternalJourney.Common.Traits;
using EternalJourney.Cores.Consts;
using EternalJourney.Enemy.Abstract.Base;
using Godot;

public interface IExplosionBullet : IBaseBullet
{
}

[Meta(typeof(IAutoNode))]
public partial class ExplosionBullet : BaseBullet, IExplosionBullet
{
    public override void _Notification(int what) => this.Notify(what);

    public ExplosionBulletLogic ExplosionBulletLogic { get; set; } = default!;

    public ExplosionBulletLogic.IBinding ExplosionBulletBinding { get; set; } = default!;

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

    public Vector2 Direction { get; set; } = default!;

    public override void Setup()
    {
        base.Setup();

        // ステートロジックのインスタンス化
        ExplosionBulletLogic = new ExplosionBulletLogic();
        ExplosionBulletBinding = ExplosionBulletLogic.Bind();
        ExplosionBulletLogic.Set(this as IBaseBullet);

        // コリジョンレイヤーの設定(自身の衝突レイヤーをBullet)
        CollisionLayer = CollisionEntity.Bullet;
        // コリジョンマスクの設定(衝突対象とする衝突レイヤーをEnemy)
        CollisionMask = CollisionEntity.Enemy;

        Status = new Status { Spd = 1, MaxDur = 0.1f, CurrentDur = 0.1f };
    }

    public override void OnResolved()
    {
        base.OnResolved();

        ExplosionBulletBinding
            .When<ExplosionBulletLogic.State.EmitWait>(state =>
            {
                // 弾丸テクスチャ非表示と弾丸当たり判定無効化
                BulletColorRect.Hide();
                BulletCollisionShape2D.Disabled = true;

                // 爆風テクスチャ非表示と爆風当たり判定無効
                BlastColorRect.Hide();
                BlastCollisionShape2D.Disabled = true;
            })
            .Handle((in ExplosionBulletLogic.Output.Emitted output) =>
            {
                // 射出時の位置を設定(武器の発射口の位置)
                GlobalPosition = output.ShotGlobalPosition;
                // 射出時の方向を設定(武器の向いている方向)
                Direction = new Vector2(1, 0).Rotated(output.ShotGlobalAngle);
                // 弾丸の向きを設定（武器の向いている方向）
                Rotation = output.ShotGlobalAngle;
            })
            .When<ExplosionBulletLogic.State.InFlight>(state =>
            {
                // 弾丸テクスチャ表示と弾丸当たり判定有効化
                BulletColorRect.Show();
                BulletCollisionShape2D.Disabled = false;

                SetPhysicsProcess(true);
            })
            .Handle((in ExplosionBulletLogic.Output.Move output) =>
            {
                GlobalPosition += output.NextPositionDelta;
            })
            .Handle((in ExplosionBulletLogic.Output.CurrentDurChange output) =>
            {
                Status.CurrentDur = output.CurrentDur;
            })
            .Handle((in ExplosionBulletLogic.Output.Collapse output) =>
            {
                SetPhysicsProcess(false);
            })
            .When<ExplosionBulletLogic.State.Blast>(state =>
            {
                // 弾丸テクスチャ非表示と弾丸当たり判定無効化
                BulletColorRect.Hide();
                BulletCollisionShape2D.Disabled = true;

                // 爆風テクスチャ表示と爆風当たり判定有効化
                BlastColorRect.Show();
                BlastCollisionShape2D.Disabled = false;
            })
            .Handle((in ExplosionBulletLogic.Output.BlastEnd _) =>
            {
                // 爆風テクスチャ表示と爆風当たり判定無効化
                BlastColorRect.Hide();
                BlastCollisionShape2D.Disabled = true;

                // フレーム終わりにRemoveSelf()呼び出し
                CallDeferred(nameof(RemoveSelf));
            });

        // コリジョンイベント設定
        AreaEntered += OnAreaEntered;
        // 画面外イベント
        VisibleOnScreenNotifier2D.ScreenExited += OnScreenExited;
        // ロジック初期化
        ExplosionBulletLogic.Start();
        // Emit(new Vector2(1, 0), 0);
        // ExplosionBulletLogic.Input(new ExplosionBulletLogic.Input.EnemyHit());
        // ExplosionBulletLogic.Input(new ExplosionBulletLogic.Input.BlastTimerTimeout());
        // InitializeBullet();
        // Emit(new Vector2(1, 0), 0);
        // トップレベルオブジェクトとして扱う（親ノードのRotationの影響を受けないようにするため）
        TopLevel = true;
    }

    public void OnPhysicsProcess(double delta)
    {
        ExplosionBulletLogic.Input(new ExplosionBulletLogic.Input.PhysicsProcess(new Vector2(1, 0), Status.Spd));
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="shotGlobalPosition"></param>
    /// <param name="shotGlobalAngle"></param>
    public override void Emit(Vector2 shotGlobalPosition, float shotGlobalAngle)
    {
        ExplosionBulletLogic.Input(new ExplosionBulletLogic.Input.Emit(shotGlobalPosition, shotGlobalAngle));
    }



    /// <summary>
    /// 弾丸/爆風コリジョンエリア進入イベントファンクション
    /// </summary>
    /// <param name="area"></param>
    private void OnAreaEntered(Area2D area)
    {
        if (area is IBaseEnemy enemy)
        {
            ExplosionBulletLogic.Input(new ExplosionBulletLogic.Input.EnemyHit(enemy));
        }
    }

    /// <summary>
    /// スクリーン外イベントファンクション
    /// </summary>
    private void OnScreenExited()
    {
        ExplosionBulletLogic.Input(new ExplosionBulletLogic.Input.Miss());
    }

    /// <summary>
    /// 爆風タイマータイムアウトイベントファンクション
    /// </summary>
    private void OnBlastTimerTimeout()
    {
        ExplosionBulletLogic.Input(new ExplosionBulletLogic.Input.BlastTimerTimeout());
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
