namespace EternalJourney.Ship.State;

using System;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using Chickensoft.Sync.Primitives;
using EternalJourney.Battle.Domain;
using EternalJourney.Bullet.Base;
using EternalJourney.Cores.Models.Research;
using EternalJourney.Enemy.Base;
using EternalJourney.Ship;
using Godot;

/// <summary>
/// 船ロジックインターフェース
/// </summary>
public interface IShipLogic : ILogicBlock;

/// <summary>
/// 船ロジック
/// </summary>
[Meta]
public partial class ShipLogic : LogicBlock, IShipLogic
{
    public ShipLogic()
    {
        // ステートを事前確保
        Set(new ShipState.Alive());
    }


    /// <summary>
    /// 入力定義
    /// </summary>
    public static class Input
    {
        /// <summary>
        /// 被弾
        /// </summary>
        public readonly record struct Damaged(float Damage);

        /// <summary>
        /// HP回復
        /// </summary>
        public readonly record struct Healed(float Amount);
    }

    /// <summary>
    /// 出力定義
    /// </summary>
    public static class Output
    {
        /// <summary>
        /// HP変化
        /// </summary>
        public readonly record struct HpChanged(float CurrentHp, float MaxHp);

        /// <summary>
        /// 撃沈
        /// </summary>
        public readonly record struct Dead;
    }

}

/// <summary>
/// 状態定義
/// </summary>
[StateDiagram]
public abstract partial record ShipState : LogicBlockState
{
    /// <summary>
    /// 生存状態
    /// </summary>
    public record Alive : ShipState, IGet<ShipLogic.Input.Damaged>, IGet<ShipLogic.Input.Healed>
    {
        /// <summary>
        /// 研究効果の購読用バインディング
        /// </summary>
        private AutoValue<ResearchBonus>.Binding? _researchBinding;

        /// <summary>
        /// 適用済みの研究による最大HP加算値（OnEnterで初期化する）
        /// </summary>
        private float _appliedMaxHpBonus;

        /// <summary>
        /// 研究による防御加算値（OnEnterで初期化する）
        /// </summary>
        private float _researchDef;

        public Alive()
        {
            this.OnEnter(() =>
            {
                IShip ship = Get<IShip>();
                IBattleRepo battleRepo = Get<IBattleRepo>();
                _appliedMaxHpBonus = 0f;
                _researchDef = 0f;
                ship.AreaEntered += OnAreaEntered;
                battleRepo.EnemyDestroyed += OnEnemyDestroyed;
                battleRepo.ShipHealRequested += OnHealRequested;
                // 購読登録時に現在値が即時通知される
                _researchBinding = battleRepo.Research.Bind();
                _researchBinding.OnValue(OnResearchChanged);
            });
            this.OnExit(() =>
            {
                IShip ship = Get<IShip>();
                IBattleRepo battleRepo = Get<IBattleRepo>();
                ship.AreaEntered -= OnAreaEntered;
                battleRepo.EnemyDestroyed -= OnEnemyDestroyed;
                battleRepo.ShipHealRequested -= OnHealRequested;
                _researchBinding?.Dispose();
                _researchBinding = null;
            });
        }

        /// <summary>
        /// 研究効果の変化：最大HPは前回からの増加分だけ最大値・現在値を増やす
        /// </summary>
        private void OnResearchChanged(ResearchBonus bonus)
        {
            _researchDef = bonus.ShipDef;
            float diff = bonus.ShipMaxHp - _appliedMaxHpBonus;
            if (diff <= 0f)
            {
                return;
            }
            _appliedMaxHpBonus = bonus.ShipMaxHp;
            IShip ship = Get<IShip>();
            ship.Status.MaxDur += diff;
            ship.Status.CurrentDur += diff;
            Output(new ShipLogic.Output.HpChanged(ship.Status.CurrentDur, ship.Status.MaxDur));
        }

        public Type On(in ShipLogic.Input.Damaged input)
        {
            ApplyDamage(input.Damage);
            return ToSelf();
        }

        public Type On(in ShipLogic.Input.Healed input)
        {
            ApplyHeal(input.Amount);
            return ToSelf();
        }

        private void OnAreaEntered(Area2D area)
        {
            if (area is IBaseBullet bullet)
            {
                IBattleRepo battleRepo = Get<IBattleRepo>();
                IShip ship = Get<IShip>();
                float shipDef = ship.Status.Def + battleRepo.ShipDefBonus + _researchDef;
                float reducedDamage = bullet.Status.Atk * BattleRepo.ENEMY_BASE_ATK_MULTIPLIER * battleRepo.EnemyAtkMultiplier * (100f / (100f + shipDef));
                ApplyDamage(reducedDamage);
            }
        }

        private void OnEnemyDestroyed(IBaseEnemy _)
        {
            ApplyHeal(10f);
        }

        private void OnHealRequested(float amount)
        {
            ApplyHeal(amount);
        }

        private bool _isDead = false;

        private void ApplyDamage(float damage)
        {
            IShip ship = Get<IShip>();
            ship.Status.CurrentDur = Math.Max(0f, ship.Status.CurrentDur - damage);
            Output(new ShipLogic.Output.HpChanged(ship.Status.CurrentDur, ship.Status.MaxDur));
            if (ship.Status.CurrentDur <= 0f && !_isDead)
            {
                _isDead = true;
                Output(new ShipLogic.Output.Dead());
            }
        }

        private void ApplyHeal(float amount)
        {
            IShip ship = Get<IShip>();
            ship.Status.CurrentDur = Math.Min(ship.Status.MaxDur, ship.Status.CurrentDur + amount);
            Output(new ShipLogic.Output.HpChanged(ship.Status.CurrentDur, ship.Status.MaxDur));
        }
    }
}
