namespace EternalJourney.Bullet.Abstract.Base;

using System;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using EternalJourney.Bullet.Strategies.Collision;
using EternalJourney.Bullet.Strategies.Movement;
using EternalJourney.Common.BaseEntity;
using EternalJourney.Common.StatusEffect;
using EternalJourney.Common.Traits;
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

    /// <summary>
    /// 移動戦略リソース
    /// </summary>
    [Export]
    public BulletMovementStrategyResource? MovementStrategyResource { get; set; }

    /// <summary>
    /// 衝突戦略リソース
    /// </summary>
    [Export]
    public BulletCollisionStrategyResource? CollisionStrategyResource { get; set; }

    /// <summary>
    /// 毒状態異常の有効・無効
    /// </summary>
    [Export]
    public bool PoisonEnabled { get; set; }

    /// <summary>
    /// スタン状態異常の有効・無効
    /// </summary>
    [Export]
    public bool StunEnabled { get; set; }

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

        // 移動・衝突ストラテジーを毎回新規生成（プール内での状態共有を避けるため）
        MovementStrategy = MovementStrategyResource?.CreateStrategy() ?? new LinearBulletMovement();
        MovementStrategy.Initialize();
        CollisionStrategy = CollisionStrategyResource?.CreateStrategy() ?? new NormalCollisionStrategy();
        CollisionStrategy.Initialize();

        // 状態異常設定を適用
        StatusEffectServerManager.Configure<PoisonEffect>(PoisonEnabled);
        StatusEffectServerManager.Configure<StunEffect>(StunEnabled);
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
