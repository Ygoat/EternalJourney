namespace EternalJourney.Common.StatusEffect.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Battle.Domain;

/// <summary>
/// バインド効果ロジックインターフェース
/// </summary>
public interface IBindEffectLogic : ILogicBlock<BindEffectLogic.State>;

/// <summary>
/// バインド効果ロジッククラス
/// </summary>
[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class BindEffectLogic : LogicBlock<BindEffectLogic.State>, IBindEffectLogic
{
    public override Transition GetInitialState() => To<State.InActive>();

    /// <summary>
    /// 入力定義
    /// </summary>
    public static class Input
    {
        /// <summary>
        /// 効果適用（持続時間付き）
        /// </summary>
        public readonly record struct Apply(float Duration);

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
        /// バインド発動
        /// </summary>
        public readonly record struct Binded(float Duration);

        /// <summary>
        /// バインド解除
        /// </summary>
        public readonly record struct Released;
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
            public Transition On(in Input.Apply input)
            {
                Output(new Output.Binded(input.Duration));
                return To<Active>(input.Duration);
            }
        }

        /// <summary>
        /// 適用
        /// </summary>
        public record Active(float RemainTime) : State, IGet<Input.Remove>, IGet<Input.Apply>, ITickable
        {
            public Transition On(in Input.Apply input)
            {
                // 再適用時はタイマーリセット
                Output(new Output.Binded(input.Duration));
                return To<Active>(input.Duration);
            }

            public Transition On(in Input.Remove input)
            {
                Output(new Output.Released());
                return To<InActive>();
            }

            public Transition OnTick(float delta)
            {
                float next = RemainTime - delta;
                if (next <= 0)
                {
                    Output(new Output.Released());
                    return To<InActive>();
                }
                return To<Active>(next);
            }
        }
    }
}
