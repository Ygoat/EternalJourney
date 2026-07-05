namespace EternalJourney.Bullet.Strategies.Collision;

using Godot;

/// <summary>
/// 爆発衝突戦略リソース
/// </summary>
[GlobalClass]
public partial class ExplosionCollisionStrategyResource : BulletCollisionStrategyResource
{
    /// <summary>
    /// 爆風持続時間
    /// </summary>
    [Export]
    public float BlastDuration { get; set; } = 0.5f;

    public override IBulletCollisionStrategy CreateStrategy() => new ExplosionCollisionStrategy(BlastDuration);
}
