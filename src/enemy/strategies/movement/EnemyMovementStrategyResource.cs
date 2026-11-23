namespace EternalJourney.Enemy.Strategies.Movement;

using Godot;

/// <summary>
/// エネミー移動戦略リソース基底（パラメータ保持＋生成のみ。実行時オブジェクトは毎回新規生成する）
/// </summary>
[GlobalClass]
public abstract partial class EnemyMovementStrategyResource : Resource
{
    public abstract IMovementStrategy CreateStrategy();
}
