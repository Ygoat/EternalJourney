namespace EternalJourney.BattleUI;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Battle.Domain;
using EternalJourney.BattleUI.State;
using EternalJourney.Game.Domain;
using EternalJourney.HealSkill;
using EternalJourney.StatusUpSkill;
using EternalJourney.SukillButton;
using Godot;

/// <summary>
/// バトルUIインターフェース
/// </summary>
public interface IBattleUI : IControl
{
    public float Count { get; }
    public ISkillButton SkillButton1 { get; }
    public ISkillButton SkillButton2 { get; }
    public ISkillButton SkillButton3 { get; }
    public ISkillButton SkillButton4 { get; }
    public IStatusUpSkill StatusUpSkill { get; }
    public IHealSkill HealSkill { get; }
}

/// <summary>
/// バトルUIクラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class BattleUI : Control, IBattleUI
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
    /// 残存HPゲージ
    /// </summary>
    [Node]
    public IColorRect LeftHPGauge { get; set; } = default!;

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
    public IStatusUpSkill StatusUpSkill { get; set; } = new StatusUpSkill();

    /// <summary>
    /// 回復スキル
    /// </summary>
    public IHealSkill HealSkill { get; set; } = new HealSkill();

    /// <summary>
    /// バトルリポジトリ
    /// </summary>
    [Dependency] public IBattleRepo BattleRepo => this.DependOn<IBattleRepo>();

    /// <summary>
    /// ゲームリポジトリ
    /// </summary>
    [Dependency] public IGameRepo GameRepo => this.DependOn<IGameRepo>();


    public void OnReady()
    {
        ZIndex = 100;
        AddChild((Node)StatusUpSkill);
        AddChild((Node)HealSkill);
    }

    public void Setup()
    {
        TimerLabel.Text = "Timer";
        ScoreLabel.Text = "Score";
        SetPhysicsProcess(false);
        BattleUILogic = new BattleUILogic();
    }

    public void OnResolved()
    {
        BattleUILogic.Set(BattleRepo);
        BattleUILogic.Set(GameRepo);
        BattleUILogic.Set<IBattleUI>(this);
        BattleUIBinding = BattleUILogic.Bind();
        BattleUIBinding
            .Handle((in BattleUILogic.Output.ScoreChanged output) =>
            {
                SetScoreLabel(output.CurrentScore);
            })
            .Handle((in BattleUILogic.Output.ShipHpChanged output) =>
            {
                UpdateHpGauge(output.CurrentHp, output.MaxHp);
            })
            .Handle((in BattleUILogic.Output.GameOver _) =>
            {
                OnGameOver();
            })
            .Handle((in BattleUILogic.Output.ActivateBattleUI _) =>
            {
                Count = 0;
                SetPhysicsProcess(true);
            })
            .Handle((in BattleUILogic.Output.TikCount _) =>
            {
                Count++;
                TimerLabel.Text = $"Time: {Count}";
            });
        BattleUILogic.Start();
    }

    private void OnGameOver()
    {
        SetPhysicsProcess(false);
    }

    public void OnTreeExiting()
    {
        BattleUIBinding.Dispose();
        ((System.IDisposable)BattleUILogic).Dispose();
    }

    public void SetScoreLabel(int score)
    {
        ScoreLabel.Text = $"Score: {score}";
    }

    public void UpdateHpGauge(float currentHp, float maxHp)
    {
        float ratio = maxHp > 0f ? currentHp / maxHp : 0f;
        LeftHPGauge.AnchorRight = ratio;
    }

    public void OnPhysicsProcess(double delta)
    {
        BattleUILogic.Input(new BattleUILogic.Input.PhysicsProcess());
    }
}
