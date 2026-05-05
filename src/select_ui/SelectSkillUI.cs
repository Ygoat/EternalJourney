namespace EternalJourney.SelectUI;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Game.Domain;
using EternalJourney.SelectUI.State;
using Godot;

public interface ISelectSkillUI : IControl
{
    /// <summary>
    /// スキル選択完了シグナル
    /// </summary>
    public event SelectSkillUI.SelectedEventHandler Selected;
}

[Meta(typeof(IAutoNode))]
public partial class SelectSkillUI : Control, ISelectSkillUI
{
    public override void _Notification(int what) => this.Notify(what);

    #region Signals
    /// <summary>
    /// スキル選択完了シグナル
    /// </summary>
    [Signal]
    public delegate void SelectedEventHandler();
    #endregion Signals
    #region State
    public SelectSkillUILogic Logic { get; set; } = default!;
    public SelectSkillUILogic.IBinding Binding { get; set; } = default!;
    #endregion State
    #region Nodes
    [Node]
    public IButton Skill1 { get; set; } = default!;

    [Node]
    public IButton Skill2 { get; set; } = default!;

    [Node]
    public IButton Skill3 { get; set; } = default!;

    [Node]
    public IButton Skill4 { get; set; } = default!;

    [Node]
    public IButton Skill5 { get; set; } = default!;

    [Node]
    public IButton Skill6 { get; set; } = default!;

    [Node]
    public IButton Skill7 { get; set; } = default!;

    [Node]
    public IButton Skill8 { get; set; } = default!;

    [Node]
    public IButton SelectButton { get; set; } = default!;

    #endregion Nodes
    #region Dependencies
    [Dependency]
    public IGameRepo GameRepo => this.DependOn<IGameRepo>();
    #endregion Dependencies

    public void Setup()
    {
        Logic = new SelectSkillUILogic();
        Logic.Set(this as ISelectSkillUI);
        Binding = Logic.Bind();
    }

    public void OnReady()
    {
        SelectButton.Pressed += () => EmitSignal(SignalName.Selected);
    }

    public void OnResolved()
    {
        Logic.Set(GameRepo);
        Logic.Start();
    }

    public void OnTreeExiting()
    {
        Binding.Dispose();
        ((System.IDisposable)Logic).Dispose();
    }
}
