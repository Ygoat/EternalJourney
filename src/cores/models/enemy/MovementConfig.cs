namespace EternalJourney.Cores.Models.Enemy;

using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// 移動パターン設定
/// </summary>
public class MovementConfig
{
    /// <summary>
    /// 移動タイプ（linear, sine_wave, homing, stop_and_go）
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "linear";

    /// <summary>
    /// パラメータ
    /// </summary>
    [JsonPropertyName("params")]
    public Dictionary<string, float> Params { get; set; } = new();
}
