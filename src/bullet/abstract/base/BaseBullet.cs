namespace EternalJourney.Bullet.Abstract.Base;

using System;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using EternalJourney.Common.BaseEntity;
using EternalJourney.Common.Traits;
using Godot;

/// <summary>
/// ベース弾丸インターフェース
/// </summary>
public interface IBaseBullet : IBaseEntity, IDestructible
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
}

/// <summary>
/// ベース弾丸クラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class BaseBullet : BaseEntity, IBaseBullet
{
    public override void _Notification(int what) => this.Notify(what);

    /// <summary>
    /// ヒットシグナル
    /// </summary>
    [Signal]
    public delegate void HitEventHandler();

    /// <summary>
    /// 自己除去イベント
    /// </summary>
    [Signal]
    public delegate void RemovedEventHandler(BaseBullet bullet);

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="shotGlobalPosition"></param>
    /// <param name="shotGlobalAngle"></param>
    public virtual void Emit(Vector2 shotGlobalPosition, float shotGlobalAngle)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public virtual void RemoveSelf()
    {
        // 親ノードを取得してから、子である自ノードを削除する
        GetParent().RemoveChild(this);
        // Removedシグナル出力
        EmitSignal(SignalName.Removed, this);
    }
}
