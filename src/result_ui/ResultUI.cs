namespace EternalJourney.ResultUI;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;

/// <summary>
/// バトルUIインターフェース
/// </summary>
public interface IResultUI : ICanvasLayer
{
}

/// <summary>
/// バトルUIクラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class ResultUI : CanvasLayer, IResultUI
{
    public override void _Notification(int what) => this.Notify(what);

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

    public void OnReady()
    {

    }

    public void Setup()
    {

    }

    public void OnResolved()
    {
    }

}
