namespace EternalJourney.Enemy.Strategies.Movement;

using Godot;

/// <summary>
/// 移動戦略インターフェース
/// </summary>
public interface IMovementStrategy
{
    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="targetPosition">ターゲット位置</param>
    void Initialize(Vector2 targetPosition);

    /// <summary>
    /// 次フレームの移動差分を計算
    /// </summary>
    /// <param name="currentPosition">現在位置</param>
    /// <param name="direction">方向</param>
    /// <param name="speed">速度</param>
    /// <param name="elapsedTime">経過時間</param>
    /// <returns>移動差分</returns>
    Vector2 CalculateMovement(Vector2 currentPosition, Vector2 direction, float speed, float elapsedTime);

    /// <summary>
    /// リセット（プールへの返却時）
    /// </summary>
    void Reset();
}
