namespace EternalJourney;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;

/// <summary>
/// Menuのインターフェース
/// </summary>
public interface IMenu : IControl
{
    /// <summary>
    /// スタートゲームイベント
    /// </summary>
    event Menu.StartGameEventHandler StartGame;
}

[Meta(typeof(IAutoNode))]
public partial class Menu : Control, IMenu
{
    public override void _Notification(int what) => this.Notify(what);

    /// <summary>
    /// スタートゲームボタン
    /// </summary>
    [Node]
    public IButton StartGameButton { get; set; } = default!;

    /// <summary>
    /// スタートゲームイベントシグナル
    /// </summary>
    [Signal]
    public delegate void StartGameEventHandler();

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void OnReady()
    {
        // ボタン押下時の発火イベントを設定
        StartGameButton.Pressed += OnStartGamePressed;
    }

    /// <summary>
    /// スタートボタン押下時のイベントに設定するメソッド
    /// </summary>
    public void OnStartGamePressed()
    {
        // スタートゲームイベントシグナルを出力
        GD.Print(SignalName.StartGame);
        EmitSignal(SignalName.StartGame);
    }

    // // Called when the node enters the scene tree for the first time.
    // public override void _Ready()
    // {
    // }

    // // Called every frame. 'delta' is the elapsed time since the previous frame.
    // public override void _Process(double delta)
    // {
    // }
}
