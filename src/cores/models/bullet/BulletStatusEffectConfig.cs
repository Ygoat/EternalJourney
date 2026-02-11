namespace EternalJourney.Cores.Models.Bullet;

using System.Text.Json.Serialization;

/// <summary>
/// 弾丸状態異常設定
/// </summary>
public class BulletStatusEffectConfig
{
    /// <summary>
    /// 状態異常タイプ（poison等）
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 有効・無効
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;
}
