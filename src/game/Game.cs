namespace EternalJourney.Game;

using System.Reflection.Metadata;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.AutoConnectTest;
using EternalJourney.Cores.Models;
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

    [Dependency]
    private ICrewCsvReader crewCsvReader => this.DependOn<ICrewCsvReader>(() => new CrewCsvReader());

    public override void _Ready() { }

    // OnReadyでProvide()を呼び出して依存関係を提供します。
    public void OnReady()
    {
        this.Provide(); // 依存関係の提供を通知
        // AutoConnectTestNode.TestEmit += OnTestEmitConnect;
        return;
    }

    public void OnTestButtonPressed()
    {
        GD.Print(ButtonPresses++);
        Crew? crew = crewCsvReader.Get(e => e.Id == 1);
        GD.Print(crew!.Name);
        return;
    }

    public void OnTestEmitConnect()
    {
        GD.Print("Pressed 10!");
    }
}
