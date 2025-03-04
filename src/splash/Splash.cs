namespace EternalJourney;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.App.Domain;
using Godot;

public interface ISplash : IControl;

[Meta(typeof(IAutoNode))]
public partial class Splash : Control, ISplash
{
    public override void _Notification(int what) => this.Notify(what);

    [Node]
    public ITimer SplashTimer { get; set; } = default!;

    [Dependency]
    public IAppRepo AppRepo => this.DependOn<IAppRepo>();

    public void OnReady()
    {
        SplashTimer.Timeout += SplashTimeOut;
        SplashTimer.SetWaitTime(2);
        SplashTimer.Start();
    }

    public void SplashTimeOut()
    {
        GD.Print("splash timeout");
        AppRepo.SkipSplashScreen();
    }

    // // Called when the node enters the scene tree for the first time.
    // public override void _Ready()
    // {
    // }

    // // Called every frame. 'delta' is the elapsed time since the previous frame.
    // public override void _Process(double delta)
    // {
    // }
}
