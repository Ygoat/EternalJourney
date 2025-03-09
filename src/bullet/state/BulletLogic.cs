namespace EternalJourney.App.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

/// <summary>
/// 弾丸ロジックインターフェース
/// </summary>
public interface IBulletLogic : ILogicBlock<BulletLogic.State>;

/// <summary>
/// 弾丸ロジック
/// </summary>
[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class BulletLogic : LogicBlock<BulletLogic.State>, IBulletLogic
{
    /// <summary>
    /// 初期状態
    /// </summary>
    /// <returns></returns>
    public override Transition GetInitialState() => To<State.Loaded>();

    /// <summary>
    /// 入力定義
    /// </summary>
    public static class Input
    {
        /// <summary>
        /// 射撃
        /// </summary>
        public readonly record struct Fire;

        /// <summary>
        /// ヒット
        /// </summary>
        public readonly record struct Hit;

        /// <summary>
        /// 崩壊
        /// </summary>
        public readonly record struct Collapse;

        /// <summary>
        /// リロード
        /// </summary>
        public readonly record struct Reload;

        /// <summary>
        /// 貫通
        /// </summary>
        public readonly record struct Penetrate;

        /// <summary>
        /// ミス
        /// </summary>
        public readonly record struct Miss;
    }

    /// <summary>
    /// 出力定義
    /// </summary>
    public static class Output
    {
        /// <summary>
        /// 射出
        /// </summary>
        public readonly record struct Emitted;

        /// <summary>
        /// 劣化
        /// </summary>
        public readonly record struct Decay;

        /// <summary>
        /// 消失
        /// </summary>
        public readonly record struct Disappear;
    }

    /// <summary>
    /// 状態定義
    /// </summary>
    public abstract record State : StateLogic<State>
    {
        /// <summary>
        /// ロード
        /// </summary>
        public record Loaded : State, IGet<Input.Fire>
        {
            public Loaded()
            {
            }

            public Transition On(in Input.Fire input) => To<InFlight>();
        }

        /// <summary>
        /// 飛翔
        /// </summary>
        public record InFlight : State, IGet<Input.Hit>, IGet<Input.Miss>
        {
            public InFlight()
            {
                this.OnEnter(() => Output(new Output.Emitted()));
            }

            public Transition On(in Input.Hit input) => To<Hitting>();

            public Transition On(in Input.Miss input) => To<Destroy>();
        }

        /// <summary>
        /// ヒット
        /// </summary>
        public record Hitting : State, IGet<Input.Collapse>, IGet<Input.Penetrate>
        {
            public Hitting()
            {
                this.OnEnter(() => Output(new Output.Decay()));
            }

            public Transition On(in Input.Collapse input) => To<Destroy>();

            public Transition On(in Input.Penetrate input) => To<InFlight>();
        }

        /// <summary>
        /// 破壊
        /// </summary>
        public record Destroy : State, IGet<Input.Reload>
        {
            public Destroy()
            {
                this.OnEnter(() => Output(new Output.Disappear()));
            }

            public Transition On(in Input.Reload input) => To<Loaded>();
        }
    }
}
