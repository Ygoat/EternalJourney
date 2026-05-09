namespace EternalJourney.ResultUI;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.App.Domain;
using EternalJourney.Battle.Domain;
using EternalJourney.Game.Domain;
using EternalJourney.ResultUI.State;
using Godot;

/// <summary>
/// リザルトUIインターフェース
/// </summary>
public interface IResultUI : IControl
{
    /// <summary>
    /// EndGameボタン
    /// </summary>
    public IButton EndButton { get; set; }
}

/// <summary>
/// リザルトUIクラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class ResultUI : Control, IResultUI
{
    public override void _Notification(int what) => this.Notify(what);

    #region State
    /// <summary>
    /// リザルトUIロジック
    /// </summary>
    public IResultUILogic ResultUILogic { get; set; } = default!;

    /// <summary>
    /// リザルトUIバインド
    /// </summary>
    public ResultUILogic.IBinding ResultUIBinding { get; set; } = default!;
    #endregion State

    #region Nodes
    /// <summary>
    /// スコアラベル
    /// </summary>
    [Node]
    public ILabel ScoreLabel { get; set; } = default!;

    /// <summary>
    /// タイマーラベル
    /// </summary>
    [Node]
    public ILabel TimerLabel { get; set; } = default!;

    /// <summary>
    /// EndGameボタン
    /// </summary>
    [Node]
    public IButton EndButton { get; set; } = default!;
    #endregion Nodes

    #region Dependencies
    /// <summary>
    /// アプリケーションリポジトリ
    /// </summary>
    [Dependency]
    public IAppRepo AppRepo => this.DependOn<IAppRepo>();

    /// <summary>
    /// ゲームリポジトリ
    /// </summary>
    [Dependency]
    public IGameRepo GameRepo => this.DependOn<IGameRepo>();

    #endregion Dependencies

    public void Setup()
    {
        ResultUILogic = new ResultUILogic();
        ResultUILogic.Set(this as IResultUI);
        ResultUIBinding = ResultUILogic.Bind();
    }

    public void OnResolved()
    {
        ResultUILogic.Set(AppRepo);
        ResultUILogic.Set(GameRepo);
        ResultUIBinding
            .Handle((in ResultUILogic.Output.UpdateDisplay o) =>
            {
                ScoreLabel.Text = $"Score: {o.Score}";
                TimerLabel.Text = $"Time: {o.Time:F0}";
            });
        ResultUILogic.Start();
    }

    public void OnTreeExiting()
    {
        ResultUIBinding.Dispose();
        ((System.IDisposable)ResultUILogic).Dispose();
    }
}
