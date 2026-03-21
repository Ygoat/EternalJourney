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
public partial class Game : Node, IGame
{
    // 子Nodeへ親Nodeの値をDIするために必要
    public override void _Notification(int what) => this.Notify(what);

    public EntityTable<int> EntityTable { get; set; } = new EntityTable<int>();
    EntityTable<int> IProvide<EntityTable<int>>.Value() => EntityTable;

    [Dependency]
    private ICrewCsvReader crewCsvReader => this.DependOn<ICrewCsvReader>(() => new CrewCsvReader());

    public void OnReady()
    {
        // Provide()を呼び出して依存関係を提供
        this.Provide();
    }

    public void Setup() { }
}
