namespace EternalJourney.AutoConnectTest;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.AutoConnectTest.State;
using Godot;

public interface IAutoConnectTestNode : INode
{
    public event AutoConnectTestNode.TestEmitEventHandler TestEmit;
}

[Meta(typeof(IAutoNode))]
public partial class AutoConnectTestNode : Node, IAutoConnectTestNode
{
    public override void _Notification(int what) => this.Notify(what);

    [Node]
    public IButton AutoConnectTestButton { get; set; } = default!;

    [Signal]
    public delegate void TestEmitEventHandler();

    private LightSwitchLogic lightSwitchLogic { get; set; }

    public int PressedCount { get; set; }

    // Called when the node enters the scene tree for the first time.

    public void OnReady()
    {
        AutoConnectTestButton.Pressed += OnAutoConnectPressed;

        lightSwitchLogic = new LightSwitchLogic();
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void OnAutoConnectPressed()
    {
        PressedCount++;
        GD.Print(PressedCount);
        if (PressedCount == 10)
        {
            lightSwitchLogic.Input(new LightSwitchLogic.Input.Toggle());
            EmitSignal(SignalName.TestEmit);
        }
        GD.Print(lightSwitchLogic.Value);
    }
}
