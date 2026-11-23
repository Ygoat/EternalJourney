namespace EternalJourney.ResearchLab;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Battle.Domain;
using EternalJourney.ResearchLab.State;
using Godot;

/// <summary>
/// 研究室インターフェース
/// </summary>
public interface IResearchLab : INode2D
{
    /// <summary>
    /// 研究室をタップした時と同じ処理を行う（入力・テスト用）
    /// </summary>
    void Press();
}

/// <summary>
/// 研究室クラス（船の中央に置き、タップでスクラップを消費して研究を始める）
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class ResearchLab : Node2D, IResearchLab
{
    public override void _Notification(int what) => this.Notify(what);

    #region State
    /// <summary>
    /// 研究室ロジック
    /// </summary>
    public IResearchLabLogic ResearchLabLogic { get; set; } = default!;

    /// <summary>
    /// 研究室ロジックバインド
    /// </summary>
    public LogicBlock.Binding ResearchLabBinding { get; set; } = default!;

    /// <summary>
    /// 描画中の色
    /// </summary>
    private Color _drawColor;

    /// <summary>
    /// 描画中のコスト表記
    /// </summary>
    private string _costText = "";
    #endregion State

    #region Exports
    /// <summary>
    /// タップ判定・描画の半径（px）
    /// </summary>
    [Export]
    public float Radius { get; set; } = 48f;

    /// <summary>
    /// 研究できる時の色
    /// </summary>
    [Export]
    public Color AvailableColor { get; set; } = new Color(0.3f, 1f, 0.7f, 0.9f);

    /// <summary>
    /// スクラップが足りない時の色
    /// </summary>
    [Export]
    public Color UnavailableColor { get; set; } = new Color(0.4f, 0.4f, 0.4f, 0.6f);

    /// <summary>
    /// 文字サイズ
    /// </summary>
    [Export]
    public int FontSize { get; set; } = 16;
    #endregion Exports

    #region Dependencies
    [Dependency]
    public IBattleRepo BattleRepo => this.DependOn<IBattleRepo>();
    #endregion Dependencies

    public void Setup()
    {
        ResearchLabLogic = new ResearchLabLogic();
        ResearchLabBinding = ResearchLabLogic.Bind();
        _drawColor = UnavailableColor;
    }

    public void OnResolved()
    {
        ResearchLabLogic.Set(BattleRepo);
        ResearchLabBinding
            .OnOutput((in ResearchLabLogic.Output.AvailabilityChanged output) =>
            {
                _drawColor = output.CanResearch ? AvailableColor : UnavailableColor;
                _costText = output.CostText;
                QueueRedraw();
            });
        ResearchLabLogic.Start<ResearchLabState.Watching>();
    }

    public override void _Input(InputEvent @event)
    {
        // UIの下に置かれるため、GUIより先に届く _Input で円内のタップを判定する
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left }
            && GetLocalMousePosition().Length() <= Radius)
        {
            Press();
        }
    }

    public void Press() => ResearchLabLogic.Input(new ResearchLabLogic.Input.Pressed());

    public override void _Draw()
    {
        DrawCircle(Vector2.Zero, Radius, new Color(0f, 0f, 0f, 0.5f));
        DrawArc(Vector2.Zero, Radius, 0f, Mathf.Tau, 48, _drawColor, 3f);
        Font font = ThemeDB.FallbackFont;
        DrawString(font, new Vector2(-Radius, FontSize / 2f), _costText,
            HorizontalAlignment.Center, Radius * 2f, FontSize, _drawColor);
    }

    public void OnExitTree()
    {
        ResearchLabBinding.Dispose();
        ((System.IDisposable)ResearchLabLogic).Dispose();
    }
}
