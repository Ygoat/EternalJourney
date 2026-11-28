namespace EternalJourney.Weapon.Standard.State;

using System;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

/// <summary>
/// 武器ロジックインターフェース
/// </summary>
public interface IStandardWeaponLogic : ILogicBlock;

/// <summary>
/// 武器ロジック
/// </summary>
[Meta]
public partial class StandardWeaponLogic : LogicBlock, IStandardWeaponLogic
{
    public StandardWeaponLogic()
    {
        // ステートを事前確保
        Set(new StandardWeaponState.Idle());
        Set(new StandardWeaponState.Attack());
    }


    /// <summary>
    /// 入力定義
    /// </summary>
    public static class Input
    {
        /// <summary>
        /// アイドル開始
        /// </summary>
        public readonly record struct StartIdle;

        /// <summary>
        /// 攻撃開始
        /// </summary>
        public readonly record struct StartAttack;
    }

    /// <summary>
    /// 出力定義
    /// </summary>
    public static class Output
    {
        /// <summary>
        /// 発見
        /// </summary>
        public readonly record struct Idling;

        /// <summary>
        /// 射撃
        /// </summary>
        public readonly record struct Attacking;
    }

}

/// <summary>
/// 状態定義
/// </summary>
[StateDiagram]
public abstract partial record StandardWeaponState : LogicBlockState
{
    /// <summary>
    /// スポーン待機
    /// </summary>
    public record Idle : StandardWeaponState, IGet<StandardWeaponLogic.Input.StartAttack>
    {
        public Idle()
        {
            this.OnEnter(() => Output(new StandardWeaponLogic.Output.Idling()));
        }

        public Type On(in StandardWeaponLogic.Input.StartAttack input) => To<Attack>();
    }

    /// <summary>
    /// 攻撃
    /// </summary>
    public record Attack : StandardWeaponState, IGet<StandardWeaponLogic.Input.StartIdle>
    {
        public Attack()
        {
            this.OnEnter(() => Output(new StandardWeaponLogic.Output.Attacking()));
        }

        public Type On(in StandardWeaponLogic.Input.StartIdle input) => To<Idle>();
    }
}
