using System;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;

public interface ISkillButton : IControl
{
}

[Meta(typeof(IAutoNode))]
public partial class SkillButton : Control, ISkillButton
{
    public override void _Notification(int what) => this.Notify(what);

    [Node]
    public ITimer Timer { get; set; } = default!;

    [Node]
    public IColorRect ColorRect { get; set; } = default!;

    [Node]
    public ILabel CooldownLabel { get; set; } = default!;

    [Node]
    public IButton Button { get; set; } = default!;

    [Node]
    public ILabel ButtonLabel { get; set; } = default!;

    [Node]
    public IControl Control { get; set; } = default!;

    public void OnReady()
    {
    }

    public void Setup()
    {
    }

    public void OnResolved()
    {
        Button.Pressed += OnPressed;
        Timer.OneShot = true;
        Timer.WaitTime = 5;
        Timer.Timeout += OnTimerTimeout;
    }

    public void OnPhysicsProcess(double delta)
    {
        CooldownLabel.Text = Mathf.FloorToInt(Timer.TimeLeft + 1.0f).ToString();
        ColorRect.SetScale(new Vector2(1, CalcRectHeightRatio(Timer.WaitTime, Timer.TimeLeft)));
    }

    public void OnPressed()
    {
        GD.Print("Pressed");
        Button.Disabled = true;
        Timer.Start();
        Control.Show();
        CooldownLabel.Text = Timer.TimeLeft.ToString();
        SetPhysicsProcess(true);
    }

    public void OnTimerTimeout()
    {
        Button.Disabled = false;
        Control.Hide();
        SetPhysicsProcess(false);
    }

    private float CalcRectHeightRatio(double waitTime, double timeLeft)
    {
        double timeRatio = timeLeft / waitTime;
        return (float)timeRatio;
    }
}
