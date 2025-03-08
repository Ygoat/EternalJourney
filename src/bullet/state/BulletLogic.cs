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
        /// スプラッシュ終了
        /// </summary>
        public readonly record struct Fire;

        /// <summary>
        /// ゲームスタート
        /// </summary>
        public readonly record struct Hit;

        /// <summary>
        /// フェードアウト完了
        /// </summary>
        public readonly record struct Collapse;
        public readonly record struct Reload;

        public readonly record struct Penetrate;
    }

    /// <summary>
    /// 出力定義
    /// </summary>
    public static class Output
    {
        public readonly record struct Emitted;
        public readonly record struct Decay;
        public readonly record struct Disappear;
    }

    // 不必要なヒープの割り当てを減らすために、入力と出力は読み取り専用のレコード構造体（readonly record struct）にすべき

    /// <summary>
    /// 状態定義
    /// </summary>
    public abstract record State : StateLogic<State>
    {
        /// <summary>
        /// ロード状態
        /// </summary>
        public record Loaded : State, IGet<Input.Fire>
        {
            public Loaded()
            {
            }

            /// <summary>
            /// 状態の遷移を定義
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            public Transition On(in Input.Fire input) => To<InFlight>();
        }

        /// <summary>
        /// メインメニュー
        /// </summary>
        public record InFlight : State, IGet<Input.Hit>//, IGet<Input.LoadGame>
        {
            public InFlight()
            {
                this.OnEnter(() => Output(new Output.Emitted()));
            }

            public Transition On(in Input.Hit input) => To<Hitting>();
        }

        public record Hitting : State, IGet<Input.Collapse>, IGet<Input.Penetrate>
        {
            public Hitting()
            {
                this.OnEnter(() => Output(new Output.Decay()));
            }

            public Transition On(in Input.Collapse input) => To<Destroy>();

            public Transition On(in Input.Penetrate input) => To<InFlight>();
        }

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
