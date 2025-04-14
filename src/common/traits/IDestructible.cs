namespace EternalJourney.Common.Traits;

using EternalJourney.Common.DurabilityModule;

/// <summary>
/// 破壊可能インターフェース
/// </summary>
public interface IDestructible
{
    /// <summary>
    /// 防御力
    /// </summary>
    public float Def { get; set; }

    /// <summary>
    /// 最大耐久値
    /// </summary>
    public float MaxDurability { get; set; }

    /// <summary>
    /// 耐久値モジュール
    /// </summary>
    public IDurabilityModule DurabilityModule { get; set; }

    /// <summary>
    /// 自己消去
    /// </summary>
    public void RemoveSelf();
}
