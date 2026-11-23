namespace EternalJourney.Enemy.Strategies.Movement;

using Godot;

/// <summary>
/// ホーミング移動戦略
/// </summary>
public class HomingMovementStrategy : IMovementStrategy
{
    private readonly float _turnSpeed;
    private readonly float _maxTurnAngle;
    private Vector2 _targetPosition;

    public HomingMovementStrategy(float turnSpeed = 1.5f, float maxTurnAngle = 45.0f)
    {
        _turnSpeed = turnSpeed;
        _maxTurnAngle = maxTurnAngle;
    }

    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize(Vector2 targetPosition)
    {
        _targetPosition = targetPosition;
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
