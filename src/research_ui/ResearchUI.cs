namespace EternalJourney.ResearchUI;

using System;
using System.Collections.Generic;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Battle.Domain;
using EternalJourney.ResearchUI.State;
using Godot;

/// <summary>
/// 研究UIインターフェース
/// </summary>
public interface IResearchUI : ICanvasLayer
{
    /// <summary>
    /// カードを選択する（ボタン押下・テスト用）
    /// </summary>
    void SelectCard(int index);
}

/// <summary>
/// 研究UIクラス（研究の演出・カード提示・選択。表示中はバトルを一時停止する）
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class ResearchUI : CanvasLayer, IResearchUI
{
    public override void _Notification(int what) => this.Notify(what);

    #region State
    /// <summary>
    /// 研究UIロジック
    /// </summary>
    public IResearchUILogic ResearchUILogic { get; set; } = default!;

    /// <summary>
    /// 研究UIロジックバインド
    /// </summary>
    public LogicBlock.Binding ResearchUIBinding { get; set; } = default!;
    #endregion State

    #region Exports
    /// <summary>
    /// 演出の長さ（秒）
    /// </summary>
    [Export]
    public float Duration { get; set; } = 3f;

    /// <summary>
    /// 提示するカード枚数
    /// </summary>
    [Export]
    public int CardCount { get; set; } = 5;

    /// <summary>
    /// 演出の回転速度（ラジアン/秒）
    /// </summary>
    [Export]
    public float SpinSpeed { get; set; } = 6f;

    /// <summary>
    /// カードの大きさ
    /// </summary>
    [Export]
    public Vector2 CardSize { get; set; } = new(130f, 240f);

    /// <summary>
    /// カード名の文字サイズ
    /// </summary>
    [Export]
    public int CardFontSize { get; set; } = 18;

    /// <summary>
    /// カード枠の太さ（px）
    /// </summary>
    [Export]
    public int CardBorderWidth { get; set; } = 4;
    #endregion Exports

    #region Nodes
    [Node]
    public IControl Root { get; set; } = default!;

    [Node]
    public ILabel TitleLabel { get; set; } = default!;

    [Node]
    public IControl Spinner { get; set; } = default!;

    [Node]
    public IProgressBar ProgressBar { get; set; } = default!;

    [Node]
    public IHBoxContainer CardList { get; set; } = default!;
    #endregion Nodes

    #region Dependencies
    [Dependency]
    public IBattleRepo BattleRepo => this.DependOn<IBattleRepo>();
    #endregion Dependencies

    public void Setup()
    {
        ResearchUILogic = new ResearchUILogic();
        ResearchUIBinding = ResearchUILogic.Bind();
        // 一時停止中も動けるようにする（バトル本体だけを止める）
        ProcessMode = ProcessModeEnum.Always;
        Root.Visible = false;
        SetProcess(false);
    }

    public void OnResolved()
    {
        ResearchUILogic.Set(BattleRepo);
        ResearchUILogic.Set(new ResearchUILogic.Settings(Duration, CardCount));
        ResearchUILogic.Set(new Random());

        ResearchUIBinding
            .OnOutput((in ResearchUILogic.Output.Opened output) =>
            {
                GetTree().Paused = true;
                ClearCards();
                Root.Visible = true;
                TitleLabel.Text = output.Title;
                Spinner.Visible = true;
                ProgressBar.Visible = true;
                ProgressBar.Value = 0;
                SetProcess(true);
            })
            .OnOutput((in ResearchUILogic.Output.Progress output) =>
            {
                ProgressBar.Value = output.Ratio;
            })
            .OnOutput((in ResearchUILogic.Output.CardsShown output) =>
            {
                SetProcess(false);
                TitleLabel.Text = output.Title;
                Spinner.Visible = false;
                ProgressBar.Visible = false;
                ShowCards(output.Cards);
            })
            .OnOutput((in ResearchUILogic.Output.Closed _) =>
            {
                SetProcess(false);
                ClearCards();
                Root.Visible = false;
                if (IsInsideTree())
                {
                    GetTree().Paused = false;
                }
            });

        ResearchUILogic.Start<ResearchUIState.Hidden>();
    }

    public void OnProcess(double delta)
    {
        Spinner.Rotation += SpinSpeed * (float)delta;
        ResearchUILogic.Input(new ResearchUILogic.Input.Process(delta));
    }

    public void SelectCard(int index) => ResearchUILogic.Input(new ResearchUILogic.Input.CardSelected(index));

    /// <summary>
    /// カードを並べる
    /// </summary>
    private void ShowCards(IReadOnlyList<ResearchUILogic.CardView> cards)
    {
        ClearCards();
        for (int i = 0; i < cards.Count; i++)
        {
            int index = i;
            Button button = CreateCardButton(cards[i]);
            button.Pressed += () => SelectCard(index);
            CardList.AddChild(button);
        }
    }

    /// <summary>
    /// カード1枚分のボタンを作る（表示内容は Logic から受け取り、レアリティ色の枠を当てる）
    /// </summary>
    private Button CreateCardButton(ResearchUILogic.CardView view)
    {
        Color color = ResearchCardStyle.RarityColor(view.Rarity);

        var button = new Button
        {
            CustomMinimumSize = CardSize,
            Text = view.Text,
            AutowrapMode = TextServer.AutowrapMode.Arbitrary,
            ProcessMode = ProcessModeEnum.Always,
        };
        button.AddThemeFontSizeOverride("font_size", CardFontSize);
        button.AddThemeColorOverride("font_color", color);
        button.AddThemeColorOverride("font_hover_color", Colors.White);
        button.AddThemeStyleboxOverride("normal", CreateCardStyle(color, 0.85f));
        button.AddThemeStyleboxOverride("hover", CreateCardStyle(color, 1f));
        button.AddThemeStyleboxOverride("pressed", CreateCardStyle(color, 1f));
        return button;
    }

    private StyleBoxFlat CreateCardStyle(Color border, float alpha)
    {
        var style = new StyleBoxFlat
        {
            BgColor = new Color(0.08f, 0.08f, 0.12f, alpha),
            BorderColor = border,
        };
        style.SetBorderWidthAll(CardBorderWidth);
        style.SetCornerRadiusAll(8);
        return style;
    }

    private void ClearCards()
    {
        foreach (Node child in CardList.GetChildren())
        {
            CardList.RemoveChild(child);
            child.QueueFree();
        }
    }

    public void OnExitTree()
    {
        // 研究中にツリーから外れても一時停止を残さない
        if (GetTree() is SceneTree tree)
        {
            tree.Paused = false;
        }
        ResearchUIBinding.Dispose();
        ((IDisposable)ResearchUILogic).Dispose();
    }
}
