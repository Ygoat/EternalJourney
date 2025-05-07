namespace EternalJourney.Enemy.Abstract.Base;

using System;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using EternalJourney.Battle.Domain;
using EternalJourney.Common.BaseEntity;
using EternalJourney.Common.StatusEffect;
using EternalJourney.Enemy.Abstract.Base.State;
using Godot;

/// <summary>
/// ベースエネミーインターフェース
/// </summary>
public interface IBaseEnemy : IBaseEntity, IStatusEffectTarget
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
    /// ベースエネミーロジック
    /// </summary>
    public BaseEnemyLogic BaseEnemyLogic { get; set; } = default!;

    /// <summary>
    /// ベースエネミーバインド
    /// </summary>
    public BaseEnemyLogic.IBinding BaseEnemyBinding { get; set; } = default!;

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

    [Dependency]
    public IBattleRepo BattleRepo => this.DependOn<IBattleRepo>();

    public virtual void Setup()
    {
        StatusEffectManager = new StatusEffectManager();
        BaseEnemyLogic = new BaseEnemyLogic();
        BaseEnemyBinding = BaseEnemyLogic.Bind();
        BaseEnemyLogic.Set(BattleRepo);
        BaseEnemyLogic.Set(Status);
    }

    public virtual void OnResolved()
    {
        AddChild(StatusEffectManager);
        StatusEffectManager.PoisonEffect.Damaged += OnPoisonDamaged;
        BaseEnemyBinding
            .Handle((in BaseEnemyLogic.Output.ReduceDurability output) =>
            {
                Status.CurrentDur = output.ReducedDurability;
            });
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

    private void OnPoisonDamaged(float damage)
    {
        BaseEnemyLogic.Input(new BaseEnemyLogic.Input.PoisonDamage(damage));
    }
}
