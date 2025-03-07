namespace EternalJourney.Game;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.AutoConnectTest;
using EternalJourney.Cores.Repositories;
using Godot;

/// <summary>
/// ゲームインターフェース
/// </summary>
public interface IGame : INode { }

/// <summary>
/// ゲームクラス
/// </summary>
[Meta(typeof(IAutoNode))] // 子Nodeへ親Nodeの値をDIするために必要なミックスイン
public partial class Game : Node, IProvide<string>, IGame
{
    // 子Nodeへ親Nodeの値をDIするために必要
    public override void _Notification(int what) => this.Notify(what);

    string IProvide<string>.Value() => "Hello from Provider!";

    public Button TestButton { get; private set; } = default!;
    public int ButtonPresses { get; private set; }

    [Node]
    public IAutoConnectTestNode AutoConnectTestNode { get; set; } = default!;

    [Dependency]
    private ICrewCsvReader crewCsvReader => this.DependOn<ICrewCsvReader>();

    public override void _Ready()
      => TestButton = GetNode<Button>("%TestButton");

    // OnReadyでProvide()を呼び出して依存関係を提供します。
    public void OnReady()
    {
        this.Provide(); // 依存関係の提供を通知
        AutoConnectTestNode.TestEmit += OnTestEmitConnect;
        return;
    }

    public void OnTestButtonPressed()
    {
        GD.Print(ButtonPresses++);
        GD.Print(crewCsvReader.Get(e => e.Id == 1).Name);
        return;
    }

    public void OnTestEmitConnect()
    {
        GD.Print("Pressed 10!");
    }
}
