namespace EternalJourney.Bullet.Strategies.Collision;

using EternalJourney.Cores.Models.Bullet;

/// <summary>
/// 通常衝突戦略（耐久値0で崩壊、ステータスエフェクト適用）
/// </summary>
public class NormalCollisionStrategy : IBulletCollisionStrategy
{
    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize(BulletCollisionConfig config)
    {
    }

    /// <summary>
    /// 1ヒットあたりの耐久コスト
    /// </summary>
    public float GetDurabilityCost() => 1.0f;

    /// <summary>
    /// ステータスエフェクトを適用する
    /// </summary>
    public bool ShouldApplyStatusEffects() => true;

    /// <summary>
    /// 耐久値枯渇時は崩壊
    /// </summary>
    public OnDepletedAction GetOnDepletedAction() => OnDepletedAction.Collapse;

    /// <summary>
    /// リセット
    /// </summary>
    public void Reset()
    {
    }
}
