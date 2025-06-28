namespace EternalJourney.StatusUpSkill;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;

public interface IStatusUpSkill : INode
{
    public void Apply();
}

[Meta(typeof(IAutoNode))]
public partial class StatusUpSkill : Node, IStatusUpSkill
{
    public override void _Notification(int what) => this.Notify(what);

    public Timer Timer { get; set; } = default!;

    public void Initialize()
    {
    }

    public void Setup()
    {
        Timer = new Timer();
    }

    public void OnResolved()
    {
        AddChild(Timer);
        Timer.OneShot = true;
        Timer.WaitTime = 3;
        Timer.Timeout += OnTimerTimeout;
    }

    public void OnTimerTimeout()
    {
        GD.Print("Timeout");
    }

    public void Apply()
    {
        GD.Print("Apply");
        Timer.Start();
    }
}
