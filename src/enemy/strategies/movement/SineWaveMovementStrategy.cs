namespace EternalJourney.Enemy.Strategies.Movement;

using EternalJourney.Cores.Models.Enemy;
using Godot;

/// <summary>
/// サインウェーブ移動戦略
/// </summary>
public class SineWaveMovementStrategy : IMovementStrategy
{
    private float _amplitude = 50.0f;
    private float _frequency = 2.0f;
    private Vector2 _perpendicular;

    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize(MovementConfig config, Vector2 targetPosition)
    {
        if (config.Params.TryGetValue("amplitude", out float amp))
        {
            _amplitude = amp;
        }
        if (config.Params.TryGetValue("frequency", out float freq))
        {
            _frequency = freq;
        }
    }

    /// <summary>
    /// 移動計算
    /// </summary>
    public Vector2 CalculateMovement(Vector2 currentPosition, Vector2 direction, float speed, float elapsedTime)
    {
        Vector2 normalizedDir = direction.Normalized();
        _perpendicular = new Vector2(-normalizedDir.Y, normalizedDir.X);

        float sineOffset = Mathf.Sin(elapsedTime * _frequency) * _amplitude * 0.01f;
        return (normalizedDir * speed) + (_perpendicular * sineOffset);
    }

    /// <summary>
    /// リセット
    /// </summary>
    public void Reset()
    {
        _perpendicular = Vector2.Zero;
    }
}
