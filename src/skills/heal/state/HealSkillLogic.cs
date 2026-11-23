namespace EternalJourney.Skills.Heal.State;

using System;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

/// <summary>
/// 回復スキルロジックインターフェース
/// </summary>
public interface IHealSkillLogic : ILogicBlock;

/// <summary>
/// 回復スキルロジック
/// </summary>
[Meta]
public partial class HealSkillLogic : LogicBlock, IHealSkillLogic
{
    public HealSkillLogic()
    {
        // ステートを事前確保
        Set(new HealSkillState.Ready());
    }


    public static class Input
    {
        /// <summary>
        /// スキル発動
        /// </summary>
        public readonly record struct Apply;
    }

    public static class Output
    {
        /// <summary>
        /// 回復量
        /// </summary>
        public readonly record struct Healed(float Amount);
    }

}

[StateDiagram]
public abstract partial record HealSkillState : LogicBlockState
{
    private const float HealAmount = 100f;

    /// <summary>
    /// 待機
    /// </summary>
    public record Ready : HealSkillState, IGet<HealSkillLogic.Input.Apply>
    {
        public Ready() { }

        public Type On(in HealSkillLogic.Input.Apply input)
        {
            Output(new HealSkillLogic.Output.Healed(HealAmount));
            return ToSelf();
        }
    }
}
