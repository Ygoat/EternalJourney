namespace EternalJourney.Cores.Models;

/// <summary>Id,Name,Attack,Speed,Health,Type
/// クルーモデルクラス
/// </summary>
public class Clue
{
    /// <summary>
    /// ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 名前
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 攻撃力
    /// </summary>
    public int Attack { get; set; }

    /// <summary>
    /// 速さ
    /// </summary>
    public int Speed { get; set; }

    /// <summary>
    /// 体力
    /// </summary>
    public int Health { get; set; }

    /// <summary>
    /// タイプ
    /// </summary>
    public string Type { get; set; } = string.Empty;

}
