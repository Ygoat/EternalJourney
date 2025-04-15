namespace EternalJourney.Common.Traits;

/// <summary>
/// 移動可能インターフェース
/// </summary>
public interface IMovable
{
    /// <summary>
    /// 移動可能フラグ
    /// </summary>
    /// <returns></returns>
    public bool CanMove { get; set; }

    /// <summary>
    /// 移動
    /// </summary>
    public void Move();
}
