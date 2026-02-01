namespace EternalJourney.Cores.Models.Enemy;

using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// エネミー設定ルート
/// </summary>
public class EnemyConfigRoot
{
    /// <summary>
    /// エネミー設定リスト
    /// </summary>
    [JsonPropertyName("enemies")]
    public List<EnemyConfig> Enemies { get; set; } = new();
}
