namespace EternalJourney.Enemy;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

/// <summary>
/// 弾丸ロジックインターフェース
/// </summary>
public interface IEnemyLogic : ILogicBlock<EnemyLogic.State>;

/// <summary>
/// 弾丸ロジック
/// </summary>
[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class EnemyLogic : LogicBlock<EnemyLogic.State>, IEnemyLogic
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
        public readonly record struct Spawn;

        /// <summary>
        /// 被ダメージ
        /// </summary>
        public readonly record struct TakeDamage;
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
        public readonly record struct Damaged;

        /// <summary>
        /// スポーン可能
        /// </summary>
        public readonly record struct SpawnEnable;
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
        public record Closing : State, IGet<Input.TakeDamage>
        {
            public Closing()
            {
                this.OnEnter(() => Output(new Output.StartClose()));
            }

            public Transition On(in Input.TakeDamage input) => To<Destroy>();
        }

        /// <summary>
        /// ロード
        /// </summary>
        public record Destroy : State, IGet<Input.TakeDamage>
        {
            public Destroy()
            {
                this.OnEnter(() => Output(new Output.Damaged()));
            }

            public Transition On(in Input.TakeDamage input) => To<SpawnWaiting>();
        }


    }
}
