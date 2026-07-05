namespace EternalJourney.Bullet.Strategies.Collision;

/// <summary>
/// 耐久値枯渇時のアクション
/// </summary>
public enum OnDepletedAction
{
    /// <summary>
    /// 崩壊（即座に除去）
    /// </summary>
    Collapse,

    /// <summary>
    /// 爆風（爆風フェーズに遷移）
    /// </summary>
    Blast,
}

/// <summary>
/// 弾丸衝突戦略インターフェース
/// </summary>
public interface IBulletCollisionStrategy
{
    /// <summary>
    /// 初期化
    /// </summary>
    void Initialize();

    /// <summary>
    /// 1ヒットあたりの耐久コスト
    /// </summary>
    float GetDurabilityCost();

    /// <summary>
    /// ステータスエフェクトを適用するか
    /// </summary>
    bool ShouldApplyStatusEffects();

    /// <summary>
    /// 耐久値枯渇時のアクション
    /// </summary>
    OnDepletedAction GetOnDepletedAction();

    /// <summary>
    /// リセット（プールへの返却時）
    /// </summary>
    void Reset();
}
