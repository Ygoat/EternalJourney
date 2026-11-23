namespace EternalJourney.Enemy.Strategies.Movement;

using Godot;

/// <summary>
/// 停止と移動を繰り返す移動戦略リソース
/// </summary>
[GlobalClass]
public partial class StopAndGoMovementStrategyResource : EnemyMovementStrategyResource
{
    /// <summary>
    /// 移動時間
    /// </summary>
    [Export]
    public float MoveDuration { get; set; } = 1.0f;

    /// <summary>
    /// 停止時間
    /// </summary>
    [Export]
    public float StopDuration { get; set; } = 0.5f;

    public override IMovementStrategy CreateStrategy() => new StopAndGoMovementStrategy(MoveDuration, StopDuration);
}
