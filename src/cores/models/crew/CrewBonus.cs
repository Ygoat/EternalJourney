namespace EternalJourney.Cores.Models.Crews;

using System;

/// <summary>
/// 銃座の武器に掛かるクルーの補正
/// </summary>
/// <param name="FireIntervalMultiplier">発射間隔・ダメージ間隔の倍率</param>
/// <param name="AtkMultiplier">攻撃力の倍率</param>
/// <param name="LockTimeMultiplier">リロード・停止時間の倍率</param>
public readonly record struct CrewBonus(float FireIntervalMultiplier, float AtkMultiplier, float LockTimeMultiplier)
{
    /// <summary>
    /// 停止時間の短縮の下限（元の何倍まで短くなるか）
    /// </summary>
    public const float MIN_LOCK_TIME_MULTIPLIER = 0.4f;

    /// <summary>
    /// 補正なし
    /// </summary>
    public static readonly CrewBonus None = new(1f, 1f, 1f);

    /// <summary>
    /// 作業クルー不在時（自動攻撃モード）の攻撃力倍率
    /// </summary>
    public const float AUTO_ATK_MULTIPLIER = 0.7f;

    /// <summary>
    /// 作業クルー不在時（自動攻撃モード）の発射速度倍率
    /// </summary>
    public const float AUTO_FIRE_RATE_MULTIPLIER = 0.8f;

    /// <summary>
    /// 自動攻撃モードの補正を掛けた値を返す（攻撃力 0.7 倍、発射速度 0.8 倍）
    /// </summary>
    public CrewBonus WithAutoMode() => this with
    {
        AtkMultiplier = AtkMultiplier * AUTO_ATK_MULTIPLIER,
        FireIntervalMultiplier = FireIntervalMultiplier / AUTO_FIRE_RATE_MULTIPLIER,
    };

    /// <summary>
    /// 指揮官の全体パッシブと、銃座を操作する作業クルーのパッシブを合成する
    /// </summary>
    /// <param name="kind">銃座の武器の種類</param>
    /// <param name="commander">指揮官（null なら効果なし）</param>
    /// <param name="worker">銃座を操作している作業クルー（null なら効果なし）</param>
    public static CrewBonus For(WeaponKind kind, CrewDefinition? commander, CrewDefinition? worker)
    {
        float fireRate = 0f;
        float attack = 0f;
        float reload = 0f;
        foreach (CrewDefinition? crew in new[] { commander, worker })
        {
            if (crew == null || !crew.Passive.AppliesTo(kind))
            {
                continue;
            }
            switch (crew.Passive.Kind)
            {
                case CrewPassiveKind.FireRate:
                    fireRate += crew.Passive.Value;
                    break;
                case CrewPassiveKind.Attack:
                    attack += crew.Passive.Value;
                    break;
                case CrewPassiveKind.ReloadSpeed:
                    reload += crew.Passive.Value;
                    break;
                default:
                    break;
            }
        }
        return new CrewBonus(
            1f / (1f + fireRate),
            1f + attack,
            Math.Max(MIN_LOCK_TIME_MULTIPLIER, 1f - reload));
    }

    /// <summary>
    /// 修理速度の倍率（指揮官と修理する作業クルーの修理速度パッシブを合成する）
    /// </summary>
    public static float RepairMultiplier(CrewDefinition? commander, CrewDefinition worker)
    {
        float bonus = 0f;
        foreach (CrewDefinition? crew in new[] { commander, worker })
        {
            if (crew?.Passive.Kind == CrewPassiveKind.RepairSpeed)
            {
                bonus += crew.Passive.Value;
            }
        }
        return 1f + bonus;
    }
}
