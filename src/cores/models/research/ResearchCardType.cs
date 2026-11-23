namespace EternalJourney.Cores.Models.Research;

/// <summary>
/// 研究カードの種類
/// </summary>
public enum ResearchCardType
{
    /// <summary>攻撃力増強（Weapon）</summary>
    AtkUp,
    /// <summary>連射強化（Weapon）</summary>
    FireRateUp,
    /// <summary>多連射（Weapon）</summary>
    MultiShot,
    /// <summary>貫通強化（Weapon）</summary>
    PierceUp,
    /// <summary>射程延長（Weapon）</summary>
    ReachUp,
    /// <summary>装甲強化（Ship）</summary>
    ShipDefUp,
    /// <summary>船体拡張（Ship）</summary>
    ShipMaxHpUp,
    /// <summary>冷却短縮（Skill）</summary>
    CooldownDown,
    /// <summary>効果延長（Skill）</summary>
    BuffDurationUp,
    /// <summary>回復強化（Skill）</summary>
    HealUp,
    /// <summary>SP効率（SP）</summary>
    SpGainUp,
    /// <summary>SP威力（SP）</summary>
    SpDamageUp,
}

/// <summary>
/// 研究カードが強化する対象
/// </summary>
public enum ResearchTarget
{
    Ship,
    Weapon,
    Skill,
    SP,
}

/// <summary>
/// 研究カード（抽選結果の1枚）
/// </summary>
/// <param name="Type">種類</param>
/// <param name="Rarity">レアリティ</param>
public readonly record struct ResearchCard(ResearchCardType Type, ResearchRarity Rarity);

/// <summary>
/// 取得済みカード（同じ種類・レアリティは累積する）
/// </summary>
/// <param name="Type">種類</param>
/// <param name="Rarity">レアリティ</param>
/// <param name="Count">取得回数</param>
public readonly record struct AcquiredCard(ResearchCardType Type, ResearchRarity Rarity, int Count);
