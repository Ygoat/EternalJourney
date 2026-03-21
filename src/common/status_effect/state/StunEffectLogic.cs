namespace EternalJourney.Common.StatusEffect.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

/// <summary>
/// スタン効果ロジックインターフェース
/// </summary>
public interface IStunEffectLogic : ILogicBlock<StunEffectLogic.State>;

/// <summary>
/// スタン効果ロジッククラス
/// </summary>
[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class StunEffectLogic : LogicBlock<StunEffectLogic.State>, IStunEffectLogic
{
    public override Transition GetInitialState() => To<State.InActive>();

    public static class Input
    {
        /// <summary>
        /// 効果適用（重ねがけも含む）
        /// </summary>
        public readonly record struct Apply;

        /// <summary>
        /// 効果除去
        /// </summary>
        public readonly record struct Remove;
    }

    public static class Output
    {
        /// <summary>
        /// スタン開始
        /// </summary>
        public readonly record struct Activated;

        /// <summary>
        /// スタン終了
        /// </summary>
        public readonly record struct Deactivated;
    }

    public abstract record State : StateLogic<State>
    {
        /// <summary>
        /// 未適用
        /// </summary>
        public record InActive : State, IGet<Input.Apply>
        {
            public InActive() { }

            public Transition On(in Input.Apply input) => To<Active>();
        }

        /// <summary>
        /// スタン中
        /// </summary>
        public record Active : State, IGet<Input.Apply>, IGet<Input.Remove>
        {
            public Active()
            {
                this.OnEnter(() => Output(new Output.Activated()));
                this.OnExit(() => Output(new Output.Deactivated()));
            }

            // 重ねがけ：タイマーリセットはStunEffect側のWatchで処理
            public Transition On(in Input.Apply input) => ToSelf();

            public Transition On(in Input.Remove input) => To<InActive>();
        }
    }
}
