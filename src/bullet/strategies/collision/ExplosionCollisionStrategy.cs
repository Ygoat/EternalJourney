namespace EternalJourney.Bullet.Strategies.Collision;

/// <summary>
/// 爆発衝突戦略（耐久値0で爆風フェーズ、ステータスエフェクト非適用）
/// </summary>
public class ExplosionCollisionStrategy : IBulletCollisionStrategy
{
    private readonly float _blastDuration;

    public ExplosionCollisionStrategy(float blastDuration = 0.5f)
    {
        _blastDuration = blastDuration;
    }

    /// <summary>
    /// 爆風持続時間
    /// </summary>
    public float BlastDuration => _blastDuration;

    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize()
    {
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
