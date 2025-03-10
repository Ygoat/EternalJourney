namespace EternalJourney.Bullet;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.App.State;
using EternalJourney.BulletFactory;
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
    public Vector2 Direction { get; set; } = new Vector2(0, 1);

    /// <summary>
    /// 速度
    /// </summary>
    public float Speed { get; set; } = 10;

    /// <summary>
    /// 耐久値
    /// </summary>
    public int Durability { get; set; } = 1;
    #endregion Exports

    [Dependency]
    public IBulletFactory BulletFactory => this.DependOn<IBulletFactory>();
    [Dependency]
    public IWeapon Weapon => this.DependOn<IWeapon>();

    public void Setup()
    {
        BulletLogic = new BulletLogic();
        BulletBinding = BulletLogic.Bind();
        // TODO:コリジョンレイヤーをEnumにする
        Area2D.CollisionLayer = 0;
        // TODO:コリジョンマスクをEnumにする
        Area2D.CollisionMask = 1;
    }

    public void OnReady()
    {
        BulletBinding
            .Handle((in BulletLogic.Output.Emitted _) =>
            {
                SetPhysicsProcess(true);
            })
            .Handle((in BulletLogic.Output.Decay _) =>
            {
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
                GetParent().RemoveChild(this);
                InitializeBullet();
                GD.Print("Removed!");
                SetPhysicsProcess(false);
                BulletFactory.BulletsQueue.Enqueue(this);
                BulletLogic.Input(new BulletLogic.Input.Reload());
            });
        Area2D.AreaEntered += OnAreaEntered;
        VisibleOnScreenNotifier2D.ScreenExited += OnScreenExited;
        BulletLogic.Start();
    }

    public void OnPhysicsProcess(double delta)
    {
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
    /// 弾丸射出
    /// </summary>
    /// <param name="shotGPosition">射出位置</param>
    /// <param name="shotDirection">射出角度</param>
    public void ThrustBullet(Vector2 shotGPosition, Vector2 shotDirection)
    {
        GlobalPosition = shotGPosition;
        Direction = shotDirection;
    }

    /// <summary>
    /// 弾丸初期化
    /// </summary>
    public void InitializeBullet()
    {
        GlobalPosition = new Vector2(0, 0);
    }

    /// <summary>
    /// 射出
    /// ※BulletFactoryから呼び出し
    /// </summary>
    public void Emit()
    {
        BulletLogic.Input(new BulletLogic.Input.Fire());
        ThrustBullet(Weapon.Marker2D.GlobalPosition, new Vector2(0, 1).Rotated(Weapon.Rotation));
    }

}
