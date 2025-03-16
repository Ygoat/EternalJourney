namespace EternalJourney.EnemySpawner;

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
    /// エネミーファクトリ
    /// </summary>
    [Node]
    public IEnemyFactory EnemyFactory { get; set; } = default!;
    #endregion  Nodes

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
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="delta"></param>
    public void OnPhysicsProcess(double delta)
    {
        // Path2D経路上の進行度を更新
        PathFollow2D.ProgressRatio += (float)delta;
    }
}
