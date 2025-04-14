namespace EternalJourney.Common.Traits;

/// <summary>
/// 移動可能インターフェース
/// </summary>
public interface IMovable
{
    /// <summary>
    /// スピード
    /// </summary>
    public float Speed { get; set; }

    /// <summary>
    /// 移動
    /// </summary>
    public void Move();
}
