namespace EternalJourney.EnemySpawner;

using System;
using System.Collections.Generic;
using System.Linq;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Cores.Repositories;
using EternalJourney.EnemyFactory;
using Godot;

/// <summary>
/// エネミースポナーインターフェース
/// </summary>
public interface IEnemySpawner : INode2D, IProvide<IEnemySpawner>
{
    /// <summary>
    /// スポーン箇所移動用
    /// </summary>
    public IPathFollow2D PathFollow2D { get; set; }
}

/// <summary>
/// エネミースポナークラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class EnemySpawner : Node2D, IEnemySpawner
{
    public override void _Notification(int what) => this.Notify(what);

    #region  Nodes
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [Node]
    public IPathFollow2D PathFollow2D { get; set; } = default!;

    /// <summary>
    /// エネミーファクトリ
    /// </summary>
    [Node]
    public IEnemyFactory EnemyFactory { get; set; } = default!;
    #endregion  Nodes

    #region State
    /// <summary>
    /// エネミー設定リーダー
    /// </summary>
    private readonly EnemyConfigReader _enemyConfigReader = new();

    /// <summary>
    /// 利用可能なエネミーIDリスト
    /// </summary>
    private List<string> _enemyIds = new();

    /// <summary>
    /// 乱数生成器
    /// </summary>
    private readonly Random _random = new();
    #endregion State

    #region  Exports
    /// <summary>
    /// スポーン移動速度(Path2D上を移動する速度)
    /// </summary>
    public int Speed { get; set; } = default!;
    #endregion Exports

    #region Provisions
    /// <summary>
    /// エネミースポナープロバイダ
    /// </summary>
    /// <returns></returns>
    IEnemySpawner IProvide<IEnemySpawner>.Value() => this;
    #endregion Provisions

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Initialize()
    {
        this.Provide();
        // JSONから利用可能なエネミーIDを読み込み
        _enemyIds = _enemyConfigReader.GetMany().Select(e => e.Id).ToList();
        SetPhysicsProcess(true);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void OnReady()
    {
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="delta"></param>
    public void OnPhysicsProcess(double delta)
    {
        // Path2D経路上の進行度を更新
        PathFollow2D.ProgressRatio += (float)delta;

        // JSONから読み込んだエネミーをランダムにスポーン
        string enemyId = GetRandomEnemyId();
        EnemyFactory.SpawnEnemy(enemyId);
    }

    /// <summary>
    /// ランダムなエネミーIDを取得
    /// </summary>
    /// <returns>エネミーID</returns>
    private string GetRandomEnemyId()
    {
        if (_enemyIds.Count == 0)
        {
            return "normal_enemy";
        }
        int index = _random.Next(_enemyIds.Count);
        return _enemyIds[index];
    }
}
