namespace EternalJourney.Weapon;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

/// <summary>
/// 武器ロジックインターフェース
/// </summary>
public interface IWeaponLogic : ILogicBlock<WeaponLogic.State>;

/// <summary>
/// 武器ロジック
/// </summary>
[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class WeaponLogic : LogicBlock<WeaponLogic.State>, IWeaponLogic
{
    /// <summary>
    /// 初期状態
    /// </summary>
    /// <returns></returns>
    public override Transition GetInitialState() => To<State.Idle>();

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

    /// <summary>
    /// 状態定義
    /// </summary>
    public abstract record State : StateLogic<State>
    {
        /// <summary>
        /// スポーン待機
        /// </summary>
        public record Idle : State, IGet<Input.StartAttack>
        {
            public Idle()
            {
                this.OnEnter(() => Output(new Output.Idling()));
            }

            public Transition On(in Input.StartAttack input) => To<Attack>();
        }

        /// <summary>
        /// 攻撃
        /// </summary>
        public record Attack : State, IGet<Input.StartIdle>
        {
            public Attack()
            {
                this.OnEnter(() => Output(new Output.Attacking()));
            }

            public Transition On(in Input.StartIdle input) => To<Idle>();
        }
    }
}
