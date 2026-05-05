namespace EternalJourney.Battle;

using Chickensoft.AutoInject;
using Chickensoft.Collections;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Battle.Domain;
using EternalJourney.BattleUI;
using Godot;
/// <summary>
/// バトルインターフェース
/// </summary>
public interface IBattle : INode2D, IProvide<IBattleRepo>, IProvide<EntityTable>
{
    /// <summary>
    /// バトルリポジトリ
    /// </summary>
    IBattleRepo BattleRepo { get; }

    /// <summary>
    /// バトルを初期化し完了を通知する
    /// </summary>
    void Initialize();

    /// <summary>
    /// バトルを開始する（タイマー・物理プロセス開始）
    /// </summary>
    void StartBattle();
}

/// <summary>
/// バトルクラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class Battle : Node2D, IBattle
{
    public override void _Notification(int what) => this.Notify(what);

    #region Save
    /// <summary>
    /// エンティティテーブル
    /// </summary>
    public EntityTable EntityTable { get; set; } = new EntityTable();

    /// <summary>
    /// エンティティテーブルプロバイダー
    /// </summary>
    /// <returns></returns>
    EntityTable IProvide<EntityTable>.Value() => EntityTable;
    #endregion Save

    #region Nodes
    /// <summary>
    /// バトルUI
    /// </summary>
    [Node]
    public IBattleUI BattleUI { get; set; } = default!;
    #endregion Nodes

    #region State
    /// <summary>
    /// バトルレポジトリ
    /// </summary>
    public IBattleRepo BattleRepo { get; set; } = default!;
    #endregion State

    #region Provisions
    /// <summary>
    /// バトルレポジトリプロバイダー
    /// </summary>
    /// <returns></returns>
    IBattleRepo IProvide<IBattleRepo>.Value() => BattleRepo;
    #endregion Provisions

    public void Setup()
    {
        // バトルレポジトリインタスタンス化
        BattleRepo = new BattleRepo();

        // ロジックブロックステートで共有できるデータテーブル
        Blackboard upgradeDpendencies = new Blackboard();

        SetPhysicsProcess(false);

    }

    public void OnResolved()
    {
        this.Provide();
    }

    public void Initialize() { }

    public void StartBattle()
    {
        SetPhysicsProcess(true);
        BattleRepo.NotifyActivateBattleUI();
    }

    public void OnExitTree() { }
}
