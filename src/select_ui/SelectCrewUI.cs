namespace EternalJourney.SelectUI;

using System;
using System.Collections.Generic;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Game.Domain;
using EternalJourney.SelectUI.State;
using Godot;

/// <summary>
/// クルー選択UIインターフェース
/// </summary>
public interface ISelectCrewUI : IControl
{
}

/// <summary>
/// クルー選択UIクラス（候補ボタンの生成と表示、シグナルの中継のみ）
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class SelectCrewUI : Control, ISelectCrewUI
{
    public override void _Notification(int what) => this.Notify(what);

    #region State
    public ISelectCrewUILogic Logic { get; set; } = default!;
    public LogicBlock.Binding Binding { get; set; } = default!;
    #endregion State

    #region Exports
    /// <summary>
    /// 候補ボタンの大きさ
    /// </summary>
    [Export]
    public Vector2 ButtonSize { get; set; } = new(130f, 90f);

    /// <summary>
    /// 候補ボタンの文字サイズ
    /// </summary>
    [Export]
    public int ButtonFontSize { get; set; } = 22;
    #endregion Exports

    #region Nodes
    [Node]
    public IGridContainer CommanderList { get; set; } = default!;

    [Node]
    public IGridContainer WorkerList { get; set; } = default!;

    [Node]
    public IButton ConfirmButton { get; set; } = default!;

    [Node]
    public IRichTextLabel CrewDescription { get; set; } = default!;
    #endregion Nodes

    #region Dependencies
    [Dependency]
    public IGameRepo GameRepo => this.DependOn<IGameRepo>();
    #endregion Dependencies

    private readonly List<Button> _commanderButtons = new();
    private readonly List<Button> _workerButtons = new();

    public void Setup()
    {
        Logic = new SelectCrewUILogic();
        Binding = Logic.Bind();
    }

    public void OnResolved()
    {
        Logic.Set(GameRepo);

        Binding
            .OnOutput((in SelectCrewUILogic.Output.Initialized output) =>
            {
                CreateButtons(CommanderList, _commanderButtons, output.CommanderNames,
                    (i, pressed) => Logic.Input(new SelectCrewUILogic.Input.CommanderToggled(i, pressed)));
                CreateButtons(WorkerList, _workerButtons, output.WorkerNames,
                    (i, pressed) => Logic.Input(new SelectCrewUILogic.Input.WorkerToggled(i, pressed)));
            })
            .OnOutput((in SelectCrewUILogic.Output.SelectionChanged output) =>
            {
                for (int i = 0; i < _commanderButtons.Count; i++)
                {
                    _commanderButtons[i].SetPressedNoSignal(i == output.CommanderIndex);
                }
                for (int i = 0; i < _workerButtons.Count && i < output.WorkerSelected.Count; i++)
                {
                    _workerButtons[i].SetPressedNoSignal(output.WorkerSelected[i]);
                }
                CrewDescription.Text = output.Description;
                ConfirmButton.Disabled = !output.CanConfirm;
            });

        ConfirmButton.Pressed += OnConfirmPressed;
        Logic.Start<SelectCrewUIState.Idle>();
    }

    private void CreateButtons(IGridContainer container, List<Button> buttons, IReadOnlyList<string> labels, Action<int, bool> onToggled)
    {
        buttons.Clear();
        for (int i = 0; i < labels.Count; i++)
        {
            int index = i;
            var button = new Button
            {
                Text = labels[i],
                ToggleMode = true,
                CustomMinimumSize = ButtonSize,
            };
            button.AddThemeFontSizeOverride("font_size", ButtonFontSize);
            // ボタンはこのノードと一緒に破棄されるため、購読解除は不要
            button.Toggled += pressed => onToggled(index, pressed);
            container.AddChild(button);
            buttons.Add(button);
        }
    }

    private void OnConfirmPressed() => Logic.Input(new SelectCrewUILogic.Input.ConfirmPressed());

    public void OnExitTree()
    {
        ConfirmButton.Pressed -= OnConfirmPressed;
        Binding.Dispose();
        ((IDisposable)Logic).Dispose();
    }
}
