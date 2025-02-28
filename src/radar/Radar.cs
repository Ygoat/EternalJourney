namespace EternalJourney;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;

[Meta(typeof(IAutoNode))]
public partial class Radar : Node2D
{
    [Node("%SearchArea")]
    public IArea2D Area2D { get; set; } = default!;

    public void OnResolved()
    {
        Area2D.BodyEntered += OnAreaBodyEntered;
    }

    public void OnAreaBodyEntered(Node2D area)
    {
        GD.Print("Entered!");
    }
}
