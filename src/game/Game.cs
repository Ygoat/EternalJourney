namespace EternalJourney.Game;

using Chickensoft.AutoInject;
using Chickensoft.Collections;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Cores.Repositories;
using Godot;


/// <summary>
/// ゲームインターフェース
/// </summary>
public interface IGame : INode, IProvide<EntityTable<int>> { }

/// <summary>
/// ゲームクラス
/// </summary>
[Meta(typeof(IAutoNode))] // 子Nodeへ親Nodeの値をDIするために必要なミックスイン
public partial class Game : Node, IProvide<string>, IGame
{
    // 子Nodeへ親Nodeの値をDIするために必要
    public override void _Notification(int what) => this.Notify(what);


    public Button TestButton { get; private set; } = default!;
    public int ButtonPresses { get; private set; }

    public EntityTable<int> EntityTable { get; set; } = new EntityTable<int>();
    string IProvide<string>.Value() => "Hello from Provider!";
    EntityTable<int> IProvide<EntityTable<int>>.Value() => EntityTable;

    [Dependency]
    private ICrewCsvReader crewCsvReader => this.DependOn<ICrewCsvReader>(() => new CrewCsvReader());

    public void Setup()
    {
        // Provide()を呼び出して依存関係を提供
        this.Provide(); // 依存関係の提供を通知
    }
}
