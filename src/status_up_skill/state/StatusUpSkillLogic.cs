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

    public static class Input
    {
        /// <summary>
        /// スキル発動
        /// </summary>
        public readonly record struct Apply;

        /// <summary>
        /// バフ解除
        /// </summary>
        public readonly record struct Remove;
    }

    public static class Output
    {
        /// <summary>
        /// バフ開始（乗数を含む）
        /// </summary>
        public readonly record struct Activated(float AtkMultiplier, float SpdMultiplier);

        /// <summary>
        /// バフ終了
        /// </summary>
        public readonly record struct Deactivated;
    }

    public abstract record State : StateLogic<State>
    {
        /// <summary>
        /// 未発動
        /// </summary>
        public record InActive : State, IGet<Input.Apply>
        {
            public InActive() { }

            public Transition On(in Input.Apply input) => To<Active>();
        }

        /// <summary>
        /// バフ中
        /// </summary>
        public record Active : State, IGet<Input.Apply>, IGet<Input.Remove>
        {
            /// <summary>スタック数（最大3）</summary>
            public int StackCount { get; set; } = 1;

            private const int MaxStacks = 3;
            private const float BaseMultiplier = 1.0f;
            private const float MultiplierPerStack = 0.5f;

            public Active()
            {
                this.OnEnter(() => Output(new Output.Activated(CalcMultiplier(), CalcMultiplier())));
                this.OnExit(() => Output(new Output.Deactivated()));
            }

            public Transition On(in Input.Apply input)
            {
                if (StackCount < MaxStacks)
                {
                    StackCount++;
                }
                // スタック更新をOutput（タイマーリセットもノード側で実施）
                Output(new Output.Activated(CalcMultiplier(), CalcMultiplier()));
                return ToSelf();
            }

            public Transition On(in Input.Remove input)
            {
                if (StackCount > 1)
                {
                    StackCount--;
                    Output(new Output.Activated(CalcMultiplier(), CalcMultiplier()));
                    return ToSelf();
                }
                return To<InActive>();
            }

            private float CalcMultiplier() => BaseMultiplier + StackCount * MultiplierPerStack;
        }
    }
}
