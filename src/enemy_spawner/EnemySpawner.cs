namespace EternalJourney.EnemySpawner;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.EnemyFactory;
using Godot;

public interface IEnemySpawner : INode2D, IProvide<IEnemySpawner>
{
    public IPathFollow2D PathFollow2D { get; set; }
}

[Meta(typeof(IAutoNode))]
public partial class EnemySpawner : Node2D, IEnemySpawner
{
    public override void _Notification(int what) => this.Notify(what);

    [Node]
    public IPathFollow2D PathFollow2D { get; set; } = default!;

    [Node]
    public IEnemyFactory EnemyFactory { get; set; } = default!;

    public int Speed { get; set; } = default!;

    IEnemySpawner IProvide<IEnemySpawner>.Value() => this;

    public void Initialize()
    {
        this.Provide();
        SetPhysicsProcess(true);
    }

    public void OnReady()
    {
    }

    public void OnPhysicsProcess(double delta)
    {
        PathFollow2D.ProgressRatio += (float)delta;
        GD.Print(PathFollow2D.GlobalPosition);
    }
}
