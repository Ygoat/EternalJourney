namespace EternalJourney.Skills.Regen.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

/// <summary>
/// リジェネスキルロジックインターフェース
/// </summary>
public interface IRegenSkillLogic : ILogicBlock<RegenSkillLogic.State>;

/// <summary>
/// リジェネスキルロジック（毎秒固定HP回復）
/// </summary>
[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class RegenSkillLogic : LogicBlock<RegenSkillLogic.State>, IRegenSkillLogic
{
    public override Transition GetInitialState() => To<State.InActive>();

    public static class Input
    {
        /// <summary>スキル発動</summary>
        public readonly record struct Apply;

        /// <summary>1秒ティック</summary>
        public readonly record struct Tick;

        /// <summary>バフ解除</summary>
        public readonly record struct Remove;
    }

    public static class Output
    {
        /// <summary>バフ開始・更新（タイマーリセット用）</summary>
        public readonly record struct Activated(float HealPerTick);

        /// <summary>毎秒回復</summary>
        public readonly record struct TikHeal(float Amount);

        /// <summary>バフ終了</summary>
        public readonly record struct Deactivated;
    }

    public abstract record State : StateLogic<State>
    {
        /// <summary>未発動</summary>
        public record InActive : State, IGet<Input.Apply>
        {
            public Transition On(in Input.Apply input) => To<Active>();
        }

        /// <summary>リジェネ中（スタック最大3）</summary>
        public record Active : State, IGet<Input.Apply>, IGet<Input.Tick>, IGet<Input.Remove>
        {
            /// <summary>スタック数</summary>
            public int StackCount { get; set; } = 1;

            private const int MaxStacks = 5;
            private const float BaseHealPerTick = 50f;
            private const float HealPerStack = 50f;

            public Active()
            {
                this.OnEnter(() => Output(new Output.Activated(CalcHeal())));
                this.OnExit(() => Output(new Output.Deactivated()));
            }

            public Transition On(in Input.Apply input)
            {
                if (StackCount < MaxStacks)
                { StackCount++; }
                Output(new Output.Activated(CalcHeal()));
                return ToSelf();
            }

            public Transition On(in Input.Tick input)
            {
                Output(new Output.TikHeal(CalcHeal()));
                return ToSelf();
            }

            public Transition On(in Input.Remove input)
            {
                if (StackCount > 1)
                {
                    StackCount--;
                    return ToSelf();
                }
                return To<InActive>();
            }

            private float CalcHeal() => BaseHealPerTick + StackCount * HealPerStack;
        }
    }
}
