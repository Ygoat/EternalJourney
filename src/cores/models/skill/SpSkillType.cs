namespace EternalJourney.Cores.Models.Skill;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// SPスキルの種類
/// </summary>
public enum SpSkillType
{
    Stun,
    BigDamage,
    AttackDown,
    FireRateDown,
}

/// <summary>
/// SPスキルの定義
/// </summary>
/// <param name="Type">種類</param>
/// <param name="Name">表示名</param>
/// <param name="Description">説明文</param>
/// <param name="Damage">衝撃波のダメージ</param>
/// <param name="Stun">命中した敵をスタンさせるか</param>
/// <param name="EnemyAtkMultiplier">デバフ中の敵攻撃力倍率（1 で効果なし）</param>
/// <param name="EnemyFireIntervalMultiplier">デバフ中の敵発射間隔倍率（1 で効果なし）</param>
/// <param name="DebuffDuration">デバフの持続時間（秒、0 でデバフなし）</param>
public sealed record SpSkillDefinition(
    SpSkillType Type,
    string Name,
    string Description,
    float Damage,
    bool Stun = false,
    float EnemyAtkMultiplier = 1f,
    float EnemyFireIntervalMultiplier = 1f,
    float DebuffDuration = 0f
)
{
    /// <summary>
    /// 敵へのデバフを伴うか
    /// </summary>
    public bool HasDebuff => DebuffDuration > 0f;
}

/// <summary>
/// SPスキルのマスタ
/// </summary>
public static class SpSkillCatalog
{
    /// <summary>
    /// 選択可能なSPスキル（選択画面の並び順）
    /// </summary>
    public static readonly IReadOnlyList<SpSkillDefinition> All = new[]
    {
        new SpSkillDefinition(SpSkillType.Stun, "スタン",
            "衝撃波で100ダメージを与え、命中した敵をスタンさせる", 100f, Stun: true),
        new SpSkillDefinition(SpSkillType.BigDamage, "大ダメージ",
            "衝撃波で300ダメージを与える", 300f),
        new SpSkillDefinition(SpSkillType.AttackDown, "攻撃力減少",
            "衝撃波で50ダメージを与え、10秒間敵の攻撃力を半減させる", 50f,
            EnemyAtkMultiplier: 0.5f, DebuffDuration: 10f),
        new SpSkillDefinition(SpSkillType.FireRateDown, "発射間隔増大",
            "衝撃波で50ダメージを与え、10秒間敵の発射間隔を2倍にする", 50f,
            EnemyFireIntervalMultiplier: 2f, DebuffDuration: 10f),
    };

    private static readonly Dictionary<SpSkillType, SpSkillDefinition> _byType = All.ToDictionary(d => d.Type);

    /// <summary>
    /// 種類から定義を取得する
    /// </summary>
    public static SpSkillDefinition Get(SpSkillType type) =>
        _byType.TryGetValue(type, out SpSkillDefinition? def)
            ? def
            : throw new ArgumentOutOfRangeException(nameof(type), type, "未定義のSPスキル");
}
