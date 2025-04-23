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
using Godot;


/// <summary>
/// スタンダード弾丸インターフェース
/// </summary>
public interface IStandardBullet : IBaseBullet, IMovable, IResizable
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

    [Dependency] public EntityTable EntityTable => this.DependOn<EntityTable>();
    [Dependency] public IBattleRepo BattleRepo => this.DependOn<IBattleRepo>();

    #endregion State

    #region Exports
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public bool CanMove { get; set; } = true;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public float Size { get; set; } = 1.0f;

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
        // コリジョンレイヤーを弾丸
        CollisionLayer = CollisionEntity.Bullet;
        // コリジョンマスクをエネミー
        CollisionMask = CollisionEntity.Enemy;
        // ステータスセット
        SetStatus(new Status { Spd = 5.0f, MaxDur = 0.1f });
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void OnResolved()
    {
        StandardBulletBinding
            // Emittedが出力された場合
            .Handle((in StandardBulletLogic.Output.Emitted _) =>
            {
                // 物理処理有効化
                SetPhysicsProcess(true);
            })
            // Decayが出力された場合
            .Handle((in StandardBulletLogic.Output.Decay _) =>
            {
                // 弾丸の耐久値を減少
                DurabilityModule.TakeDamage(1.0f);
            })
            // Disappearが出力された場合
            .Handle((in StandardBulletLogic.Output.Disappear _) =>
            {
                // フレーム終わりにRemoveSelf()呼び出し
                CallDeferred(nameof(RemoveSelf));

            });
        // コリジョンイベント設定
        AreaEntered += OnAreaEntered;
        // 画面外イベント
        VisibleOnScreenNotifier2D.ScreenExited += OnScreenExited;
        // 耐久値ゼロイベント設定
        DurabilityModule.ZeroDurability += OnZeroDurability;
        // 耐久値残存イベント設定
        DurabilityModule.DurabilityLeft += OnDurabilityLeft;
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
        // グローバル位置の更新
        Move();
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
        // Reload入力
        StandardBulletLogic.Input(new StandardBulletLogic.Input.Reload());
    }

    /// <summary>
    /// 弾丸位置移動
    /// </summary>
    public virtual void Move()
    {
        GlobalPosition += Direction.Normalized() * Status.Spd;
    }

    /// <summary>
    /// 衝突時の処理
    /// </summary>
    /// <param name="area"></param>
    public void OnAreaEntered(Area2D area)
    {
        // ヒットを入力
        StandardBulletLogic.Input(new StandardBulletLogic.Input.Hit());
        // ヒットシグナルを出力
        // EmitSignal(BaseBullet.SignalName.Hit);
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
        // Fireを入力
        StandardBulletLogic.Input(new StandardBulletLogic.Input.Fire());
        // 射出時の位置を設定(武器の発射口の位置)
        GlobalPosition = shotGlobalPosition;
        // 射出時の方向を設定(武器の向いている方向)
        Direction = new Vector2(1, 0).Rotated(shotGlobalAngle);
        // 弾丸の向きを設定（武器の向いている方向）
        Rotation = shotGlobalAngle;
    }

    /// <summary>
    /// 射出（弾丸攻撃力設定）
    /// </summary>
    /// <param name="shotGlobalPosition"></param>
    /// <param name="shotGlobalAngle"></param>
    /// <param name="atk"></param>
    public void Emit(Vector2 shotGlobalPosition, float shotGlobalAngle, float atk)
    {
        SetBulletAtk(atk);
        Emit(shotGlobalPosition, shotGlobalAngle);
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
        DurabilityModule.FullRepir();
    }

    /// <summary>
    /// 弾丸攻撃力設定
    /// </summary>
    /// <param name="atk"></param>
    private void SetBulletAtk(float atk)
    {
        Status.Atk = atk;
    }

    /// <summary>
    /// 耐久値ゼロイベントファンクション
    /// </summary>
    private void OnZeroDurability()
    {
        StandardBulletLogic.Input(new StandardBulletLogic.Input.Collapse());
    }

    /// <summary>
    /// 耐久値残存イベントファンクション
    /// </summary>
    private void OnDurabilityLeft()
    {
        StandardBulletLogic.Input(new StandardBulletLogic.Input.Penetrate());
    }
}
