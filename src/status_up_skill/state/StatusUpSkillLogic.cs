namespace EternalJourney.StatusUpSkill.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

/// <summary>
/// ステータスアップスキルロジックインターフェース
/// </summary>
public interface IStatusUpSkillLogic : ILogicBlock<StatusUpSkillLogic.State>;

/// <summary>
/// ステータスアップスキルロジッククラス
/// </summary>
[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class StatusUpSkillLogic : LogicBlock<StatusUpSkillLogic.State>, IStatusUpSkillLogic
{
    public override Transition GetInitialState() => To<State.InActive>();

    /// <summary>スタック数を保持するblackboardオブジェクト</summary>
    public class StackState { public int Count { get; set; } = 1; }

    public static class Input
    {
        /// <summary>スキル発動</summary>
        public readonly record struct Apply;

        /// <summary>バフ解除</summary>
        public readonly record struct Remove;
    }

    public static class Output
    {
        /// <summary>バフ開始（スタック数を含む）</summary>
        public readonly record struct Activated(int StackCount);

        /// <summary>バフ終了</summary>
        public readonly record struct Deactivated;
    }

    public abstract record State : StateLogic<State>
    {
        /// <summary>未発動</summary>
        public record InActive : State, IGet<Input.Apply>
        {
            public Transition On(in Input.Apply input)
            {
                Get<StackState>().Count = 1;
                return To<Active>();
            }
        }

        /// <summary>バフ中（スタック最大3）</summary>
        public record Active : State, IGet<Input.Apply>, IGet<Input.Remove>
        {
            private const int MaxStacks = 3;

            public Active()
            {
                this.OnEnter(() => Output(new Output.Activated(Get<StackState>().Count)));
                this.OnExit(() => Output(new Output.Deactivated()));
            }

            public Transition On(in Input.Apply input)
            {
                var stack = Get<StackState>();
                if (stack.Count < MaxStacks) { stack.Count++; }
                Output(new Output.Activated(stack.Count));
                return ToSelf();
            }

            public Transition On(in Input.Remove input)
            {
                var stack = Get<StackState>();
                if (stack.Count > 1)
                {
                    stack.Count--;
                    Output(new Output.Activated(stack.Count));
                    return ToSelf();
                }
                return To<InActive>();
            }
        }
    }
}
