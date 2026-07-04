namespace EternalJourney.BattleUI;

using System.Collections.Generic;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Battle.Domain;
using EternalJourney.BattleUI.State;
using EternalJourney.Cores.Models.Skill;
using EternalJourney.Game.Domain;
using EternalJourney.Skills;
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
    ISkillNode GetSkill(SkillType type);
}

/// <summary>
/// バトルUIクラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class BattleUI : Control, IBattleUI
{
    public override void _Notification(int what) => this.Notify(what);

    /// <summary>タイマーラベル</summary>
    [Node]
    public ILabel TimerLabel { get; set; } = default!;

    /// <summary>スコアラベル</summary>
    [Node]
    public ILabel ScoreLabel { get; set; } = default!;

    /// <summary>スキルボタン1</summary>
    [Node]
    public ISkillButton SkillButton1 { get; set; } = default!;

    /// <summary>スキルボタン2</summary>
    [Node]
    public ISkillButton SkillButton2 { get; set; } = default!;

    /// <summary>スキルボタン3</summary>
    [Node]
    public ISkillButton SkillButton3 { get; set; } = default!;

    /// <summary>スキルボタン4</summary>
    [Node]
    public ISkillButton SkillButton4 { get; set; } = default!;

    /// <summary>残存HPゲージ</summary>
    [Node]
    public IColorRect LeftHPGauge { get; set; } = default!;

    /// <summary>
    /// 残存SPゲージ
    /// </summary>
    [Node]
    public IColorRect LeftSPGauge { get; set; } = default!;

    /// <summary>
    /// SPボタン（SPが一定以上で押せるようになる）
    /// </summary>
    [Node]
    public IButton SPButton { get; set; } = default!;

    /// <summary>バトルUIロジック</summary>
    public IBattleUILogic BattleUILogic { get; set; } = default!;

    /// <summary>バトルUIバインド</summary>
    public BattleUILogic.IBinding BattleUIBinding { get; set; } = default!;

    public float Count { get; set; } = default!;

    private Dictionary<SkillType, ISkillNode> _skills = default!;

    public ISkillNode GetSkill(SkillType type) => _skills[type];

    /// <summary>バトルリポジトリ</summary>
    [Dependency] public IBattleRepo BattleRepo => this.DependOn<IBattleRepo>();

    /// <summary>ゲームリポジトリ</summary>
    [Dependency] public IGameRepo GameRepo => this.DependOn<IGameRepo>();

    public void Initialize()
    {
        _skills = new Dictionary<SkillType, ISkillNode>();
        foreach (var type in SkillRegistry.SelectableSkills)
            _skills[type] = SkillRegistry.CreateNode(type);
        _skills[SkillType.Heal] = SkillRegistry.CreateNode(SkillType.Heal);
        _skills[SkillType.Regen] = SkillRegistry.CreateNode(SkillType.Regen);
    }

    public void OnReady()
    {
        ZIndex = 100;
        foreach (var skill in _skills.Values)
            AddChild((Node)skill);
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
                LeftHPGauge.AnchorRight = output.Ratio;
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
            })
            .Handle((in BattleUILogic.Output.SpPercentChanged output) =>
            {
                LeftSPGauge.AnchorRight = output.Ratio;
                SPButton.Disabled = output.Ratio < 1.0f;
            });
        SPButton.Pressed += OnSPButtonPressed;
        SPButton.Disabled = true;
        BattleUILogic.Start();
    }

    public void OnPhysicsProcess(double delta)
    {
        BattleUILogic.Input(new BattleUILogic.Input.PhysicsProcess());
    }

    public void SetScoreLabel(int score)
    {
        ScoreLabel.Text = $"Score: {score}";
    }

    private void OnSPButtonPressed() =>
        BattleUILogic.Input(new BattleUILogic.Input.SPButtonPressed());

    private void OnGameOver()
    {
        SetPhysicsProcess(false);
    }

    public void OnTreeExiting()
    {
        BattleUIBinding.Dispose();
        ((System.IDisposable)BattleUILogic).Dispose();
    }
}
