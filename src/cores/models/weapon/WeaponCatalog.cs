namespace EternalJourney.Cores.Models.Weapon;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 船に積める武器の種類
/// </summary>
public enum WeaponType
{
    Normal,
    Penetrate,
    Explosion,
    Laser,
    PiercingLaser,
    Flame,
    Cold,
}

/// <summary>
/// 武器の定義
/// </summary>
/// <param name="Type">種類</param>
/// <param name="Name">表示名</param>
/// <param name="Description">説明文（特性と性質）</param>
/// <param name="ScenePath">武器シーンのパス</param>
public sealed record WeaponDefinition(WeaponType Type, string Name, string Description, string ScenePath);

/// <summary>
/// 武器のマスタ
/// </summary>
public static class WeaponCatalog
{
    /// <summary>
    /// 銃座の数
    /// </summary>
    public const int SLOT_COUNT = 4;

    /// <summary>
    /// 選択可能な武器（選択画面の並び順）
    /// </summary>
    public static readonly IReadOnlyList<WeaponDefinition> All = new[]
    {
        new WeaponDefinition(WeaponType.Normal, "通常弾", "リロード：弾を連射する標準的な武器",
            "res://src/weapon/01_NormalWeapon.tscn"),
        new WeaponDefinition(WeaponType.Penetrate, "貫通弾", "特性なし：敵を貫通する弾を撃つ",
            "res://src/weapon/02_PenetrateWeapon.tscn"),
        new WeaponDefinition(WeaponType.Explosion, "爆発弾", "チャージ：溜めてから威力3倍の爆発弾を撃つ",
            "res://src/weapon/03_ExplosionWeapon.tscn"),
        new WeaponDefinition(WeaponType.Laser, "レーザー", "バッテリー：最寄りの敵を照射し続ける",
            "res://src/weapon/04_LaserWeapon.tscn"),
        new WeaponDefinition(WeaponType.PiercingLaser, "貫通レーザー", "バッテリー：射線上の敵をまとめて照射する",
            "res://src/weapon/05_PiercingLaserWeapon.tscn"),
        new WeaponDefinition(WeaponType.Flame, "火炎", "オーバーヒート：扇状に炎を放ち毒を与える",
            "res://src/weapon/06_FlameWeapon.tscn"),
        new WeaponDefinition(WeaponType.Cold, "冷気", "オーバーヒート：扇状に冷気を放ちスタンさせる",
            "res://src/weapon/07_ColdWeapon.tscn"),
    };

    /// <summary>
    /// 既定の配置（左上・左下・右上・右下）
    /// </summary>
    public static readonly IReadOnlyList<WeaponType> DefaultLoadout = new[]
    {
        WeaponType.Normal, WeaponType.Laser, WeaponType.Explosion, WeaponType.Flame,
    };

    private static readonly Dictionary<WeaponType, WeaponDefinition> _byType = All.ToDictionary(d => d.Type);

    /// <summary>
    /// 種類から定義を取得する
    /// </summary>
    public static WeaponDefinition Get(WeaponType type) =>
        _byType.TryGetValue(type, out WeaponDefinition? def)
            ? def
            : throw new ArgumentOutOfRangeException(nameof(type), type, "未定義の武器");

    /// <summary>
    /// 種類の並び順（選択画面の番号）
    /// </summary>
    public static int IndexOf(WeaponType type) => All.ToList().FindIndex(d => d.Type == type);

    /// <summary>
    /// 銃座の表示名
    /// </summary>
    public static string SlotName(int slot) => slot switch
    {
        0 => "左上",
        1 => "左下",
        2 => "右上",
        3 => "右下",
        _ => "",
    };
}
