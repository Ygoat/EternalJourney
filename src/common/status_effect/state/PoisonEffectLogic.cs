namespace EternalJourney.Common.StatusEffect.State;


using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

/// <summary>
/// 毒効果ロジックインターフェース
/// </summary>
public interface IPoisonEffectLogic : ILogicBlock<PoisonEffectLogic.State>;

/// <summary>
/// 毒効果ロジッククラス
/// </summary>
[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class PoisonEffectLogic : LogicBlock<PoisonEffectLogic.State>, IPoisonEffectLogic
{
    /// <summary>
    /// 初期状態
    /// </summary>
    /// <returns></returns>
    public override Transition GetInitialState() => To<State.InActive>();

    /// <summary>
    /// 入力定義
    /// </summary>
    public static class Input
    {
        /// <summary>
        /// 効果適用
        /// </summary>
        public readonly record struct Apply;

        /// <summary>
        /// 効果除去
        /// </summary>
        public readonly record struct Remove;
    }

    /// <summary>
    /// 出力定義
    /// </summary>
    public static class Output
    {
        /// <summary>
        /// 回復
        /// </summary>
        public readonly record struct Recovery;
    }

    /// <summary>
    /// 状態定義
    /// </summary>
    public abstract record State : StateLogic<State>
    {
        /// <summary>
        /// 未適用
        /// </summary>
        public record InActive : State, IGet<Input.Apply>
        {
            public InActive()
            {
            }

            public Transition On(in Input.Apply input) => To<Active>();
        }

        /// <summary>
        /// 適用
        /// </summary>
        public record Active : State, IGet<Input.Remove>, IGet<Input.Apply>
        {
            public Active()
            {
            }
            public Transition On(in Input.Apply input) => ToSelf();
            public Transition On(in Input.Remove input) => To<InActive>();
        }
    }
}

