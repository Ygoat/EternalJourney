namespace EternalJourney.Enemy;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;
public interface IEnemy : INode2D { }

[Meta(typeof(IAutoNode))]
public partial class Enemy : Node2D, IEnemy
{
    public override void _Notification(int what) => this.Notify(what);

    #region Signals
    #endregion Signals
    #region State
    #endregion State
    #region Exports
    #endregion Exports
    #region PackedScenes
    #endregion PackedScenes
    #region
    [Node]
    public IArea2D Area2D { get; set; } = default!;
    #endregion Nodes
    #region Provisions
    #endregion Provisions
    #region Dependencies
    #endregion Dependencies

    public void Setup()
    {

    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
