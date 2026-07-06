namespace EternalJourney.Enemy.Strategies.Movement;

using Godot;

/// <summary>
/// 停止と移動を繰り返す移動戦略
/// </summary>
public class StopAndGoMovementStrategy : IMovementStrategy
{
    private readonly float _moveDuration;
    private readonly float _stopDuration;

    public StopAndGoMovementStrategy(float moveDuration = 1.0f, float stopDuration = 0.5f)
    {
        _moveDuration = moveDuration;
        _stopDuration = stopDuration;
    }

    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize(Vector2 targetPosition)
    {
    }

    /// <summary>
    /// 移動計算
    /// </summary>
    public Vector2 CalculateMovement(Vector2 currentPosition, Vector2 direction, float speed, float elapsedTime)
    {
        float cycleTime = _moveDuration + _stopDuration;
        float cyclePosition = elapsedTime % cycleTime;

        if (cyclePosition < _moveDuration)
        {
            return direction.Normalized() * speed;
        }
        return Vector2.Zero;
    }

    /// <summary>
    /// リセット
    /// </summary>
    public void Reset()
    {
    }
}
