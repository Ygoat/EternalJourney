namespace EternalJourney.BattleUI;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;

public interface IBattleUI : ICanvasLayer
{
}

[Meta(typeof(IAutoNode))]
public partial class BattleUI : CanvasLayer, IBattleUI
{
    public override void _Notification(int what) => this.Notify(what);

    [Node]
    public ILabel TimerLabel { get; set; } = default!;

    [Node]
    public ILabel ScoreLabel { get; set; } = default!;

    public float Count { get; set; } = default!;

    public void OnReady()
    {
    }

    public void Setup()
    {
        TimerLabel.Text = "Timer";
        ScoreLabel.Text = "Score";
        SetPhysicsProcess(true);
    }

    public void OnResolved()
    {
    }

    public void OnPhysicsProcess(double delta)
    {
        Count++;
        TimerLabel.Text = $"Time: {Count}";
        ScoreLabel.Text = $"Score: {Count}";
    }
}
