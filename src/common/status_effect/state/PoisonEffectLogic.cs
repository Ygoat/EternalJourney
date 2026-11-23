namespace EternalJourney.Common.StatusEffect.State;

using System;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Battle.Domain;


/// <summary>
/// 毒効果ロジックインターフェース
/// </summary>
public interface IPoisonEffectLogic : ILogicBlock;

/// <summary>
/// 毒効果ロジッククラス
/// </summary>
[Meta]
public partial class PoisonEffectLogic : LogicBlock, IPoisonEffectLogic
{
    public PoisonEffectLogic()
    {
        // ステートを事前確保
        Set(new PoisonEffectState.InActive());
        Set(new PoisonEffectState.Active());
    }


    /// <summary>
    /// 入力定義
    /// </summary>
    public static class Input
    {
        /// <summary>
        /// 効果適用
        /// </summary>
        public readonly record struct Apply;

        /// <summary>
        /// 効果除去
        /// </summary>
        public readonly record struct Remove;
    }

    /// <summary>
    /// 出力定義
    /// </summary>
    public static class Output
    {
        /// <summary>
        /// 回復
        /// </summary>
        public readonly record struct Recovery;
    }

}

/// <summary>
/// 状態定義
/// </summary>
[StateDiagram]
public abstract partial record PoisonEffectState : LogicBlockState
{
    /// <summary>
    /// 未適用
    /// </summary>
    public record InActive : PoisonEffectState, IGet<PoisonEffectLogic.Input.Apply>
    {
        public InActive()
        {
        }

        public Type On(in PoisonEffectLogic.Input.Apply input) => To<Active>();
    }

    /// <summary>
    /// 適用
    /// </summary>
    public record Active : PoisonEffectState, IGet<PoisonEffectLogic.Input.Remove>, IGet<PoisonEffectLogic.Input.Apply>
    {
        public float PoisonDamage { get; set; } = default!;

        public Active()
        {
            this.OnEnter(() =>
            {
                IBattleRepo battleRepo = Get<IBattleRepo>();
                PoisonDamage = battleRepo.PoisonDamage;
            });
        }
        public Type On(in PoisonEffectLogic.Input.Apply input) => ToSelf();
        public Type On(in PoisonEffectLogic.Input.Remove input) => To<InActive>();
    }
}
