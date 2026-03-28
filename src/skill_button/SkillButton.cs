namespace EternalJourney.SukillButton;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;

public interface ISkillButton : IControl
{
    /// <summary>
    /// スキルボタン押下シグナル
    /// </summary>
    public event SkillButton.ActivatedEventHandler Activated;
}

[Meta(typeof(IAutoNode))]
public partial class SkillButton : Control, ISkillButton
{
    public override void _Notification(int what) => this.Notify(what);

    /// <summary>
    /// スキル発動シグナル
    /// </summary>
    [Signal]
    public delegate void ActivatedEventHandler();

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
        EmitSignal(SignalName.Activated);
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
