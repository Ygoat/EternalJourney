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
/// 武器選択UIインターフェース
/// </summary>
public interface ISelectWeaponUI : IControl
{
}

/// <summary>
/// 武器選択UIクラス（銃座ごとの選択欄の生成と表示、シグナルの中継のみ）
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class SelectWeaponUI : Control, ISelectWeaponUI
{
    public override void _Notification(int what) => this.Notify(what);

    #region State
    public ISelectWeaponUILogic Logic { get; set; } = default!;
    public LogicBlock.Binding Binding { get; set; } = default!;
    #endregion State

    #region Exports
    /// <summary>
    /// 銃座名・武器名の文字サイズ
    /// </summary>
    [Export]
    public int FontSize { get; set; } = 30;
    #endregion Exports

    #region Nodes
    [Node]
    public IGridContainer SlotList { get; set; } = default!;

    [Node]
    public IButton RandomButton { get; set; } = default!;

    [Node]
    public IButton ConfirmButton { get; set; } = default!;

    [Node]
    public IRichTextLabel WeaponDescription { get; set; } = default!;
    #endregion Nodes

    #region Dependencies
    [Dependency]
    public IGameRepo GameRepo => this.DependOn<IGameRepo>();
    #endregion Dependencies

    private readonly List<OptionButton> _options = new();

    public void Setup()
    {
        Logic = new SelectWeaponUILogic();
        Binding = Logic.Bind();
    }

    public void OnResolved()
    {
        Logic.Set(GameRepo);
        Logic.Set(new Random());

        Binding
            .OnOutput((in SelectWeaponUILogic.Output.Initialized output) => CreateSlots(output.SlotNames, output.WeaponNames))
            .OnOutput((in SelectWeaponUILogic.Output.SelectionChanged output) =>
            {
                for (int i = 0; i < _options.Count && i < output.Selected.Count; i++)
                {
                    _options[i].Select(output.Selected[i]);
                }
                WeaponDescription.Text = output.Description;
            });

        RandomButton.Pressed += OnRandomPressed;
        ConfirmButton.Pressed += OnConfirmPressed;
        Logic.Start<SelectWeaponUIState.Idle>();
    }

    private void CreateSlots(IReadOnlyList<string> slotNames, IReadOnlyList<string> weaponNames)
    {
        _options.Clear();
        for (int slot = 0; slot < slotNames.Count; slot++)
        {
            int index = slot;
            var label = new Label { Text = slotNames[slot] };
            label.AddThemeFontSizeOverride("font_size", FontSize);
            var option = new OptionButton { CustomMinimumSize = new Vector2(320f, 60f) };
            option.AddThemeFontSizeOverride("font_size", FontSize);
            foreach (string name in weaponNames)
            {
                option.AddItem(name);
            }
            // 選択欄はこのノードと一緒に破棄されるため、購読解除は不要
            option.ItemSelected += selected => Logic.Input(new SelectWeaponUILogic.Input.WeaponChosen(index, (int)selected));
            SlotList.AddChild(label);
            SlotList.AddChild(option);
            _options.Add(option);
        }
    }

    private void OnRandomPressed() => Logic.Input(new SelectWeaponUILogic.Input.RandomPressed());

    private void OnConfirmPressed() => Logic.Input(new SelectWeaponUILogic.Input.ConfirmPressed());

    public void OnExitTree()
    {
        RandomButton.Pressed -= OnRandomPressed;
        ConfirmButton.Pressed -= OnConfirmPressed;
        Binding.Dispose();
        ((IDisposable)Logic).Dispose();
    }
}
