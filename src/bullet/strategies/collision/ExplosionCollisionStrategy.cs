namespace EternalJourney.Bullet.Strategies.Collision;

using EternalJourney.Cores.Models.Bullet;

/// <summary>
/// 爆発衝突戦略（耐久値0で爆風フェーズ、ステータスエフェクト非適用）
/// </summary>
public class ExplosionCollisionStrategy : IBulletCollisionStrategy
{
    private float _blastDuration = 0.5f;

    /// <summary>
    /// 爆風持続時間
    /// </summary>
    public float BlastDuration => _blastDuration;

    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize(BulletCollisionConfig config)
    {
        if (config.Params.TryGetValue("blastDuration", out float duration))
        {
            _blastDuration = duration;
        }
    }

    /// <summary>
    /// 1ヒットあたりの耐久コスト
    /// </summary>
    public float GetDurabilityCost() => 1.0f;

    /// <summary>
    /// ステータスエフェクトは適用しない
    /// </summary>
    public bool ShouldApplyStatusEffects() => false;

    /// <summary>
    /// 耐久値枯渇時は爆風フェーズへ
    /// </summary>
    public OnDepletedAction GetOnDepletedAction() => OnDepletedAction.Blast;

    /// <summary>
    /// リセット
    /// </summary>
    public void Reset()
    {
    }
}
