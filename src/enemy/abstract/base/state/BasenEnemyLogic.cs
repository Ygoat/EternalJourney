namespace EternalJourney.Enemy.Abstract.Base.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Battle.Domain;
using EternalJourney.Common.Traits;

/// <summary>
/// エネミーロジックインターフェース
/// </summary>
public interface IBaseEnemyLogic : ILogicBlock<BaseEnemyLogic.State>;

/// <summary>
/// エネミーロジック
/// </summary>
[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class BaseEnemyLogic : LogicBlock<BaseEnemyLogic.State>, IBaseEnemyLogic
{
    /// <summary>
    /// 初期状態
    /// </summary>
    /// <returns></returns>
    public override Transition GetInitialState() => To<State.DummyState>();

    /// <summary>
    /// 入力定義
    /// </summary>
    public static class Input
    {
        /// <summary>
        /// 毒ダメージ
        /// </summary>
        public readonly record struct PoisonDamage(float Damage);
    }

    /// <summary>
    /// 出力定義
    /// </summary>
    public static class Output
    {
        /// <summary>
        /// 減少後耐久値
        /// </summary>
        public readonly record struct ReduceDurability(float ReducedDurability);
    }

    /// <summary>
    /// 状態定義
    /// </summary>
    public abstract record State : StateLogic<State>
    {
        /// <summary>
        /// スポーン待機
        /// </summary>
        public record DummyState : State, IGet<Input.PoisonDamage>
        {
            public DummyState()
            {
            }

            public Transition On(in Input.PoisonDamage input)
            {
                IBattleRepo battleRepo = Get<IBattleRepo>();
                Status status = Get<Status>();
                float reducedDurability = battleRepo.ReduceEnemyDurability(status.CurrentDur, input.Damage);
                Output(new Output.ReduceDurability(reducedDurability));
                return ToSelf();
            }
        }
    }
}
