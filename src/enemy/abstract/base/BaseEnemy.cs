namespace EternalJourney.Enemy.Abstract.Base;

using System;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using EternalJourney.Common.BaseEntity;
using EternalJourney.Common.StatusEffect;
using EternalJourney.Common.Traits;
using Godot;

/// <summary>
/// ベースエネミーインターフェース
/// </summary>
public interface IBaseEnemy : IBaseEntity, IDestructible, IStatusEffectTarget
{
    /// <summary>
    /// ヒットシグナル
    /// </summary>
    public event BaseEnemy.HitEventHandler Hit;

    /// <summary>
    /// 除去シグナル
    /// </summary>
    public event BaseEnemy.RemovedEventHandler Removed;

    /// <summary>
    /// スポーン
    /// </summary>
    /// <param name="shotGlobalPosition"></param>
    /// <param name="shotGlobalAngle"></param>
    public void Spawn(Vector2 spawnGlobalPosition, float spawnGlobalAngle);
}

/// <summary>
/// ベース弾丸クラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class BaseEnemy : BaseEntity, IBaseEnemy
{
    public override void _Notification(int what) => this.Notify(what);

    /// <summary>
    /// ヒットシグナル
    /// </summary>
    [Signal]
    public delegate void HitEventHandler();

    public StatusEffectManager StatusEffectManager { get; set; } = default!;

    /// <summary>
    /// 自己除去イベント
    /// </summary>
    [Signal]
    public delegate void RemovedEventHandler(BaseEnemy Enemy);

    public virtual void Setup()
    {
        StatusEffectManager = new StatusEffectManager();
    }

    public virtual void OnResolved()
    {
        AddChild(StatusEffectManager);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="spawnGlobalPosition"></param>
    /// <param name="spawnGlobalAngle"></param>
    /// <exception cref="NotImplementedException"></exception>
    public virtual void Spawn(Vector2 spawnGlobalPosition, float spawnGlobalAngle)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public virtual void RemoveSelf()
    {
        throw new NotImplementedException();
    }
}
