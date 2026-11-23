namespace EternalJourney.Bullet.Strategies.Collision;

using Godot;

/// <summary>
/// 通常衝突戦略リソース
/// </summary>
[GlobalClass]
public partial class NormalCollisionStrategyResource : BulletCollisionStrategyResource
{
    public override IBulletCollisionStrategy CreateStrategy() => new NormalCollisionStrategy();
}
