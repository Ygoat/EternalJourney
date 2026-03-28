namespace EternalJourney.Cores.Models.Enemy;

using System.Text.Json.Serialization;

/// <summary>
/// エネミーステータス設定
/// </summary>
public class EnemyStatusConfig
{
    /// <summary>
    /// 最大耐久値
    /// </summary>
    [JsonPropertyName("maxDur")]
    public float MaxDur { get; set; } = 10.0f;

    /// <summary>
    /// 攻撃力
    /// </summary>
    [JsonPropertyName("atk")]
    public float Atk { get; set; } = 1.0f;

    /// <summary>
    /// 速度
    /// </summary>
    [JsonPropertyName("spd")]
    public float Spd { get; set; } = 0.7f;

    /// <summary>
    /// 防御力
    /// </summary>
    [JsonPropertyName("def")]
    public float Def { get; set; } = 0.0f;

    /// <summary>
    /// サイズ
    /// </summary>
    [JsonPropertyName("size")]
    public float Size { get; set; } = 1.0f;
}
