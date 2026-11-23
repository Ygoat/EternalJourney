namespace EternalJourney.Enemy.Base.State;

using System;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Battle.Domain;
using EternalJourney.Common.Traits;

/// <summary>
/// エネミーロジックインターフェース
/// </summary>
public interface IBaseEnemyLogic : ILogicBlock;

/// <summary>
/// エネミーロジック
/// </summary>
[Meta]
public partial class BaseEnemyLogic : LogicBlock, IBaseEnemyLogic
{
    public BaseEnemyLogic()
    {
        // ステートを事前確保
        Set(new BaseEnemyState.DummyState());
    }


    /// <summary>
    /// 入力定義
    /// </summary>
    public static class Input
    {
        /// <summary>
        /// 毒ダメージ
        /// </summary>
        public readonly record struct PoisonDamage(float Damage);

        /// <summary>
        /// スタン開始
        /// </summary>
        public readonly record struct StunStart;

        /// <summary>
        /// スタン終了
        /// </summary>
        public readonly record struct StunEnd;

        /// <summary>
        /// SPダメージ
        /// </summary>
        public readonly record struct SPDamage(float Damage);
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

        /// <summary>
        /// スタン開始
        /// </summary>
        public readonly record struct StunStart;

        /// <summary>
        /// スタン終了
        /// </summary>
        public readonly record struct StunEnd;
    }

}

/// <summary>
/// 状態定義
/// </summary>
[StateDiagram]
public abstract partial record BaseEnemyState : LogicBlockState
{
    /// <summary>
    /// スポーン待機
    /// </summary>
    public record DummyState : BaseEnemyState, IGet<BaseEnemyLogic.Input.PoisonDamage>, IGet<BaseEnemyLogic.Input.StunStart>, IGet<BaseEnemyLogic.Input.StunEnd>, IGet<BaseEnemyLogic.Input.SPDamage>
    {
        public DummyState()
        {
        }

        public Type On(in BaseEnemyLogic.Input.PoisonDamage input)
        {
            IBattleRepo battleRepo = Get<IBattleRepo>();
            IBaseEnemy baseEnemy = Get<IBaseEnemy>();
            float reducedDurability = battleRepo.ReduceEnemyDurability(baseEnemy.Status.CurrentDur, input.Damage, 0f, 0f);
            Output(new BaseEnemyLogic.Output.ReduceDurability(reducedDurability));
            return ToSelf();
        }

        public Type On(in BaseEnemyLogic.Input.StunStart input)
        {
            Output(new BaseEnemyLogic.Output.StunStart());
            return ToSelf();
        }

        public Type On(in BaseEnemyLogic.Input.StunEnd input)
        {
            Output(new BaseEnemyLogic.Output.StunEnd());
            return ToSelf();
        }

        public Type On(in BaseEnemyLogic.Input.SPDamage input)
        {
            IBattleRepo battleRepo = Get<IBattleRepo>();
            IBaseEnemy baseEnemy = Get<IBaseEnemy>();
            float reduced = battleRepo.ReduceEnemyDurability(baseEnemy.Status.CurrentDur, input.Damage, 0f, 0f);
            Output(new BaseEnemyLogic.Output.ReduceDurability(reduced));
            return ToSelf();
        }
    }
}
