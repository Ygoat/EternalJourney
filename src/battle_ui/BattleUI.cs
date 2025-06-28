namespace EternalJourney.BattleUI;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Battle.Domain;
using EternalJourney.BattleUI.State;
using Godot;

/// <summary>
/// バトルUIインターフェース
/// </summary>
public interface IBattleUI : ICanvasLayer
{
}

/// <summary>
/// バトルUIクラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class BattleUI : CanvasLayer, IBattleUI, IProvide<IBattleRepo>
{
    public override void _Notification(int what) => this.Notify(what);

    /// <summary>
    /// タイマーラベル
    /// </summary>
    [Node]
    public ILabel TimerLabel { get; set; } = default!;

    /// <summary>
    /// スコアラベル
    /// </summary>
    [Node]
    public ILabel ScoreLabel { get; set; } = default!;

    /// <summary>
    /// バトルUIロジック
    /// </summary>
    public IBattleUILogic BattleUILogic { get; set; } = default!;

    /// <summary>
    /// バトルUIバインド
    /// </summary>
    public BattleUILogic.IBinding BattleUIBinding { get; set; } = default!;

    public float Count { get; set; } = default!;

    /// <summary>
    /// バトルリポジトリ
    /// </summary>
    [Dependency] public IBattleRepo BattleRepo => this.DependOn<IBattleRepo>();


    /// <summary>
    /// バトルレポジトリプロバイダー
    /// </summary>
    /// <returns></returns>
    IBattleRepo IProvide<IBattleRepo>.Value() => BattleRepo;

    public void OnReady()
    {
    }

    public void Setup()
    {
        TimerLabel.Text = "Timer";
        ScoreLabel.Text = "Score";
        SetPhysicsProcess(true);
        BattleUILogic = new BattleUILogic();
    }

    public void OnResolved()
    {
        BattleUILogic.Set(BattleRepo);
        BattleUIBinding = BattleUILogic.Bind();
        BattleUIBinding.Handle((in BattleUILogic.Output.ScoreChanged output) =>
        {
            SetScoreLabel(output.CurrentScore);
        });
        BattleUILogic.Start();
    }

    public void SetScoreLabel(int score)
    {
        ScoreLabel.Text = $"Score: {score}";
    }

    public void OnPhysicsProcess(double delta)
    {
        Count++;
        TimerLabel.Text = $"Time: {Count}";
    }
}
