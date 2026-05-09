namespace EternalJourney.Bullet.Abstract.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

/// <summary>
/// スタンダード弾丸ファクトリロジックインターフェース
/// </summary>
public interface IStandardBulletFactoryLogic : ILogicBlock<StandardBulletFactoryLogic.State>;

/// <summary>
/// スタンダード弾丸ファクトリロジック
/// Ready → CoolDown → Ready のサイクルで弾丸射出とクールダウンを管理
/// </summary>
[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class StandardBulletFactoryLogic : LogicBlock<StandardBulletFactoryLogic.State>, IStandardBulletFactoryLogic
{
    public override Transition GetInitialState() => To<State.Ready>();

    public static class Input
    {
        /// <summary>
        /// 発射リクエスト
        /// </summary>
        public readonly record struct FireRequested;

        /// <summary>
        /// クールダウン完了
        /// </summary>
        public readonly record struct CoolDownComplete;
    }

    public static class Output
    {
        /// <summary>
        /// 発射
        /// </summary>
        public readonly record struct Fire;

        /// <summary>
        /// クールダウン開始
        /// </summary>
        public readonly record struct StartCoolDown;
    }

    public abstract record State : StateLogic<State>
    {
        /// <summary>
        /// 発射可能状態
        /// </summary>
        public record Ready : State, IGet<Input.FireRequested>
        {
            public Transition On(in Input.FireRequested input)
            {
                Output(new Output.Fire());
                return To<CoolDown>();
            }
        }

        /// <summary>
        /// クールダウン状態
        /// </summary>
        public record CoolDown : State, IGet<Input.CoolDownComplete>
        {
            public CoolDown()
            {
                this.OnEnter(() => Output(new Output.StartCoolDown()));
            }

            public Transition On(in Input.CoolDownComplete input) => To<Ready>();
        }
    }
}
