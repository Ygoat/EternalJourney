namespace EternalJourney.BattleUI;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Battle.Domain;
using EternalJourney.BattleUI.State;
using EternalJourney.StatusUpSkill;
using EternalJourney.SukillButton;
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
public partial class BattleUI : CanvasLayer, IBattleUI
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
    /// スキルボタン1
    /// </summary>
    [Node]
    public ISkillButton SkillButton1 { get; set; } = default!;

    /// <summary>
    /// スキルボタン2
    /// </summary>
    [Node]
    public ISkillButton SkillButton2 { get; set; } = default!;

    /// <summary>
    /// スキルボタン3
    /// </summary>
    [Node]
    public ISkillButton SkillButton3 { get; set; } = default!;

    /// <summary>
    /// スキルボタン4
    /// </summary>
    [Node]
    public ISkillButton SkillButton4 { get; set; } = default!;

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
    /// ステータスアップスキル
    /// </summary>
    public StatusUpSkill StatusUpSkill { get; set; } = new StatusUpSkill();

    /// <summary>
    /// バトルリポジトリ
    /// </summary>
    [Dependency] public IBattleRepo BattleRepo => this.DependOn<IBattleRepo>();


    public void OnReady()
    {
        AddChild(StatusUpSkill);
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

        // 最初のスキルボタンをステータスアップスキルに接続
        SkillButton1.Activated += StatusUpSkill.Activate;
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
