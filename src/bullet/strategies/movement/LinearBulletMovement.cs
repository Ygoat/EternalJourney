namespace EternalJourney.Bullet.Strategies.Movement;

using EternalJourney.Cores.Models.Bullet;
using Godot;

/// <summary>
/// 直線移動戦略
/// </summary>
public class LinearBulletMovement : IBulletMovementStrategy
{
    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize(BulletMovementConfig config)
    {
    }

    /// <summary>
    /// 移動計算（直線移動）
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
