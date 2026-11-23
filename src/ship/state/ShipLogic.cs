namespace EternalJourney.Ship.State;

using System;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Battle.Domain;
using EternalJourney.Bullet.Abstract.Base;
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
        public Alive()
        {
            this.OnEnter(() =>
            {
                IShip ship = Get<IShip>();
                IBattleRepo battleRepo = Get<IBattleRepo>();
                ship.AreaEntered += OnAreaEntered;
                battleRepo.EnemyDestroyed += OnEnemyDestroyed;
                battleRepo.ShipHealRequested += OnHealRequested;
            });
            this.OnExit(() =>
            {
                IShip ship = Get<IShip>();
                IBattleRepo battleRepo = Get<IBattleRepo>();
                ship.AreaEntered -= OnAreaEntered;
                battleRepo.EnemyDestroyed -= OnEnemyDestroyed;
                battleRepo.ShipHealRequested -= OnHealRequested;
            });
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
                float shipDef = ship.Status.Def + battleRepo.ShipDefBonus;
                float reducedDamage = bullet.Status.Atk * (100f / (100f + shipDef));
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
