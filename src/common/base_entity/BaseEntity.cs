namespace EternalJourney.Common.BaseEntity;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.App.Domain;
using EternalJourney.Common.Traits;
using Godot;

/// <summary>
/// ベースエンティティインターフェース
/// </summary>
public interface IBaseEntity : IArea2D
{
    public Status Status { get; set; }
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
