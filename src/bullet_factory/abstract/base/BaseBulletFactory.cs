namespace EternalJourney.Bullet.Abstract.Base;

using System;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using EternalJourney.Common.BaseEntity;

/// <summary>
/// ベース弾丸ファクトリインターフェース
/// </summary>
public interface IBaseBulletFactory : IBaseEntity
{
    /// <summary>
    /// 弾丸生成
    /// </summary>
    public void GenerateBullet();
}

/// <summary>
/// ベース弾丸ファクトリクラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class BaseBulletFactory : BaseEntity, IBaseBulletFactory
{
    public override void _Notification(int what) => this.Notify(what);

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public virtual void GenerateBullet()
    {
        throw new NotImplementedException();
    }
}
