namespace EternalJourney.Common.Traits;

/// <summary>
/// 攻撃可能インターフェース
/// </summary>
public interface IAttackable
{
    /// <summary>
    /// 攻撃可能フラグ
    /// </summary>
    public bool CanAttack { get; set; }

    /// <summary>
    /// 攻撃
    /// </summary>
    public void Attack();
}
