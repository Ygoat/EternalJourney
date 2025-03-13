namespace EternalJourney.Ship;

using Chickensoft.AutoInject;
using Chickensoft.Collections;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Cores.Consts;
using Godot;

public interface IShip : INode2D
{
    public IMarker2D EnemyTargetMarker { get; set; }
};

[Meta(typeof(IAutoNode))]
public partial class Ship : Node2D, IShip
{
    public override void _Notification(int what) => this.Notify(what);
    // Called when the node enters the scene tree for the first time.

    [Node]
    public IMarker2D EnemyTargetMarker { get; set; } = default!;

    [Node]
    public IArea2D Area2D { get; set; } = default!;

    [Dependency]
    public EntityTable<int> EntityTable => this.DependOn<EntityTable<int>>();

    public void Setup()
    {
        Area2D.CollisionLayer = CollisionEntity.Ship;
        Area2D.CollisionMask = CollisionEntity.Enemy;
    }

    public void OnResolved()
    {
        EntityTable.Set(0, this);
    }

}
