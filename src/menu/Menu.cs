namespace EternalJourney;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;

/// <summary>
/// メニューインターフェース
/// </summary>
public interface IMenu : IControl
{
    /// <summary>
    /// スタートゲームイベント
    /// </summary>
    public event Menu.StartGameEventHandler StartGame;
}

/// <summary>
/// メニュークラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class Menu : Control, IMenu
{
    public override void _Notification(int what) => this.Notify(what);

    #region Signals
    /// <summary>
    /// スタートゲームイベントシグナル
    /// </summary>
    [Signal]
    public delegate void StartGameEventHandler();
    #endregion Signals

    #region Nodes
    /// <summary>
    /// スタートゲームボタン
    /// </summary>
    [Node]
    public IButton StartGameButton { get; set; } = default!;
    #endregion Nodes

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void OnReady()
    {
        // スタートボタン押下イベント
        StartGameButton.Pressed += OnStartGamePressed;
    }

    /// <summary>
    /// スタートボタン押下イベントファンクション
    /// </summary>
    public void OnStartGamePressed()
    {
        // スタートゲームイベントシグナルを出力
        EmitSignal(SignalName.StartGame);
    }
}
