namespace EternalJourney.BulletFactory.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

/// <summary>
/// 弾丸ファクトリロジックインターフェース
/// </summary>
public interface IBulletFactoryLogic : ILogicBlock<BulletFactoryLogic.State>;

/// <summary>
/// 弾丸ファクトリロジック
/// </summary>
[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class BulletFactoryLogic : LogicBlock<BulletFactoryLogic.State>, IBulletFactoryLogic
{
    /// <summary>
    /// 初期状態
    /// </summary>
    /// <returns></returns>
    public override Transition GetInitialState() => To<State.ShootReady>();

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
        /// クールダウン完了
        /// </summary>
        public readonly record struct CoolDownComplete;

        /// <summary>
        /// クールダウン開始
        /// </summary>
        public readonly record struct StartCoolDonw;
    }

    /// <summary>
    /// 出力定義
    /// </summary>
    public static class Output
    {
        /// <summary>
        /// クール中
        /// </summary>
        public readonly record struct Cooling;

        /// <summary>
        /// 生成完了
        /// </summary>
        public readonly record struct Generated;
    }

    // 不必要なヒープの割り当てを減らすために、入力と出力は読み取り専用のレコード構造体（readonly record struct）にすべき

    /// <summary>
    /// 状態定義
    /// </summary>
    public abstract record State : StateLogic<State>
    {
        /// <summary>
        /// 射撃可能
        /// </summary>
        public record ShootReady : State, IGet<Input.Fire>
        {
            public ShootReady()
            {
            }

            public Transition On(in Input.Fire input) => To<Generate>();
        }

        /// <summary>
        /// 生成
        /// </summary>
        public record Generate : State, IGet<Input.StartCoolDonw>
        {
            public Generate()
            {
                this.OnEnter(() => Output(new Output.Generated()));
            }

            public Transition On(in Input.StartCoolDonw input) => To<CoolDown>();
        }

        /// <summary>
        /// クールダウン
        /// </summary>
        public record CoolDown : State, IGet<Input.CoolDownComplete>
        {
            public CoolDown()
            {
                this.OnEnter(() => Output(new Output.Cooling()));
            }

            public Transition On(in Input.CoolDownComplete input) => To<ShootReady>();
        }

    }
}
