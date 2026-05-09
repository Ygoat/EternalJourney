namespace EternalJourney.Skills.Heal.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

/// <summary>
/// 回復スキルロジックインターフェース
/// </summary>
public interface IHealSkillLogic : ILogicBlock<HealSkillLogic.State>;

/// <summary>
/// 回復スキルロジック
/// </summary>
[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class HealSkillLogic : LogicBlock<HealSkillLogic.State>, IHealSkillLogic
{
    public override Transition GetInitialState() => To<State.Ready>();

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

    public abstract record State : StateLogic<State>
    {
        private const float HealAmount = 100f;

        /// <summary>
        /// 待機
        /// </summary>
        public record Ready : State, IGet<Input.Apply>
        {
            public Ready() { }

            public Transition On(in Input.Apply input)
            {
                Output(new Output.Healed(HealAmount));
                return ToSelf();
            }
        }
    }
}
