namespace EternalJourney.Cores.Models.Weapon;

using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// 武器設定ルート
/// </summary>
public class WeaponConfigRoot
{
    [JsonPropertyName("weapons")]
    public List<WeaponEntry> Weapons { get; set; } = new();
}

/// <summary>
/// 武器設定エントリ
/// </summary>
public class WeaponEntry
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public WeaponStatusConfig Status { get; set; } = new();
}

/// <summary>
/// 武器ステータス設定
/// </summary>
public class WeaponStatusConfig
{
    [JsonPropertyName("rotationSpeed")]
    public float RotationSpeed { get; set; } = 0.05f;
}
