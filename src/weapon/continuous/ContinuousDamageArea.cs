namespace EternalJourney.Weapon.Continuous;

using System.Linq;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using EternalJourney.Common.StatusEffect;
using EternalJourney.Cores.Consts;
using EternalJourney.Enemy.Base;
using Godot;

/// <summary>
/// 継続ダメージ判定エリア（レーザー・放射武器の当たり判定）
/// ダメージのタイミングはロジックが決め、このクラスは重なっている敵へ効果を与えるだけ
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class ContinuousDamageArea : Area2D
{
    public override void _Notification(int what) => this.Notify(what);

    #region State
    // 以下の値は所有する武器（ContinuousWeapon）が設定する

    /// <summary>
    /// 1回あたりのダメージ（敵の耐久値に対する攻撃力）
    /// </summary>
    public float Atk { get; set; } = 1f;

    /// <summary>
    /// 1回で効果を与える最大の敵数（0以下は制限なし＝貫通）。近い敵から優先する
    /// </summary>
    public int MaxTargets { get; set; }

    /// <summary>
    /// 毒を付与するか
    /// </summary>
    public bool PoisonEnabled { get; set; }

    /// <summary>
    /// スタンを付与するか
    /// </summary>
    public bool StunEnabled { get; set; }
    #endregion State

    public void Setup()
    {
        // 何からも検知されず、敵だけを検知する
        CollisionLayer = 0;
        CollisionMask = CollisionEntity.Enemy;
        Monitorable = false;
        Monitoring = false;
    }

    /// <summary>
    /// 判定の有効・無効を切り替える
    /// </summary>
    public void SetActive(bool active)
    {
        // 物理クエリ中に呼ばれる可能性があるため遅延設定する
        SetDeferred(Area2D.PropertyName.Monitoring, active);
    }

    /// <summary>
    /// 重なっている敵へダメージと状態異常を与える
    /// </summary>
    /// <param name="multiplier">威力倍率（武器特性のチャージ等）</param>
    public void DealDamage(float multiplier = 1f)
    {
        var targets = GetOverlappingAreas()
            .Where(area => area is IBaseEnemy)
            .OrderBy(area => GlobalPosition.DistanceSquaredTo(area.GlobalPosition));

        foreach (Area2D area in MaxTargets > 0 ? targets.Take(MaxTargets) : targets)
        {
            if (area is not IBaseEnemy enemy)
            {
                continue;
            }

            enemy.TakeDamage(Atk * multiplier);
            if (PoisonEnabled)
            {
                enemy.StatusEffectReceiverManager.Get<PoisonEffect>()?.Apply();
            }
            if (StunEnabled)
            {
                enemy.StatusEffectReceiverManager.Get<StunEffect>()?.Apply();
            }
        }
    }
}
