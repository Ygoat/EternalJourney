namespace EternalJourney.Skills.Regen.State;

using System;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

/// <summary>
/// リジェネスキルロジックインターフェース
/// </summary>
public interface IRegenSkillLogic : ILogicBlock;

/// <summary>
/// リジェネスキルロジック（毎秒固定HP回復）
/// </summary>
[Meta]
public partial class RegenSkillLogic : LogicBlock, IRegenSkillLogic
{
    public RegenSkillLogic()
    {
        // ステートを事前確保
        Set(new RegenSkillState.InActive());
        Set(new RegenSkillState.Active());
    }


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

}

[StateDiagram]
public abstract partial record RegenSkillState : LogicBlockState
{
    /// <summary>未発動</summary>
    public record InActive : RegenSkillState, IGet<RegenSkillLogic.Input.Apply>
    {
        public Type On(in RegenSkillLogic.Input.Apply input) => To<Active>();
    }

    /// <summary>リジェネ中（スタック最大3）</summary>
    public record Active : RegenSkillState, IGet<RegenSkillLogic.Input.Apply>, IGet<RegenSkillLogic.Input.Tick>, IGet<RegenSkillLogic.Input.Remove>
    {
        /// <summary>スタック数</summary>
        public int StackCount { get; set; } = 1;

        private const int MaxStacks = 5;
        private const float BaseHealPerTick = 50f;
        private const float HealPerStack = 50f;

        public Active()
        {
            this.OnEnter(() =>
            {
                // 事前確保された状態インスタンスが再利用されるため、進入時にスタックを初期化
                StackCount = 1;
                Output(new RegenSkillLogic.Output.Activated(CalcHeal()));
            });
            this.OnExit(() => Output(new RegenSkillLogic.Output.Deactivated()));
        }

        public Type On(in RegenSkillLogic.Input.Apply input)
        {
            if (StackCount < MaxStacks)
            { StackCount++; }
            Output(new RegenSkillLogic.Output.Activated(CalcHeal()));
            return ToSelf();
        }

        public Type On(in RegenSkillLogic.Input.Tick input)
        {
            Output(new RegenSkillLogic.Output.TikHeal(CalcHeal()));
            return ToSelf();
        }

        public Type On(in RegenSkillLogic.Input.Remove input)
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
