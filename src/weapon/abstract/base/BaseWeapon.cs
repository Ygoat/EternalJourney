namespace EternalJourney.Weapon.Abstract.Base;

using System;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using EternalJourney.Common.BaseEntity;
using EternalJourney.Common.Traits;



/// <summary>
/// ベース武器インターフェース
/// </summary>
public interface IBaseWeapon : IBaseEntity, IAttackable
{
}

/// <summary>
/// ベース武器クラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class BaseWeapon : BaseEntity, IBaseWeapon
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public bool CanAttack { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public virtual void Attack()
    {
        throw new NotImplementedException();
    }
}
