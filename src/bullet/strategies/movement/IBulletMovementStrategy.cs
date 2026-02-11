namespace EternalJourney.Bullet.Strategies.Movement;

using EternalJourney.Cores.Models.Bullet;
using Godot;

/// <summary>
/// 弾丸移動戦略インターフェース
/// </summary>
public interface IBulletMovementStrategy
{
    /// <summary>
    /// 設定で初期化
    /// </summary>
    /// <param name="config">移動設定</param>
    void Initialize(BulletMovementConfig config);

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
