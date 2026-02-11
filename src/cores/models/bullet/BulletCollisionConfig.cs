namespace EternalJourney.Cores.Models.Bullet;

using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// 弾丸衝突パターン設定
/// </summary>
public class BulletCollisionConfig
{
    /// <summary>
    /// 衝突タイプ（normal, explosion等）
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "normal";

    /// <summary>
    /// パラメータ
    /// </summary>
    [JsonPropertyName("params")]
    public Dictionary<string, float> Params { get; set; } = new();
}
