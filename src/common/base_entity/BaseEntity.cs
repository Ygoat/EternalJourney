namespace EternalJourney.Common.BaseEntity;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.App.Domain;
using EternalJourney.Common.Traits;
using EternalJourney.Cores.Models.Skill;
using Godot;

/// <summary>
/// ベースエンティティインターフェース
/// </summary>
public interface IBaseEntity : IArea2D
{
    public Status Status { get; set; }

    /// <summary>
    /// プレイヤー所有かどうか
    /// </summary>
    public bool IsPlayerOwned { get; set; }

    /// <summary>
    /// 指定スキルターゲットのスキル効果を適用すべきか判定する
    /// </summary>
    public bool ShouldApplySkillEffect(SkillTarget activeTarget, SkillTarget requiredTarget);
}

/// <summary>
/// ベースエンティティクラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class BaseEntity : Area2D, IBaseEntity
{
    public override void _Notification(int what) => this.Notify(what);

    /// <summary>
    /// ステータス
    /// </summary>
    [Export]
    public Status Status { get; set; } = new Status();

    /// <summary>
    /// プレイヤー所有かどうか（デフォルト: true）
    /// </summary>
    public bool IsPlayerOwned { get; set; } = true;

    /// <summary>
    /// 自機所有かつスキルターゲットが一致する場合のみスキル効果を適用する
    /// </summary>
    public bool ShouldApplySkillEffect(SkillTarget activeTarget, SkillTarget requiredTarget) =>
        IsPlayerOwned && activeTarget.HasFlag(requiredTarget);

    /// <summary>
    /// デバッグ情報表示用のラベル
    /// </summary>
    [Node]
    public ILabel DebugLabel { get; set; } = default!;

    [Dependency]
    public IAppRepo AppRepo => this.DependOn<IAppRepo>();

    public virtual void OnReady()
    {
    }

    public virtual void Setup()
    {
    }

    public virtual void OnResolved()
    {
        DebugLabel.Visible = false;
        if (AppRepo.IsDebugMode)
        {
            DebugLabel.Visible = true;
        }
        DebugLabel.TopLevel = true;
    }

    public virtual void OnPhysicsProcess(double delta)
    {
        DebugLabel.Position = GlobalPosition;
        DebugLabel.Text =
        $"MaxDur:{Status.MaxDur} Dur:{Status.CurrentDur:F1}\n" +
        $"Atk:{Status.Atk} Spd:{Status.Spd}\n" +
        $"Def:{Status.Def} Size:{Status.Size}";
    }
}
