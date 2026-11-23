namespace EternalJourney.Bullet.Strategies.Movement;

using Godot;

/// <summary>
/// 直線移動戦略リソース
/// </summary>
[GlobalClass]
public partial class LinearBulletMovementStrategyResource : BulletMovementStrategyResource
{
    public override IBulletMovementStrategy CreateStrategy() => new LinearBulletMovement();
}
