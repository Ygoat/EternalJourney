namespace EternalJourney.Bullet;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.App.State;
using EternalJourney.BulletFactory;
using EternalJourney.Cores.Consts;
using EternalJourney.Weapon;
using Godot;

/// <summary>
/// 弾丸インターフェース
/// </summary>
public interface IBullet : INode2D
{
    public event Bullet.HitEventHandler Hit;

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
    /// ヒットイベント
    /// </summary>
    [Signal]
    public delegate void HitEventHandler();
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
    public int Durability { get; set; } = 1;
    #endregion Exports

    #region Dependency
    /// <summary>
    /// 弾丸ファクトリー
    /// </summary>
    [Dependency]
    public IBulletFactory BulletFactory => this.DependOn<IBulletFactory>();

    /// <summary>
    /// 武器
    /// </summary>
    [Dependency]
    public IWeapon Weapon => this.DependOn<IWeapon>();
    #endregion Dependency

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Setup()
    {
        BulletLogic = new BulletLogic();
        BulletBinding = BulletLogic.Bind();
        // TODO:コリジョンレイヤーをEnumにする
        Area2D.CollisionLayer = CollisionEntity.Bullet;
        // TODO:コリジョンマスクをEnumにする
        Area2D.CollisionMask = CollisionEntity.Enemy;
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
                Durability -= 1;
                if (Durability <= 0)
                {
                    BulletLogic.Input(new BulletLogic.Input.Collapse());
                }
                else
                {
                    BulletLogic.Input(new BulletLogic.Input.Penetrate());
                }
            })
            .Handle((in BulletLogic.Output.Disappear _) =>
            {
                CallDeferred("RemoveSelf");
                GD.Print("Removed!");
                SetPhysicsProcess(false);
                BulletFactory.BulletsQueue.Enqueue(this);
                BulletLogic.Input(new BulletLogic.Input.Reload());
            });
        Area2D.AreaEntered += OnAreaEntered;
        VisibleOnScreenNotifier2D.ScreenExited += OnScreenExited;
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
    /// ※BulletFactoryから呼び出し
    /// </summary>
    public void Emit()
    {
        // Fireを入力
        BulletLogic.Input(new BulletLogic.Input.Fire());
        // 弾丸射出
        ThrustBullet(Weapon.Marker2D.GlobalPosition, new Vector2(1, 0).Rotated(Weapon.Rotation));
    }

    /// <summary>
    /// 弾丸射出
    /// </summary>
    /// <param name="shotGPosition">射出位置</param>
    /// <param name="shotDirection">射出角度</param>
    public void ThrustBullet(Vector2 shotGPosition, Vector2 shotDirection)
    {
        // 射出時の位置を設定(武器の発射口の位置)
        GlobalPosition = shotGPosition;
        // 射出時の方向を設定(武器の向いている方向)
        Direction = shotDirection;
        // 弾丸の向きを設定（武器の向いている方向）
        Rotation = shotDirection.Angle();
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
    }

}
