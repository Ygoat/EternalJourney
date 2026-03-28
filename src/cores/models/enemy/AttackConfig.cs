namespace EternalJourney.Cores.Models.Enemy;

using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// 攻撃パターン設定（将来実装用）
/// </summary>
public class AttackConfig
{
    /// <summary>
    /// 攻撃タイプ（none, single_shot, aimed_shot, spread_shot）
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "none";

    /// <summary>
    /// パラメータ
    /// </summary>
    [JsonPropertyName("params")]
    public Dictionary<string, float> Params { get; set; } = new();
}
