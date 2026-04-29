namespace EternalJourney.Game;

using Chickensoft.AutoInject;
using Chickensoft.Collections;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Battle;
using EternalJourney.Battle.Domain;
using EternalJourney.Cores.Repositories;
using EternalJourney.Game.Domain;
using EternalJourney.Result;
using Godot;


/// <summary>
/// ゲームインターフェース
/// </summary>
public interface IGame : INode2D, IProvide<EntityTable<int>>, IProvide<IBattleRepo>, IProvide<IGameRepo> { }

/// <summary>
/// ゲームクラス
/// </summary>
[Meta(typeof(IAutoNode))] // 子Nodeへ親Nodeの値をDIするために必要なミックスイン
public partial class Game : Node2D, IGame
{
    // 子Nodeへ親Nodeの値をDIするために必要
    public override void _Notification(int what) => this.Notify(what);

    public EntityTable<int> EntityTable { get; set; } = new EntityTable<int>();
    EntityTable<int> IProvide<EntityTable<int>>.Value() => EntityTable;

    /// <summary>
    /// IBattleRepo を子ノード配下へ再提供
    /// </summary>
    IBattleRepo IProvide<IBattleRepo>.Value() => Battle.BattleRepo;

    /// <summary>
    /// ゲームリポジトリ
    /// </summary>
    public IGameRepo GameRepo { get; set; } = default!;

    /// <summary>
    /// IGameRepo を子ノード配下へ提供
    /// </summary>
    IGameRepo IProvide<IGameRepo>.Value() => GameRepo;

    /// <summary>
    /// バトル
    /// </summary>
    [Node]
    public IBattle Battle { get; set; } = default!;

    /// <summary>
    /// リザルト
    /// </summary>
    [Node]
    public IResult Result { get; set; } = default!;

    [Dependency]
    private ICrewCsvReader crewCsvReader => this.DependOn<ICrewCsvReader>(() => new CrewCsvReader());

    public void OnReady()
    {
        // Provide()を呼び出して依存関係を提供
        this.Provide();

        // Battle は子ノードのため OnReady 時点で初期化済み
        Battle.BattleRepo.GameOverOccurred += OnGameOver;
    }

    public void Setup()
    {
        GameRepo = new GameRepo();
    }

    public void OnTreeExiting()
    {
        Battle.BattleRepo.GameOverOccurred -= OnGameOver;
    }

    private void OnGameOver()
    {
        Battle.Hide();
        Result.Show();
    }
}
