namespace EternalJourney.Enemy.Strategies.Movement;

using EternalJourney.Cores.Models.Enemy;
using Godot;

/// <summary>
/// 直線移動戦略
/// </summary>
public class LinearMovementStrategy : IMovementStrategy
{
    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize(MovementConfig config, Vector2 targetPosition)
    {
    }

    /// <summary>
    /// 移動計算
    /// </summary>
    public Vector2 CalculateMovement(Vector2 currentPosition, Vector2 direction, float speed, float elapsedTime)
    {
        return direction.Normalized() * speed;
    }

    /// <summary>
    /// リセット
    /// </summary>
    public void Reset()
    {
    }
}
