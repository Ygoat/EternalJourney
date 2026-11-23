namespace EternalJourney.Enemy.Strategies.Movement;

using Godot;

/// <summary>
/// サインウェーブ移動戦略リソース
/// </summary>
[GlobalClass]
public partial class SineWaveMovementStrategyResource : EnemyMovementStrategyResource
{
    /// <summary>
    /// 振幅
    /// </summary>
    [Export]
    public float Amplitude { get; set; } = 50.0f;

    /// <summary>
    /// 周波数
    /// </summary>
    [Export]
    public float Frequency { get; set; } = 2.0f;

    public override IMovementStrategy CreateStrategy() => new SineWaveMovementStrategy(Amplitude, Frequency);
}
