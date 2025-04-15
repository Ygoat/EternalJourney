namespace EternalJourney.Bullet.Abstract.Base;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Common.BaseEntity;
using Godot;

/// <summary>
/// ベース弾丸インターフェース
/// </summary>
public interface IBaseBullet : INode2D
{
    /// <summary>
    /// ヒットシグナル
    /// </summary>
    public event BaseBullet.HitEventHandler Hit;

    /// <summary>
    /// 除去シグナル
    /// </summary>
    public event BaseBullet.RemovedEventHandler Removed;

    /// <summary>
    /// 弾丸射出
    /// </summary>
    /// <param name="shotGlobalPosition"></param>
    /// <param name="shotGlobalAngle"></param>
    public void Emit(Vector2 shotGlobalPosition, float shotGlobalAngle);

    /// <summary>
    /// 自ノード除去
    /// </summary>
    public void RemoveSelf();
}

/// <summary>
/// ベース弾丸クラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class BaseBullet : BaseEntity, IBaseBullet
{
    public override void _Notification(int what) => this.Notify(what);

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [Signal]
    public delegate void HitEventHandler();

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [Signal]
    public delegate void RemovedEventHandler(StandardBullet bullet);

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="shotGlobalPosition"></param>
    /// <param name="shotGlobalAngle"></param>
    public virtual void Emit(Vector2 shotGlobalPosition, float shotGlobalAngle) { }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public virtual void RemoveSelf() { }
}
