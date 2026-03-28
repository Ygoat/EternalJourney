namespace EternalJourney.Enemy.Strategies.Movement;

using EternalJourney.Cores.Models.Enemy;
using Godot;

/// <summary>
/// ホーミング移動戦略
/// </summary>
public class HomingMovementStrategy : IMovementStrategy
{
    private float _turnSpeed = 1.5f;
    private float _maxTurnAngle = 45.0f;
    private Vector2 _targetPosition;

    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize(MovementConfig config, Vector2 targetPosition)
    {
        _targetPosition = targetPosition;
        if (config.Params.TryGetValue("turnSpeed", out float ts))
        {
            _turnSpeed = ts;
        }
        if (config.Params.TryGetValue("maxTurnAngle", out float mta))
        {
            _maxTurnAngle = mta;
        }
    }

    /// <summary>
    /// 移動計算
    /// </summary>
    public Vector2 CalculateMovement(Vector2 currentPosition, Vector2 direction, float speed, float elapsedTime)
    {
        Vector2 toTarget = (_targetPosition - currentPosition).Normalized();
        Vector2 currentDir = direction.Normalized();

        float angleDiff = currentDir.AngleTo(toTarget);
        float maxRad = Mathf.DegToRad(_maxTurnAngle) * _turnSpeed * 0.016f;
        float clampedAngle = Mathf.Clamp(angleDiff, -maxRad, maxRad);

        Vector2 newDirection = currentDir.Rotated(clampedAngle);
        return newDirection * speed;
    }

    /// <summary>
    /// リセット
    /// </summary>
    public void Reset()
    {
        _targetPosition = Vector2.Zero;
    }
}
