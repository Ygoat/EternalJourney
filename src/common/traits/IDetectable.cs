namespace EternalJourney.Common.Traits;

using EternalJourney.Common.BaseEntity;

/// <summary>
/// 探知可能インターフェース
/// </summary>
public interface IDetectable
{
    /// <summary>
    /// 探知距離
    /// </summary>
    public float DetectRange { get; set; }

    /// <summary>
    /// 探知可能
    /// </summary>
    public bool CanDetect { get; set; }

    /// <summary>
    /// エンティティ探知(1体のエンティティを標的として探知)
    /// </summary>
    /// <returns></returns>
    public T Detect<T>() where T : class;
}
