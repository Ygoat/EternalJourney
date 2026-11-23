namespace EternalJourney.SkillButton;

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

    void SetLabel(string text);

    /// <summary>
    /// クールダウンの倍率を設定する（次回の発動から反映）
    /// </summary>
    void SetCooldownMultiplier(float multiplier);
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

    /// <summary>
    /// 基本クールダウン（秒）
    /// </summary>
    [Export]
    public float BaseCooldown { get; set; } = 5f;

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
        _cooldown = BaseCooldown;
        Timer.WaitTime = _cooldown;
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
        Timer.WaitTime = _cooldown;
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

    public void SetLabel(string text) => ButtonLabel.Text = text;

    public void SetCooldownMultiplier(float multiplier)
    {
        // クールダウン中に変えると残り時間表示の比率が崩れるため、次回の発動時に反映する
        _cooldown = BaseCooldown * multiplier;
    }

    /// <summary>
    /// 次回の発動で使うクールダウン（秒）
    /// </summary>
    private float _cooldown;

    private float CalcRectHeightRatio(double waitTime, double timeLeft)
    {
        double timeRatio = timeLeft / waitTime;
        return (float)timeRatio;
    }
}
