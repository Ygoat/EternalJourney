namespace EternalJourney.Cores.Models.Crews;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// クルーの役割
/// </summary>
public enum CrewRole
{
    /// <summary>
    /// 指揮官（作業クルーの配置を決める）
    /// </summary>
    Commander,

    /// <summary>
    /// 作業クルー（銃座の射撃・船体の修理）
    /// </summary>
    Worker,
}

/// <summary>
/// クルーの個性
/// </summary>
public enum CrewPersonality
{
    /// <summary>
    /// 臆病（早めに修理させる／先に修理へ回る）
    /// </summary>
    Coward,

    /// <summary>
    /// 普通
    /// </summary>
    Steady,

    /// <summary>
    /// 勇敢（ぎりぎりまで戦闘する／最後まで銃座に残る）
    /// </summary>
    Brave,
}

/// <summary>
/// パッシブスキルの対象となる武器の種類
/// </summary>
public enum WeaponKind
{
    Bullet,
    Explosion,
    Laser,
    Flame,
}

/// <summary>
/// パッシブスキルの効果の種類
/// </summary>
public enum CrewPassiveKind
{
    /// <summary>
    /// 連射速度向上（割合）
    /// </summary>
    FireRate,

    /// <summary>
    /// 攻撃力向上（割合）
    /// </summary>
    Attack,

    /// <summary>
    /// リロード速度向上（停止時間の短縮割合）
    /// </summary>
    ReloadSpeed,

    /// <summary>
    /// 修理速度向上（割合）
    /// </summary>
    RepairSpeed,
}

/// <summary>
/// パッシブスキル
/// </summary>
/// <param name="Kind">効果の種類</param>
/// <param name="Target">対象の武器（null は全武器）</param>
/// <param name="Value">効果量（割合）</param>
public readonly record struct CrewPassive(CrewPassiveKind Kind, WeaponKind? Target, float Value)
{
    /// <summary>
    /// 指定の武器に効くか（修理速度は武器に関係しない）
    /// </summary>
    public bool AppliesTo(WeaponKind kind) =>
        Kind != CrewPassiveKind.RepairSpeed && (Target == null || Target == kind);
}

/// <summary>
/// クルーの定義
/// </summary>
/// <param name="Id">ID</param>
/// <param name="Name">名前</param>
/// <param name="Role">役割</param>
/// <param name="Personality">個性</param>
/// <param name="Passive">パッシブスキル</param>
public sealed record CrewDefinition(int Id, string Name, CrewRole Role, CrewPersonality Personality, CrewPassive Passive)
{
    /// <summary>
    /// 駒に表示する頭文字
    /// </summary>
    public string Initial => Name.Length > 0 ? Name[..1] : "?";

    /// <summary>
    /// 選択画面に表示する説明文
    /// </summary>
    public string Description =>
        $"{Name}（{CrewCatalog.PersonalityName(Personality)}）\n{CrewCatalog.PassiveText(Passive)}";
}

/// <summary>
/// クルーのマスタ
/// </summary>
public static class CrewCatalog
{
    /// <summary>
    /// 指揮官候補
    /// </summary>
    public static readonly IReadOnlyList<CrewDefinition> Commanders = new[]
    {
        new CrewDefinition(1, "アマギ", CrewRole.Commander, CrewPersonality.Coward,
            new CrewPassive(CrewPassiveKind.FireRate, null, 0.10f)),
        new CrewDefinition(2, "カザマ", CrewRole.Commander, CrewPersonality.Steady,
            new CrewPassive(CrewPassiveKind.Attack, null, 0.10f)),
        new CrewDefinition(3, "ミズハ", CrewRole.Commander, CrewPersonality.Brave,
            new CrewPassive(CrewPassiveKind.ReloadSpeed, null, 0.15f)),
        new CrewDefinition(4, "クロガネ", CrewRole.Commander, CrewPersonality.Steady,
            new CrewPassive(CrewPassiveKind.RepairSpeed, null, 0.50f)),
    };

    /// <summary>
    /// 作業クルー候補
    /// </summary>
    public static readonly IReadOnlyList<CrewDefinition> Workers = new[]
    {
        new CrewDefinition(11, "ハヤト", CrewRole.Worker, CrewPersonality.Brave,
            new CrewPassive(CrewPassiveKind.FireRate, WeaponKind.Bullet, 0.20f)),
        new CrewDefinition(12, "リン", CrewRole.Worker, CrewPersonality.Coward,
            new CrewPassive(CrewPassiveKind.FireRate, WeaponKind.Laser, 0.20f)),
        new CrewDefinition(13, "ゴウ", CrewRole.Worker, CrewPersonality.Brave,
            new CrewPassive(CrewPassiveKind.Attack, WeaponKind.Explosion, 0.25f)),
        new CrewDefinition(14, "ミナ", CrewRole.Worker, CrewPersonality.Steady,
            new CrewPassive(CrewPassiveKind.Attack, WeaponKind.Flame, 0.20f)),
        new CrewDefinition(15, "ソラ", CrewRole.Worker, CrewPersonality.Coward,
            new CrewPassive(CrewPassiveKind.RepairSpeed, null, 0.50f)),
        new CrewDefinition(16, "タクミ", CrewRole.Worker, CrewPersonality.Steady,
            new CrewPassive(CrewPassiveKind.ReloadSpeed, null, 0.20f)),
        new CrewDefinition(17, "レイ", CrewRole.Worker, CrewPersonality.Brave,
            new CrewPassive(CrewPassiveKind.Attack, WeaponKind.Laser, 0.20f)),
        new CrewDefinition(18, "ユウ", CrewRole.Worker, CrewPersonality.Coward,
            new CrewPassive(CrewPassiveKind.Attack, WeaponKind.Bullet, 0.20f)),
    };

    /// <summary>
    /// 選ぶ作業クルーの人数
    /// </summary>
    public const int WORKER_COUNT = 4;

    /// <summary>
    /// 未選択時の既定の指揮官
    /// </summary>
    public static CrewDefinition DefaultCommander => Commanders[0];

    /// <summary>
    /// 未選択時の既定の作業クルー
    /// </summary>
    public static IReadOnlyList<CrewDefinition> DefaultWorkers => Workers.Take(WORKER_COUNT).ToArray();

    /// <summary>
    /// 個性の表示名
    /// </summary>
    public static string PersonalityName(CrewPersonality personality) => personality switch
    {
        CrewPersonality.Coward => "臆病",
        CrewPersonality.Steady => "普通",
        CrewPersonality.Brave => "勇敢",
        _ => "",
    };

    /// <summary>
    /// 武器の種類の表示名
    /// </summary>
    public static string WeaponKindName(WeaponKind? kind) => kind switch
    {
        null => "全武器",
        WeaponKind.Bullet => "通常弾",
        WeaponKind.Explosion => "爆発弾",
        WeaponKind.Laser => "レーザー",
        WeaponKind.Flame => "放射",
        _ => "",
    };

    /// <summary>
    /// パッシブスキルの説明文
    /// </summary>
    public static string PassiveText(CrewPassive passive)
    {
        int percent = (int)MathF.Round(passive.Value * 100f);
        return passive.Kind switch
        {
            CrewPassiveKind.FireRate => $"{WeaponKindName(passive.Target)}の連射速度 +{percent}%",
            CrewPassiveKind.Attack => $"{WeaponKindName(passive.Target)}の攻撃力 +{percent}%",
            CrewPassiveKind.ReloadSpeed => $"{WeaponKindName(passive.Target)}のリロード・停止時間 -{percent}%",
            CrewPassiveKind.RepairSpeed => $"修理速度 +{percent}%",
            _ => "",
        };
    }
}
