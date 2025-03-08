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

    public Vector2 Direction { get; set; } = new Vector2(0, 0);
    public int Speed { get; set; }

    [Signal]
    public delegate void HitEventHandler();

    [Node]
    public Area2D Area2D { get; set; } = default!;

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
            .Handle((in BulletLogic.Output.Disappear _) =>
            {
                Remove();
            });
        Area2D.AreaEntered += OnAreaEntered;
        TreeEntered += Emit;
        BulletLogic.Start();
    }


    public void OnAreaEntered(Area2D area)
    {
        EmitSignal(SignalName.Hit);
    }

    public void Remove()
    {
        GetParent().RemoveChild(this);
        BulletLogic.Input(new BulletLogic.Input.Reload());
    }

    public void Emit()
    {
        BulletLogic.Input(new BulletLogic.Input.Fire());
    }

    public void Initialize()
    {
        GlobalPosition = new Vector2(0, 0);
    }

    public void Thrust(Vector2 shotGPosition, Vector2 shotDirection)
    {
        GlobalPosition = shotGPosition;
        Direction = shotDirection;
    }
}
