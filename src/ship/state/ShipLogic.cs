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
public interface IShipLogic : ILogicBlock<ShipLogic.State>;

/// <summary>
/// 船ロジック
/// </summary>
[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class ShipLogic : LogicBlock<ShipLogic.State>, IShipLogic
{
    /// <summary>
    /// 初期状態
    /// </summary>
    public override Transition GetInitialState() => To<State.Alive>();

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

    /// <summary>
    /// 状態定義
    /// </summary>
    public abstract record State : StateLogic<State>
    {
        /// <summary>
        /// 生存状態
        /// </summary>
        public record Alive : State, IGet<Input.Damaged>, IGet<Input.Healed>
        {
            public Alive()
            {
                OnAttach(() =>
                {
                    IShip ship = Get<IShip>();
                    IBattleRepo battleRepo = Get<IBattleRepo>();
                    ship.AreaEntered += OnAreaEntered;
                    battleRepo.EnemyDestroyed += OnEnemyDestroyed;
                });

                OnDetach(() =>
                {
                    IShip ship = Get<IShip>();
                    IBattleRepo battleRepo = Get<IBattleRepo>();
                    ship.AreaEntered -= OnAreaEntered;
                    battleRepo.EnemyDestroyed -= OnEnemyDestroyed;
                });
            }

            public Transition On(in Input.Damaged input)
            {
                ApplyDamage(input.Damage);
                return ToSelf();
            }

            public Transition On(in Input.Healed input)
            {
                ApplyHeal(input.Amount);
                return ToSelf();
            }

            private void OnAreaEntered(Area2D area)
            {
                if (area is IBaseBullet bullet)
                {
                    ApplyDamage(bullet.Status.Atk);
                }
            }

            private void OnEnemyDestroyed(IBaseEnemy _)
            {
                ApplyHeal(10f);
            }

            private bool _isDead = false;

            private void ApplyDamage(float damage)
            {
                IShip ship = Get<IShip>();
                ship.Status.CurrentDur = Math.Max(0f, ship.Status.CurrentDur - damage);
                Output(new Output.HpChanged(ship.Status.CurrentDur, ship.Status.MaxDur));
                if (ship.Status.CurrentDur <= 0f && !_isDead)
                {
                    _isDead = true;
                    Output(new Output.Dead());
                }
            }

            private void ApplyHeal(float amount)
            {
                IShip ship = Get<IShip>();
                ship.Status.CurrentDur = Math.Min(ship.Status.MaxDur, ship.Status.CurrentDur + amount);
                Output(new Output.HpChanged(ship.Status.CurrentDur, ship.Status.MaxDur));
            }
        }
    }
}
