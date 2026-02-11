namespace EternalJourney.Cores.Models.Bullet;

using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// 弾丸設定モデル
/// </summary>
public class BulletConfig
{
    /// <summary>
    /// 弾丸ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 弾丸名
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// ステータス設定
    /// </summary>
    [JsonPropertyName("status")]
    public BulletStatusConfig Status { get; set; } = new();

    /// <summary>
    /// 状態異常設定リスト
    /// </summary>
    [JsonPropertyName("statusEffects")]
    public List<BulletStatusEffectConfig> StatusEffects { get; set; } = new();

    /// <summary>
    /// 移動パターン設定
    /// </summary>
    [JsonPropertyName("movement")]
    public BulletMovementConfig Movement { get; set; } = new();

    /// <summary>
    /// 衝突パターン設定
    /// </summary>
    [JsonPropertyName("collision")]
    public BulletCollisionConfig Collision { get; set; } = new();
}
