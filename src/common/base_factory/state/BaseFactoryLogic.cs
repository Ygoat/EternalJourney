namespace EternalJourney.Common.BaseFactory.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

/// <summary>
/// ベースファクトリロジックインターフェース
/// </summary>
public interface IBaseFactoryLogic : ILogicBlock<BaseFactoryLogic.State>;

/// <summary>
/// ベースファクトリロジック
/// オブジェクトプールを使用したファクトリの共通状態遷移を管理します
/// Ready → Generate → CoolDown → Ready のサイクルで動作
/// </summary>
[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class BaseFactoryLogic : LogicBlock<BaseFactoryLogic.State>, IBaseFactoryLogic
{
    /// <summary>
    /// 初期状態
    /// </summary>
    /// <returns></returns>
    public override Transition GetInitialState() => To<State.Ready>();

    /// <summary>
    /// 入力定義
    /// </summary>
    public static class Input
    {
        /// <summary>
        /// 生成リクエスト
        /// </summary>
        public readonly record struct Generate;

        /// <summary>
        /// クールダウン開始
        /// </summary>
        public readonly record struct StartCoolDown;

        /// <summary>
        /// クールダウン完了
        /// </summary>
        public readonly record struct CoolDownComplete;
    }

    /// <summary>
    /// 出力定義
    /// </summary>
    public static class Output
    {
        /// <summary>
        /// 生成完了
        /// </summary>
        public readonly record struct Generated;

        /// <summary>
        /// クールダウン中
        /// </summary>
        public readonly record struct Cooling;
    }

    /// <summary>
    /// 状態定義
    /// </summary>
    public abstract record State : StateLogic<State>
    {
        /// <summary>
        /// 生成可能状態
        /// </summary>
        public record Ready : State, IGet<Input.Generate>
        {
            public Ready()
            {
            }

            public Transition On(in Input.Generate input) => To<Generating>();
        }

        /// <summary>
        /// 生成中状態
        /// </summary>
        public record Generating : State, IGet<Input.StartCoolDown>
        {
            public Generating()
            {
                this.OnEnter(() => Output(new Output.Generated()));
            }

            public Transition On(in Input.StartCoolDown input) => To<CoolDown>();
        }

        /// <summary>
        /// クールダウン状態
        /// </summary>
        public record CoolDown : State, IGet<Input.CoolDownComplete>
        {
            public CoolDown()
            {
                this.OnEnter(() => Output(new Output.Cooling()));
            }

            public Transition On(in Input.CoolDownComplete input) => To<Ready>();
        }
    }
}
