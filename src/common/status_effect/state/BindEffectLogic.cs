namespace EternalJourney.Common.StatusEffect.State;

using System;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

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
        /// 解除
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
            public InActive()
            {
            }
            public Transition On(in Input.Apply input) => To<Active>();
        }

        /// <summary>
        /// 適用
        /// </summary>
        public record Active(float RemainTime) : State, IGet<Input.Remove>, IGet<Input.Apply>
        {
            // 再適用時は持続時間をリセット
            public Transition On(in Input.Apply input) => To<Active>();

            // 解除時は未適用へ
            public Transition On(in Input.Remove input) => To<InActive>();
        }
    }
}
