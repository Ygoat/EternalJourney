namespace EternalJourney.EnemySpawner;

using System;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
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
    /// 通常エネミーファクトリ
    /// </summary>
    [Node]
    public IEnemyFactory NormalEnemyFactory { get; set; } = default!;

    /// <summary>
    /// サインウェーブエネミーファクトリ
    /// </summary>
    [Node]
    public IEnemyFactory SineWaveEnemyFactory { get; set; } = default!;

    /// <summary>
    /// ホーミングエネミーファクトリ
    /// </summary>
    [Node]
    public IEnemyFactory HomingEnemyFactory { get; set; } = default!;

    /// <summary>
    /// ストップアンドゴーエネミーファクトリ
    /// </summary>
    [Node]
    public IEnemyFactory StopAndGoEnemyFactory { get; set; } = default!;
    #endregion  Nodes

    #region State
    /// <summary>
    /// エネミーファクトリ一覧（ランダム選択用）
    /// </summary>
    private IEnemyFactory[] _enemyFactories = default!;

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
        SetPhysicsProcess(true);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void OnReady()
    {
        _enemyFactories = new[]
        {
            NormalEnemyFactory,
            SineWaveEnemyFactory,
            HomingEnemyFactory,
            StopAndGoEnemyFactory,
        };
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="delta"></param>
    public void OnPhysicsProcess(double delta)
    {
        // Path2D経路上の進行度を更新
        PathFollow2D.ProgressRatio += (float)delta;

        // エネミーファクトリをランダムに選んでスポーン
        GetRandomEnemyFactory().SpawnEnemy();
    }

    /// <summary>
    /// ランダムなエネミーファクトリを取得
    /// </summary>
    /// <returns>エネミーファクトリ</returns>
    private IEnemyFactory GetRandomEnemyFactory()
    {
        int index = _random.Next(_enemyFactories.Length);
        return _enemyFactories[index];
    }
}
