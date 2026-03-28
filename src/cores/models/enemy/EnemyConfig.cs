namespace EternalJourney.Cores.Models.Enemy;

using System.Text.Json.Serialization;

/// <summary>
/// エネミー設定モデル
/// </summary>
public class EnemyConfig
{
    /// <summary>
    /// エネミーID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// エネミー名
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// ステータス設定
    /// </summary>
    [JsonPropertyName("status")]
    public EnemyStatusConfig Status { get; set; } = new();

    /// <summary>
    /// 移動パターン設定
    /// </summary>
    [JsonPropertyName("movement")]
    public MovementConfig Movement { get; set; } = new();

    /// <summary>
    /// 攻撃パターン設定（将来実装用）
    /// </summary>
    [JsonPropertyName("attack")]
    public AttackConfig Attack { get; set; } = new();

    /// <summary>
    /// スコア値
    /// </summary>
    [JsonPropertyName("scoreValue")]
    public int ScoreValue { get; set; } = 10;
}
