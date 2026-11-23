namespace EternalJourney.Common.StatusEffect.State;

using System;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

/// <summary>
/// スタン効果ロジックインターフェース
/// </summary>
public interface IStunEffectLogic : ILogicBlock;

/// <summary>
/// スタン効果ロジッククラス
/// </summary>
[Meta]
public partial class StunEffectLogic : LogicBlock, IStunEffectLogic
{
    public StunEffectLogic()
    {
        // ステートを事前確保
        Set(new StunEffectState.InActive());
        Set(new StunEffectState.Active());
    }

    public static class Input
    {
        /// <summary>
        /// 効果適用（重ねがけも含む）
        /// </summary>
        public readonly record struct Apply;

        /// <summary>
        /// 効果除去
        /// </summary>
        public readonly record struct Remove;
    }

    public static class Output
    {
        /// <summary>
        /// スタン開始
        /// </summary>
        public readonly record struct Activated;

        /// <summary>
        /// スタン終了
        /// </summary>
        public readonly record struct Deactivated;
    }
}

/// <summary>
/// スタン効果ステート
/// </summary>
[StateDiagram]
public abstract partial record StunEffectState : LogicBlockState
{
    /// <summary>
    /// 未適用
    /// </summary>
    public record InActive : StunEffectState, IGet<StunEffectLogic.Input.Apply>
    {
        public Type On(in StunEffectLogic.Input.Apply input) => To<Active>();
    }

    /// <summary>
    /// スタン中
    /// </summary>
    public record Active : StunEffectState, IGet<StunEffectLogic.Input.Apply>, IGet<StunEffectLogic.Input.Remove>
    {
        public Active()
        {
            this.OnEnter(() => Output(new StunEffectLogic.Output.Activated()));
            this.OnExit(() => Output(new StunEffectLogic.Output.Deactivated()));
        }

        // 重ねがけ：タイマーリセットはStunEffect側のWatchで処理
        public Type On(in StunEffectLogic.Input.Apply input) => ToSelf();

        public Type On(in StunEffectLogic.Input.Remove input) => To<InActive>();
    }
}
