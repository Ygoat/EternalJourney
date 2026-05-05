namespace EternalJourney.SelectUI;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;

public interface ISelectSkillUI : IControl
{
}

[Meta(typeof(IAutoNode))]
public partial class SelectSkillUI : Control, ISelectSkillUI
{
    public override void _Notification(int what) => this.Notify(what);

    #region Signals
    #endregion Signals
    #region State
    #endregion State
    #region Exports
    #endregion Exports
    #region PackedScenes
    #endregion PackedScenes
    #region Nodes
    [Node]
    IButton Skill1 { get; set; } = default!;

    [Node]
    IButton Skill2 { get; set; } = default!;

    [Node]
    IButton Skill3 { get; set; } = default!;

    [Node]
    IButton Skill4 { get; set; } = default!;

    [Node]
    IButton Skill5 { get; set; } = default!;

    [Node]
    IButton Skill6 { get; set; } = default!;

    [Node]
    IButton Skill7 { get; set; } = default!;

    [Node]
    IButton Skill8 { get; set; } = default!;


    #endregion Nodes
    #region Provisions
    #endregion Provisions
    #region Dependencies
    #endregion Dependencies

    public void OnInitialized()
    {
    }

    public void OnReady()
    {
    }

    public void OnResolved()
    {
    }

    public void Setup()
    {
    }
}
