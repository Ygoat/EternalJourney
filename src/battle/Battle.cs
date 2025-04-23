namespace EternalJourney.Battle;

using Chickensoft.AutoInject;
using Chickensoft.Collections;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Battle.Domain;
using Godot;

/// <summary>
/// バトルインターフェース
/// </summary>
public interface IBattle : INode2D, IProvide<IBattleRepo>, IProvide<EntityTable>
{
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
    }

    public void OnResolved()
    {
        this.Provide();
    }

    public void OnExitTree() { }
}
