namespace EternalJourney.Enemy.Strategies.Movement;

using Godot;

/// <summary>
/// ホーミング移動戦略リソース
/// </summary>
[GlobalClass]
public partial class HomingMovementStrategyResource : EnemyMovementStrategyResource
{
    /// <summary>
    /// 旋回速度
    /// </summary>
    [Export]
    public float TurnSpeed { get; set; } = 1.5f;

    /// <summary>
    /// 最大旋回角度
    /// </summary>
    [Export]
    public float MaxTurnAngle { get; set; } = 45.0f;

    public override IMovementStrategy CreateStrategy() => new HomingMovementStrategy(TurnSpeed, MaxTurnAngle);
}
