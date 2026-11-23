namespace EternalJourney.Skills.StatusUp.State;

using System;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

/// <summary>
/// ステータスアップスキルロジックインターフェース
/// </summary>
public interface IStatusUpSkillLogic : ILogicBlock;

/// <summary>
/// ステータスアップスキルロジッククラス
/// </summary>
[Meta]
public partial class StatusUpSkillLogic : LogicBlock, IStatusUpSkillLogic
{
    public StatusUpSkillLogic()
    {
        // ステートを事前確保
        Set(new StatusUpSkillState.InActive());
        Set(new StatusUpSkillState.Active());
    }


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

}

[StateDiagram]
public abstract partial record StatusUpSkillState : LogicBlockState
{
    /// <summary>未発動</summary>
    public record InActive : StatusUpSkillState, IGet<StatusUpSkillLogic.Input.Apply>
    {
        public Type On(in StatusUpSkillLogic.Input.Apply input)
        {
            Get<StatusUpSkillLogic.StackState>().Count = 1;
            return To<Active>();
        }
    }

    /// <summary>バフ中（スタック最大3）</summary>
    public record Active : StatusUpSkillState, IGet<StatusUpSkillLogic.Input.Apply>, IGet<StatusUpSkillLogic.Input.Remove>
    {
        private const int MaxStacks = 3;

        public Active()
        {
            this.OnEnter(() => Output(new StatusUpSkillLogic.Output.Activated(Get<StatusUpSkillLogic.StackState>().Count)));
            this.OnExit(() => Output(new StatusUpSkillLogic.Output.Deactivated()));
        }

        public Type On(in StatusUpSkillLogic.Input.Apply input)
        {
            var stack = Get<StatusUpSkillLogic.StackState>();
            if (stack.Count < MaxStacks) { stack.Count++; }
            Output(new StatusUpSkillLogic.Output.Activated(stack.Count));
            return ToSelf();
        }

        public Type On(in StatusUpSkillLogic.Input.Remove input)
        {
            var stack = Get<StatusUpSkillLogic.StackState>();
            if (stack.Count > 1)
            {
                stack.Count--;
                Output(new StatusUpSkillLogic.Output.Activated(stack.Count));
                return ToSelf();
            }
            return To<InActive>();
        }
    }
}
