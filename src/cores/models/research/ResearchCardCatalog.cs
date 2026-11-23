namespace EternalJourney.Cores.Models.Research;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 研究カードの定義
/// </summary>
/// <param name="Type">種類</param>
/// <param name="Target">強化対象</param>
/// <param name="Name">表示名</param>
/// <param name="DescriptionFormat">効果説明の書式（{0} に効果量が入る）</param>
/// <param name="Values">レアリティ別の効果量（Common, Rare, Epic, Legendary の順）</param>
/// <param name="MinRarity">出現する最低レアリティ</param>
public sealed record ResearchCardDefinition(
    ResearchCardType Type,
    ResearchTarget Target,
    string Name,
    string DescriptionFormat,
    float[] Values,
    ResearchRarity MinRarity = ResearchRarity.Common
)
{
    /// <summary>
    /// レアリティに応じた効果量
    /// </summary>
    public float ValueOf(ResearchRarity rarity) => Values[(int)rarity];

    /// <summary>
    /// 効果説明文
    /// </summary>
    public string Describe(ResearchRarity rarity) => string.Format(DescriptionFormat, ValueOf(rarity));
}

/// <summary>
/// 研究カードのマスタ（効果量・出現率）
/// </summary>
public static class ResearchCardCatalog
{
    /// <summary>
    /// レアリティ別の出現重み（Common, Rare, Epic, Legendary の順）
    /// </summary>
    public static readonly IReadOnlyList<float> RarityWeights = new[] { 60f, 25f, 12f, 3f };

    /// <summary>
    /// 全カード定義
    /// </summary>
    public static readonly IReadOnlyList<ResearchCardDefinition> All = new[]
    {
        new ResearchCardDefinition(ResearchCardType.AtkUp, ResearchTarget.Weapon, "攻撃力増強",
            "全ダメージ +{0}%", new[] { 10f, 20f, 35f, 60f }),
        new ResearchCardDefinition(ResearchCardType.FireRateUp, ResearchTarget.Weapon, "連射強化",
            "射撃・照射間隔 -{0}%", new[] { 8f, 15f, 25f, 40f }),
        new ResearchCardDefinition(ResearchCardType.MultiShot, ResearchTarget.Weapon, "多連射",
            "同時に撃つ弾 +{0}", new[] { 0f, 0f, 1f, 2f }, ResearchRarity.Epic),
        new ResearchCardDefinition(ResearchCardType.PierceUp, ResearchTarget.Weapon, "貫通強化",
            "弾の貫通 +{0}", new[] { 1f, 2f, 3f, 5f }),
        new ResearchCardDefinition(ResearchCardType.ReachUp, ResearchTarget.Weapon, "射程延長",
            "レーザー・放射の射程 +{0}%", new[] { 10f, 20f, 35f, 60f }),
        new ResearchCardDefinition(ResearchCardType.ShipDefUp, ResearchTarget.Ship, "装甲強化",
            "船の防御 +{0}", new[] { 5f, 10f, 18f, 30f }),
        new ResearchCardDefinition(ResearchCardType.ShipMaxHpUp, ResearchTarget.Ship, "船体拡張",
            "最大HP +{0}（同量回復）", new[] { 50f, 100f, 180f, 300f }),
        new ResearchCardDefinition(ResearchCardType.CooldownDown, ResearchTarget.Skill, "冷却短縮",
            "スキルのクールダウン -{0}%", new[] { 5f, 10f, 18f, 30f }),
        new ResearchCardDefinition(ResearchCardType.BuffDurationUp, ResearchTarget.Skill, "効果延長",
            "バフの持続時間 +{0}秒", new[] { 1f, 2f, 4f, 7f }),
        new ResearchCardDefinition(ResearchCardType.HealUp, ResearchTarget.Skill, "回復強化",
            "スキルの回復量 +{0}%", new[] { 10f, 20f, 35f, 60f }),
        new ResearchCardDefinition(ResearchCardType.SpGainUp, ResearchTarget.SP, "SP効率",
            "撃破時のSP獲得 +{0}%", new[] { 10f, 20f, 35f, 60f }),
        new ResearchCardDefinition(ResearchCardType.SpDamageUp, ResearchTarget.SP, "SP威力",
            "SPスキルのダメージ +{0}%", new[] { 20f, 40f, 70f, 120f }),
    };

    private static readonly Dictionary<ResearchCardType, ResearchCardDefinition> _byType =
        All.ToDictionary(d => d.Type);

    /// <summary>
    /// 種類から定義を取得する
    /// </summary>
    public static ResearchCardDefinition Get(ResearchCardType type) =>
        _byType.TryGetValue(type, out ResearchCardDefinition? def)
            ? def
            : throw new ArgumentOutOfRangeException(nameof(type), type, "未定義の研究カード");

    /// <summary>
    /// レアリティの表記
    /// </summary>
    public static string RarityLabel(ResearchRarity rarity) => rarity switch
    {
        ResearchRarity.Common => "C",
        ResearchRarity.Rare => "R",
        ResearchRarity.Epic => "E",
        ResearchRarity.Legendary => "L",
        _ => "?",
    };

    /// <summary>
    /// 強化対象の表記
    /// </summary>
    public static string TargetLabel(ResearchTarget target) => target switch
    {
        ResearchTarget.Ship => "SHIP",
        ResearchTarget.Weapon => "WEAPON",
        ResearchTarget.Skill => "SKILL",
        ResearchTarget.SP => "SP",
        _ => "",
    };
}
