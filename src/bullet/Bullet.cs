namespace EternalJourney.Bullet;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.App.State;
using Godot;

public interface IBullet : INode2D
{
    public event Bullet.HitEventHandler Hit;

}

[Meta(typeof(IAutoNode))]
public partial class Bullet : Node2D, IBullet
{
    public override void _Notification(int what) => this.Notify(what);

    public Vector2 Direction { get; set; } = new Vector2(0, 1);
    public float Speed { get; set; } = 10;

    [Signal]
    public delegate void HitEventHandler();

    [Node]
    public Area2D Area2D { get; set; } = default!;

    [Node]
    public VisibleOnScreenNotifier2D VisibleOnScreenNotifier2D { get; set; } = default!;

    public BulletLogic BulletLogic { get; set; } = default!;

    public BulletLogic.IBinding BulletBinding { get; set; } = default!;

    public void Setup()
    {
        BulletLogic = new BulletLogic();
        BulletBinding = BulletLogic.Bind();
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
                // 弾丸の耐久値を減らす
            })
            .Handle((in BulletLogic.Output.Disappear _) =>
            {
                GetParent().RemoveChild(this);
                GD.Print("Removed!");
                SetPhysicsProcess(false);
            });
        Area2D.AreaEntered += OnAreaEntered;
        TreeEntered += OnTreeentered;
        VisibleOnScreenNotifier2D.ScreenExited += OnScreenExited;
        BulletLogic.Start();
        BulletLogic.Input(new BulletLogic.Input.Fire());
        // BulletLogic.Input(new BulletLogic.Input.Hit());
        // BulletLogic.Input(new BulletLogic.Input.Collapse());
        // BulletLogic.Input(new BulletLogic.Input.Fire());
    }

    public void OnPhysicsProcess(double delta)
    {
        GlobalPosition += Direction * Speed;
    }

    public void OnAreaEntered(Area2D area)
    {
        EmitSignal(SignalName.Hit);
    }

    public void OnScreenExited()
    {
        BulletLogic.Input(new BulletLogic.Input.Miss());
    }

    public void OnTreeentered()
    {
        BulletLogic.Input(new BulletLogic.Input.Fire());
    }

    public void InitializeBullet()
    {
        GlobalPosition = new Vector2(0, 0);
    }

    public void ThrustBullet(Vector2 shotGPosition, Vector2 shotDirection)
    {
        GlobalPosition = shotGPosition;
        Direction = shotDirection;
    }
}
