namespace EternalJourney.Bullet;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.App.State;
using EternalJourney.Common.DurabilityModule;
using EternalJourney.Cores.Consts;
using Godot;

/// <summary>
/// 弾丸インターフェース
/// </summary>
public interface IBullet : INode2D
{
    /// <summary>
    /// ヒットシグナル
    /// </summary>
    public event Bullet.HitEventHandler Hit;

    /// <summary>
    /// 崩壊シグナル
    /// </summary>
    public event Bullet.CollapsedEventHandler Collapsed;

    /// <summary>
    /// 弾丸射出
    /// </summary>
    /// <param name="shotGlobalPosition"></param>
    /// <param name="shotGlobalAngle"></param>
    public void Emit(Vector2 shotGlobalPosition, float shotGlobalAngle);
}

/// <summary>
/// 弾丸クラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class Bullet : Node2D, IBullet
{
    public override void _Notification(int what) => this.Notify(what);

    #region Signals
    /// <summary>
    /// ヒットシグナル
    /// </summary>
    [Signal]
    public delegate void HitEventHandler();

    /// <summary>
    /// 崩壊シグナル
    /// </summary>
    [Signal]
    public delegate void CollapsedEventHandler(Bullet bullet);
    #endregion Signals

    #region State
    /// <summary>
    /// 弾丸ロジック
    /// </summary>
    public BulletLogic BulletLogic { get; set; } = default!;

    /// <summary>
    /// 弾丸ロジックバインド
    /// </summary>
    public BulletLogic.IBinding BulletBinding { get; set; } = default!;
    #endregion State

    #region Nodes
    /// <summary>
    /// ヒット判定用エリア
    /// </summary>
    [Node]
    public IArea2D Area2D { get; set; } = default!;

    /// <summary>
    /// 画面外検知用通知ノード
    /// </summary>
    [Node]
    public IVisibleOnScreenNotifier2D VisibleOnScreenNotifier2D { get; set; } = default!;

    /// <summary>
    /// 耐久値モジュール
    /// </summary>
    [Node]
    public IDurabilityModule DurabilityModule { get; set; } = default!;
    #endregion Nodes

    #region Exports
    /// <summary>
    /// 移動方向
    /// </summary>
    public Vector2 Direction { get; set; } = new Vector2(1, 0);

    /// <summary>
    /// 速度
    /// </summary>
    public float Speed { get; set; } = 10;

    /// <summary>
    /// 耐久値
    /// </summary>
    public double Durability { get; set; } = 0.1;
    #endregion Exports

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Setup()
    {
        BulletLogic = new BulletLogic();
        BulletBinding = BulletLogic.Bind();
        // コリジョンレイヤーを弾丸
        Area2D.CollisionLayer = CollisionEntity.Bullet;
        // コリジョンマスクをエネミー
        Area2D.CollisionMask = CollisionEntity.Enemy;
        // 耐久値セット
        DurabilityModule.SetDurability(Durability);
        // 耐久値ゼロイベント設定
        DurabilityModule.ZeroDurability += OnZeroDurability;
        // 耐久値残存イベント設定
        DurabilityModule.DurabilityLeft += OnDurabilityLeft;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void OnResolved()
    {
        BulletBinding
            // Emittedが出力された場合
            .Handle((in BulletLogic.Output.Emitted _) =>
            {
                // 物理処理有効化
                SetPhysicsProcess(true);
            })
            // Decayが出力された場合
            .Handle((in BulletLogic.Output.Decay _) =>
            {
                // 弾丸の耐久値を減少
                DurabilityModule.TakeDamage(1.0);
            })
            // Disappearが出力された場合
            .Handle((in BulletLogic.Output.Disappear _) =>
            {
                // フレーム終わりにRemoveSelf()呼び出し
                CallDeferred("RemoveSelf");
                // 物理処理無効化
                SetPhysicsProcess(false);
                // OnCollapsedシグナル出力
                EmitSignal(SignalName.Collapsed, this);
                // Reload入力
                BulletLogic.Input(new BulletLogic.Input.Reload());
            });
        // コリジョンイベント設定
        Area2D.AreaEntered += OnAreaEntered;
        // 画面外イベント
        VisibleOnScreenNotifier2D.ScreenExited += OnScreenExited;
        // ロジック初期化
        BulletLogic.Start();
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
        GlobalPosition += Direction.Normalized() * Speed;
    }

    /// <summary>
    /// 衝突時の処理
    /// </summary>
    /// <param name="area"></param>
    public void OnAreaEntered(Area2D area)
    {
        // ヒットを入力
        BulletLogic.Input(new BulletLogic.Input.Hit());
        // ヒットシグナルを出力
        EmitSignal(SignalName.Hit);
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
    public void Emit(Vector2 shotGlobalPosition, float shotGlobalAngle)
    {
        // Fireを入力
        BulletLogic.Input(new BulletLogic.Input.Fire());
        // 射出時の位置を設定(武器の発射口の位置)
        GlobalPosition = shotGlobalPosition;
        // 射出時の方向を設定(武器の向いている方向)
        Direction = new Vector2(1, 0).Rotated(shotGlobalAngle);
        // 弾丸の向きを設定（武器の向いている方向）
        Rotation = shotGlobalAngle;
    }

    /// <summary>
    /// 自インスタンスをツリーから一時的に取り除く
    /// ※インスタンスは完全には削除されない
    /// </summary>
    public void RemoveSelf()
    {
        // 親ノードを取得してから、子である自ノードを削除する
        GetParent().RemoveChild(this);
        // 弾丸の初期化
        InitializeBullet();
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
    /// 耐久値ゼロイベントファンクション
    /// </summary>
    private void OnZeroDurability()
    {
        BulletLogic.Input(new BulletLogic.Input.Collapse());
    }

    /// <summary>
    /// 耐久値残存イベントファンクション
    /// </summary>
    private void OnDurabilityLeft()
    {
        BulletLogic.Input(new BulletLogic.Input.Penetrate());
    }
}
