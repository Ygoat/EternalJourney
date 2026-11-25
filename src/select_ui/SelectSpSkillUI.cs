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
/// SPスキル選択UIインターフェース
/// </summary>
public interface ISelectSpSkillUI : IControl
{
}

/// <summary>
/// SPスキル選択UIクラス（表示とシグナルの中継のみ）
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class SelectSpSkillUI : Control, ISelectSpSkillUI
{
    public override void _Notification(int what) => this.Notify(what);

    #region State
    public ISelectSpSkillUILogic Logic { get; set; } = default!;
    public LogicBlock.Binding Binding { get; set; } = default!;
    #endregion State

    #region Nodes
    [Node]
    public IButton SpSkill1 { get; set; } = default!;

    [Node]
    public IButton SpSkill2 { get; set; } = default!;

    [Node]
    public IButton SpSkill3 { get; set; } = default!;

    [Node]
    public IButton SpSkill4 { get; set; } = default!;

    [Node]
    public IButton ConfirmButton { get; set; } = default!;

    [Node]
    public IButton RandomButton { get; set; } = default!;

    [Node]
    public IRichTextLabel SpSkillDescription { get; set; } = default!;
    #endregion Nodes

    #region Dependencies
    [Dependency]
    public IGameRepo GameRepo => this.DependOn<IGameRepo>();
    #endregion Dependencies

    /// <summary>
    /// ボタンごとのトグルハンドラ（解除用に保持する）
    /// </summary>
    private BaseButton.ToggledEventHandler[] _toggleHandlers = Array.Empty<BaseButton.ToggledEventHandler>();

    private IButton[] Buttons => new[] { SpSkill1, SpSkill2, SpSkill3, SpSkill4 };

    public void Setup()
    {
        Logic = new SelectSpSkillUILogic();
        Binding = Logic.Bind();
    }

    public void OnResolved()
    {
        Logic.Set(GameRepo);
        Logic.Set(new Random());

        Binding
            .OnOutput((in SelectSpSkillUILogic.Output.Initialized output) =>
            {
                IButton[] buttons = Buttons;
                for (int i = 0; i < buttons.Length; i++)
                {
                    buttons[i].ToggleMode = true;
                    buttons[i].Text = i < output.Names.Count ? output.Names[i] : "";
                    buttons[i].Disabled = i >= output.Names.Count;
                }
            })
            .OnOutput((in SelectSpSkillUILogic.Output.SelectionChanged output) =>
            {
                IButton[] buttons = Buttons;
                for (int i = 0; i < buttons.Length; i++)
                {
                    buttons[i].SetPressedNoSignal(i == output.SelectedIndex);
                }
                SpSkillDescription.Text = output.Description;
                ConfirmButton.Disabled = !output.CanConfirm;
            });

        IButton[] targets = Buttons;
        _toggleHandlers = new BaseButton.ToggledEventHandler[targets.Length];
        for (int i = 0; i < targets.Length; i++)
        {
            int index = i;
            _toggleHandlers[i] = pressed => Logic.Input(new SelectSpSkillUILogic.Input.SpSkillToggled(index, pressed));
            targets[i].Toggled += _toggleHandlers[i];
        }
        ConfirmButton.Pressed += OnConfirmPressed;
        RandomButton.Pressed += OnRandomPressed;

        Logic.Start<SelectSpSkillUIState.Idle>();
    }

    private void OnConfirmPressed() => Logic.Input(new SelectSpSkillUILogic.Input.ConfirmPressed());

    private void OnRandomPressed() => Logic.Input(new SelectSpSkillUILogic.Input.RandomPressed());

    public void OnExitTree()
    {
        IButton[] targets = Buttons;
        for (int i = 0; i < targets.Length && i < _toggleHandlers.Length; i++)
        {
            targets[i].Toggled -= _toggleHandlers[i];
        }
        ConfirmButton.Pressed -= OnConfirmPressed;
        RandomButton.Pressed -= OnRandomPressed;
        Binding.Dispose();
        ((IDisposable)Logic).Dispose();
    }
}
