namespace EternalJourney.SelectUI;

using System;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Game.Domain;
using EternalJourney.SelectUI.State;
using Godot;

/// <summary>
/// スキル選択UIインターフェース
/// </summary>
public interface ISelectSkillUI : IControl
{
}

/// <summary>
/// スキル選択UIクラス（表示とシグナルの中継のみ。選択の判断は SelectSkillUILogic が行う）
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class SelectSkillUI : Control, ISelectSkillUI
{
    public override void _Notification(int what) => this.Notify(what);

    #region State
    public ISelectSkillUILogic Logic { get; set; } = default!;
    public LogicBlock.Binding Binding { get; set; } = default!;
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

    [Node]
    public IButton RandomButton { get; set; } = default!;

    [Node]
    public IRichTextLabel SkillDescription { get; set; } = default!;
    #endregion Nodes

    #region Dependencies
    [Dependency]
    public IGameRepo GameRepo => this.DependOn<IGameRepo>();
    #endregion Dependencies

    /// <summary>
    /// ボタンごとのトグルハンドラ（解除用に保持する）
    /// </summary>
    private BaseButton.ToggledEventHandler[] _toggleHandlers = Array.Empty<BaseButton.ToggledEventHandler>();

    private IButton[] Buttons => new[] { Skill1, Skill2, Skill3, Skill4, Skill5, Skill6, Skill7, Skill8 };

    public void Setup()
    {
        Logic = new SelectSkillUILogic();
        Binding = Logic.Bind();
    }

    public void OnResolved()
    {
        Logic.Set(GameRepo);
        Logic.Set(new Random());

        Binding
            .OnOutput((in SelectSkillUILogic.Output.Initialized output) =>
            {
                IButton[] buttons = Buttons;
                for (int i = 0; i < buttons.Length; i++)
                {
                    buttons[i].ToggleMode = true;
                    buttons[i].Text = i < output.Names.Count ? output.Names[i] : "";
                }
            })
            .OnOutput((in SelectSkillUILogic.Output.SelectionChanged output) =>
            {
                IButton[] buttons = Buttons;
                for (int i = 0; i < buttons.Length && i < output.Selected.Count; i++)
                {
                    buttons[i].SetPressedNoSignal(output.Selected[i]);
                }
                SkillDescription.Text = output.Description;
            });

        IButton[] targets = Buttons;
        _toggleHandlers = new BaseButton.ToggledEventHandler[targets.Length];
        for (int i = 0; i < targets.Length; i++)
        {
            int index = i;
            _toggleHandlers[i] = pressed => Logic.Input(new SelectSkillUILogic.Input.SkillToggled(index, pressed));
            targets[i].Toggled += _toggleHandlers[i];
        }
        SelectButton.Pressed += OnSelectPressed;
        RandomButton.Pressed += OnRandomPressed;

        Logic.Start<SelectSkillUIState.Idle>();
    }

    private void OnSelectPressed() => Logic.Input(new SelectSkillUILogic.Input.SelectButtonPressed());

    private void OnRandomPressed() => Logic.Input(new SelectSkillUILogic.Input.RandomPressed());

    public void OnExitTree()
    {
        IButton[] targets = Buttons;
        for (int i = 0; i < targets.Length && i < _toggleHandlers.Length; i++)
        {
            targets[i].Toggled -= _toggleHandlers[i];
        }
        SelectButton.Pressed -= OnSelectPressed;
        RandomButton.Pressed -= OnRandomPressed;
        Binding.Dispose();
        ((IDisposable)Logic).Dispose();
    }
}
