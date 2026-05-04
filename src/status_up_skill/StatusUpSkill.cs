namespace EternalJourney.StatusUpSkill;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Battle.Domain;
using EternalJourney.Cores.Models.Skill;
using EternalJourney.StatusUpSkill.State;
using Godot;

/// <summary>
/// ステータスアップスキルインターフェース
/// </summary>
public interface IStatusUpSkill : INode
{
    /// <summary>
    /// スキルを発動する
    /// </summary>
    void Activate();
}

/// <summary>
/// ステータスアップスキルクラス（ATK・SPDを一定時間バフする）
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class StatusUpSkill : Node, IStatusUpSkill
{
    public override void _Notification(int what) => this.Notify(what);

    /// <summary>
    /// スキルロジック
    /// </summary>
    public StatusUpSkillLogic Logic { get; set; } = default!;

    /// <summary>
    /// スキルバインド
    /// </summary>
    public StatusUpSkillLogic.IBinding Binding { get; set; } = default!;

    /// <summary>
    /// バフ持続タイマー
    /// </summary>
    public Timer BuffTimer { get; set; } = default!;

    /// <summary>
    /// バフ持続時間（秒）
    /// </summary>
    public float BuffDuration { get; set; }

    [Dependency] public IBattleRepo BattleRepo => this.DependOn<IBattleRepo>();

    public void Initialize()
    {
        BuffTimer = new Timer();
        BuffDuration = 10.0f;
    }

    public void OnReady()
    {
        AddChild(BuffTimer);
    }

    public void Setup()
    {
        Logic = new StatusUpSkillLogic();
        Binding = Logic.Bind();
    }

    public void OnResolved()
    {
        BuffTimer.WaitTime = BuffDuration;
        BuffTimer.OneShot = true;
        BuffTimer.Timeout += OnBuffTimerTimeout;

        Binding
            .Handle((in StatusUpSkillLogic.Output.Activated o) =>
            {
                BattleRepo.AtkMultiplier = o.AtkMultiplier;
                BattleRepo.SpdMultiplier = o.SpdMultiplier;
                BattleRepo.ActiveSkillCategory = SkillCategory.StatusUp;
                BattleRepo.ActiveSkillTarget = SkillTarget.Ship | SkillTarget.Weapon | SkillTarget.Bullet;
                // 重ねがけ時はタイマーをリセット
                BuffTimer.Stop();
                BuffTimer.Start();
            })
            .Handle((in StatusUpSkillLogic.Output.Deactivated _) =>
            {
                BattleRepo.AtkMultiplier = 1.0f;
                BattleRepo.SpdMultiplier = 1.0f;
                BattleRepo.ActiveSkillCategory = SkillCategory.None;
                BattleRepo.ActiveSkillTarget = SkillTarget.None;
            });
        Logic.Start();
    }

    /// <summary>
    /// スキルを発動する
    /// </summary>
    public void Activate()
    {
        Logic?.Input(new StatusUpSkillLogic.Input.Apply());
    }

    private void OnBuffTimerTimeout()
    {
        Logic.Input(new StatusUpSkillLogic.Input.Remove());
    }
}
