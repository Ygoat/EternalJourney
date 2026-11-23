namespace EternalJourney.Enemy.Strategies.Movement;

using Godot;

/// <summary>
/// 直線移動戦略リソース
/// </summary>
[GlobalClass]
public partial class LinearMovementStrategyResource : EnemyMovementStrategyResource
{
    public override IMovementStrategy CreateStrategy() => new LinearMovementStrategy();
}
