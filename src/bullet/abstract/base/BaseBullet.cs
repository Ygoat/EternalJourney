namespace EternalJourney.Bullet.Abstract.Base;

using System;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using EternalJourney.Bullet.Strategies.Collision;
using EternalJourney.Bullet.Strategies.Movement;
using EternalJourney.Common.BaseEntity;
using EternalJourney.Common.StatusEffect;
using EternalJourney.Common.Traits;
using EternalJourney.Cores.Models.Bullet;
using EternalJourney.Cores.Pooling;
using Godot;

/// <summary>
/// ベース弾丸インターフェース
/// </summary>
public interface IBaseBullet : IBaseEntity
{
    /// <summary>
    /// ヒットシグナル
    /// </summary>
    public event BaseBullet.HitEventHandler Hit;

    /// <summary>
    /// 除去シグナル
    /// </summary>
    public event BaseBullet.RemovedEventHandler Removed;

    /// <summary>
    /// 弾丸射出
    /// </summary>
    /// <param name="shotGlobalPosition"></param>
    /// <param name="shotGlobalAngle"></param>
    public void Emit(Vector2 shotGlobalPosition, float shotGlobalAngle);

    /// <summary>
    /// 状態異常付与マネージャー
    /// </summary>
    public StatusEffectServerManager StatusEffectServerManager { get; set; }

    /// <summary>
    /// 衝突ストラテジー
    /// </summary>
    public IBulletCollisionStrategy CollisionStrategy { get; set; }
}

/// <summary>
/// ベース弾丸クラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class BaseBullet : BaseEntity, IBaseBullet, IPoolable
{
    public override void _Notification(int what) => this.Notify(what);

    /// <summary>
    /// ヒットシグナル
    /// </summary>
    [Signal]
    public delegate void HitEventHandler();

    /// <summary>
    /// 自己除去イベント
    /// </summary>
    [Signal]
    public delegate void RemovedEventHandler(BaseBullet bullet);

    public StatusEffectServerManager StatusEffectServerManager { get; set; } = new StatusEffectServerManager();

    /// <summary>
    /// 移動方向
    /// </summary>
    public Vector2 Direction { get; set; } = new Vector2(1, 0);

    /// <summary>
    /// 移動ストラテジー
    /// </summary>
    public IBulletMovementStrategy MovementStrategy { get; set; } = new LinearBulletMovement();

    /// <summary>
    /// 衝突ストラテジー
    /// </summary>
    public IBulletCollisionStrategy CollisionStrategy { get; set; } = new NormalCollisionStrategy();

    /// <summary>
    /// 経過時間
    /// </summary>
    public float ElapsedTime { get; set; }

    public override void Setup()
    {
        TopLevel = true;
    }

    public override void OnResolved()
    {
        base.OnResolved();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="shotGlobalPosition"></param>
    /// <param name="shotGlobalAngle"></param>
    public virtual void Emit(Vector2 shotGlobalPosition, float shotGlobalAngle)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 自インスタンスをツリーから一時的に取り除く
    /// ※インスタンスは完全には削除されない
    /// </summary>
    public virtual void RemoveSelf()
    {
        // 親ノードを取得してから、子である自ノードを削除する
        GetParent().RemoveChild(this);
        // 弾丸の初期化
        InitializeBullet();
        // 物理処理無効化
        SetPhysicsProcess(false);
        // Removedシグナル出力
        EmitSignal(SignalName.Removed, this);
    }

    /// <summary>
    /// 弾丸設定を適用
    /// </summary>
    /// <param name="config">弾丸設定</param>
    public virtual void Configure(BulletConfig config)
    {
        // ステータスを設定から適用
        Status = new Status
        {
            MaxDur = config.Status.MaxDur,
            CurrentDur = config.Status.MaxDur,
            Atk = config.Status.Atk,
            Spd = config.Status.Spd,
            Def = config.Status.Def,
            Size = config.Status.Size
        };

        // 状態異常をJSONから設定
        foreach (var effectConfig in config.StatusEffects)
        {
            ConfigureStatusEffect(effectConfig);
        }

        // 移動ストラテジーを生成・初期化
        MovementStrategy = BulletMovementStrategyFactory.Create(config.Movement.Type);
        MovementStrategy.Initialize(config.Movement);

        // 衝突ストラテジーを生成・初期化
        CollisionStrategy = BulletCollisionStrategyFactory.Create(config.Collision.Type);
        CollisionStrategy.Initialize(config.Collision);
    }

    /// <summary>
    /// 状態異常の設定を適用
    /// </summary>
    /// <param name="effectConfig">状態異常設定</param>
    private void ConfigureStatusEffect(BulletStatusEffectConfig effectConfig)
    {
        // 状態異常タイプに応じてConfigureを呼び出す
        // 状態異常が増えたらcaseを追加する
        switch (effectConfig.Type)
        {
            case BulletStatusEffectType.Poison:
                StatusEffectServerManager.Configure<PoisonEffect>(effectConfig.Enabled);
                break;
            case BulletStatusEffectType.Stun:
                StatusEffectServerManager.Configure<StunEffect>(effectConfig.Enabled);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 弾丸初期化
    /// </summary>
    public virtual void InitializeBullet()
    {
        // グローバル座標の初期化
        GlobalPosition = new Vector2(0, 0);
        // 方向を初期化
        Direction = new Vector2(0, 0);
        // 耐久値を回復
        Status.CurrentDur = Status.MaxDur;
        // 経過時間リセット
        ElapsedTime = 0f;
        // ストラテジーリセット
        MovementStrategy.Reset();
        CollisionStrategy.Reset();
    }

    /// <summary>
    /// プールから取得された時のコールバック（IPoolable実装）
    /// </summary>
    public virtual void OnAcquired()
    {
        // 耐久値を最大値にリセット
        if (Status != null)
        {
            Status.CurrentDur = Status.MaxDur;
        }
        // 表示状態を有効化
        Visible = true;
    }

    /// <summary>
    /// プールに返却される時のコールバック（IPoolable実装）
    /// </summary>
    public virtual void OnReleased()
    {
        // 表示状態を無効化
        Visible = false;
        // 状態異常をクリア
        if (StatusEffectServerManager != null)
        {
            // 状態異常のクリーンアップ処理
        }
    }
}
