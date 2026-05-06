namespace EternalJourney.SelectUI;

using System.Collections.Generic;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Game.Domain;
using EternalJourney.SelectUI.State;
using Godot;

public interface ISelectSkillUI : IControl
{
    public IButton Skill1 { get; }
    public IButton Skill2 { get; }
    public IButton Skill3 { get; }
    public IButton Skill4 { get; }
    public IButton Skill5 { get; }
    public IButton Skill6 { get; }
    public IButton Skill7 { get; }
    public IButton Skill8 { get; }
    public IButton SelectButton { get; }
}

[Meta(typeof(IAutoNode))]
public partial class SelectSkillUI : Control, ISelectSkillUI
{
    public override void _Notification(int what) => this.Notify(what);

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
        Binding = Logic.Bind();
    }

    public void OnResolved()
    {
        Logic.Set(GameRepo);
        Logic.Set<ISelectSkillUI>(this);
        Logic.Set(new HashSet<int>());
        Logic.Start();
    }

    public void OnTreeExiting()
    {
        Binding.Dispose();
        ((System.IDisposable)Logic).Dispose();
    }
}
