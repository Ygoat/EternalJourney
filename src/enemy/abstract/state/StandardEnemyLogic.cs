namespace EternalJourney.Enemy.Abstract.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

/// <summary>
/// エネミーロジックインターフェース
/// </summary>
public interface IStandardEnemyLogic : ILogicBlock<StandardEnemyLogic.State>;

/// <summary>
/// エネミーロジック
/// </summary>
[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class StandardEnemyLogic : LogicBlock<StandardEnemyLogic.State>, IStandardEnemyLogic
{
    /// <summary>
    /// 初期状態
    /// </summary>
    /// <returns></returns>
    public override Transition GetInitialState() => To<State.SpawnWaiting>();

    /// <summary>
    /// 入力定義
    /// </summary>
    public static class Input
    {
        /// <summary>
        /// ターゲット発見
        /// </summary>
        public readonly record struct TargetDiscover;

        /// <summary>
        /// スポーン
        /// </summary>
        public readonly record struct Removed;

        /// <summary>
        /// 被ダメージ
        /// </summary>
        public readonly record struct TakeDamage;

        /// <summary>
        /// 崩壊
        /// </summary>
        public readonly record struct Collapse;

        /// <summary>
        /// 生存
        /// </summary>
        public readonly record struct Alive;

        /// <summary>
        /// エリア外
        /// </summary>
        public readonly record struct OutOfArea;
    }

    /// <summary>
    /// 出力定義
    /// </summary>
    public static class Output
    {
        /// <summary>
        /// 接近開始
        /// </summary>
        public readonly record struct StartClose;

        /// <summary>
        /// 被弾
        /// </summary>
        public readonly record struct Disappear;

        /// <summary>
        /// スポーン可能
        /// </summary>
        public readonly record struct SpawnEnable;

        /// <summary>
        /// 劣化
        /// </summary>
        public readonly record struct Decay;
    }

    /// <summary>
    /// 状態定義
    /// </summary>
    public abstract record State : StateLogic<State>
    {
        /// <summary>
        /// スポーン待機
        /// </summary>
        public record SpawnWaiting : State, IGet<Input.TargetDiscover>
        {
            public SpawnWaiting()
            {
                this.OnEnter(() => Output(new Output.SpawnEnable()));
            }

            public Transition On(in Input.TargetDiscover input) => To<Closing>();
        }

        /// <summary>
        /// 接近
        /// </summary>
        public record Closing : State, IGet<Input.TakeDamage>, IGet<Input.OutOfArea>
        {
            public Closing()
            {
                this.OnEnter(() => Output(new Output.StartClose()));
            }

            public Transition On(in Input.TakeDamage input) => To<TakingDamage>();

            public Transition On(in Input.OutOfArea input) => To<Destroy>();
        }

        /// <summary>
        /// 被弾中
        /// </summary>
        public record TakingDamage : State, IGet<Input.Alive>, IGet<Input.Collapse>
        {
            public TakingDamage()
            {
                this.OnEnter(() => Output(new Output.Decay()));
            }

            public Transition On(in Input.Alive input) => To<Closing>();
            public Transition On(in Input.Collapse input) => To<Destroy>();
        }

        /// <summary>
        /// 破壊
        /// </summary>
        public record Destroy : State, IGet<Input.Removed>
        {
            public Destroy()
            {
                this.OnEnter(() => Output(new Output.Disappear()));
            }

            public Transition On(in Input.Removed input) => To<SpawnWaiting>();
        }


    }
}
