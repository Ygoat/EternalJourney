namespace EternalJourney.Enemy.Strategies.Movement;

using EternalJourney.Cores.Models.Enemy;
using Godot;

/// <summary>
/// 停止と移動を繰り返す移動戦略
/// </summary>
public class StopAndGoMovementStrategy : IMovementStrategy
{
    private float _moveDuration = 1.0f;
    private float _stopDuration = 0.5f;

    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize(MovementConfig config, Vector2 targetPosition)
    {
        if (config.Params.TryGetValue("moveDuration", out float md))
        {
            _moveDuration = md;
        }
        if (config.Params.TryGetValue("stopDuration", out float sd))
        {
            _stopDuration = sd;
        }
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
