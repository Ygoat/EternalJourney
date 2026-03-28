namespace EternalJourney.Cores.Models.Bullet;

using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// 弾丸移動パターン設定
/// </summary>
public class BulletMovementConfig
{
    /// <summary>
    /// 移動タイプ（linear等）
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "linear";

    /// <summary>
    /// パラメータ
    /// </summary>
    [JsonPropertyName("params")]
    public Dictionary<string, float> Params { get; set; } = new();
}
