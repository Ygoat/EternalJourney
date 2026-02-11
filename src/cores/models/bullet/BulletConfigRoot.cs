namespace EternalJourney.Cores.Models.Bullet;

using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// 弾丸設定ルート
/// </summary>
public class BulletConfigRoot
{
    /// <summary>
    /// 弾丸設定リスト
    /// </summary>
    [JsonPropertyName("bullets")]
    public List<BulletConfig> Bullets { get; set; } = new();
}
