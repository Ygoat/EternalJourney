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

    /// <summary>
    /// デバッグモードでゲームスタートするイベント
    /// </summary>
    public event Menu.StartDebugGameEventHandler StartDebugGame;
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

    /// <summary>
    /// デバッグモードゲームスタートシグナル
    /// </summary>
    [Signal]
    public delegate void StartDebugGameEventHandler();
    #endregion Signals

    #region Nodes
    /// <summary>
    /// スタートゲームボタン
    /// </summary>
    [Node]
    public IButton StartGameButton { get; set; } = default!;

    /// <summary>
    /// デバッグボタン
    /// </summary>
    [Node]
    public IButton DebugButton { get; set; } = default!;
    #endregion Nodes

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void OnReady()
    {
        StartGameButton.Pressed += OnStartGamePressed;
        DebugButton.Pressed += OnDebugButtonPressed;
    }

    /// <summary>
    /// スタートボタン押下イベントファンクション
    /// </summary>
    public void OnStartGamePressed()
    {
        EmitSignal(SignalName.StartGame);
    }

    /// <summary>
    /// デバッグボタン押下イベントファンクション
    /// </summary>
    public void OnDebugButtonPressed()
    {
        EmitSignal(SignalName.StartDebugGame);
    }
}
