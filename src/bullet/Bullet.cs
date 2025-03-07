namespace EternalJourney.Bullet;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;

public interface IBullet : INode2D { }

[Meta(typeof(IAutoNode))]
public partial class Bullet : Node2D, IBullet
{
    public override void _Notification(int what) => this.Notify(what);

    [Node]
    public Area2D Area2D { get; set; } = default!;

    public void OnReady()
    {

    }
}
