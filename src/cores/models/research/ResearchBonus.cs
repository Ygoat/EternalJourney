namespace EternalJourney.Cores.Models.Research;

using System;

/// <summary>
/// 研究による累計強化値（バトル中ずっと有効）。不変の値で、カードを適用すると新しい値を返す
/// </summary>
public readonly record struct ResearchBonus
{
    /// <summary>
    /// 間隔・クールダウン短縮の下限（元の値に対する倍率）
    /// </summary>
    public const float MIN_INTERVAL_MULTIPLIER = 0.4f;

    /// <summary>全ダメージ増加率（0.1 = +10%）</summary>
    public float AtkRate { get; init; }

    /// <summary>射撃・照射間隔の短縮率（累計）</summary>
    public float FireIntervalReduction { get; init; }

    /// <summary>同時に撃つ追加弾数</summary>
    public int ExtraShots { get; init; }

    /// <summary>弾の追加貫通数</summary>
    public int PierceBonus { get; init; }

    /// <summary>レーザー・放射の射程増加率</summary>
    public float ReachRate { get; init; }

    /// <summary>船の防御加算値</summary>
    public float ShipDef { get; init; }

    /// <summary>船の最大HP加算値</summary>
    public float ShipMaxHp { get; init; }

    /// <summary>スキルのクールダウン短縮率（累計）</summary>
    public float CooldownReduction { get; init; }

    /// <summary>バフの持続時間加算（秒）</summary>
    public float BuffDuration { get; init; }

    /// <summary>スキルの回復量増加率</summary>
    public float HealRate { get; init; }

    /// <summary>SP獲得量増加率</summary>
    public float SpGainRate { get; init; }

    /// <summary>SPスキルのダメージ増加率</summary>
    public float SpDamageRate { get; init; }

    /// <summary>
    /// 射撃・照射間隔に掛ける倍率（下限あり）
    /// </summary>
    public float FireIntervalMultiplier => Math.Max(MIN_INTERVAL_MULTIPLIER, 1f - FireIntervalReduction);

    /// <summary>
    /// スキルのクールダウンに掛ける倍率（下限あり）
    /// </summary>
    public float CooldownMultiplier => Math.Max(MIN_INTERVAL_MULTIPLIER, 1f - CooldownReduction);

    /// <summary>
    /// 射程に掛ける倍率
    /// </summary>
    public float ReachMultiplier => 1f + ReachRate;

    /// <summary>
    /// カードの効果を加えた新しい値を返す
    /// </summary>
    public ResearchBonus Apply(ResearchCard card)
    {
        float v = ResearchCardCatalog.Get(card.Type).ValueOf(card.Rarity);
        float pct = v / 100f;
        return card.Type switch
        {
            ResearchCardType.AtkUp => this with { AtkRate = AtkRate + pct },
            ResearchCardType.FireRateUp => this with { FireIntervalReduction = FireIntervalReduction + pct },
            ResearchCardType.MultiShot => this with { ExtraShots = ExtraShots + (int)v },
            ResearchCardType.PierceUp => this with { PierceBonus = PierceBonus + (int)v },
            ResearchCardType.ReachUp => this with { ReachRate = ReachRate + pct },
            ResearchCardType.ShipDefUp => this with { ShipDef = ShipDef + v },
            ResearchCardType.ShipMaxHpUp => this with { ShipMaxHp = ShipMaxHp + v },
            ResearchCardType.CooldownDown => this with { CooldownReduction = CooldownReduction + pct },
            ResearchCardType.BuffDurationUp => this with { BuffDuration = BuffDuration + v },
            ResearchCardType.HealUp => this with { HealRate = HealRate + pct },
            ResearchCardType.SpGainUp => this with { SpGainRate = SpGainRate + pct },
            ResearchCardType.SpDamageUp => this with { SpDamageRate = SpDamageRate + pct },
            _ => this,
        };
    }
}
