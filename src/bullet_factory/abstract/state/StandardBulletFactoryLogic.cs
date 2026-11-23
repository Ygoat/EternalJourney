namespace EternalJourney.Bullet.Abstract.State;

using System;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

/// <summary>
/// スタンダード弾丸ファクトリロジックインターフェース
/// </summary>
public interface IStandardBulletFactoryLogic : ILogicBlock;

/// <summary>
/// スタンダード弾丸ファクトリロジック
/// Ready → CoolDown → Ready のサイクルで弾丸射出とクールダウンを管理
/// </summary>
[Meta]
public partial class StandardBulletFactoryLogic : LogicBlock, IStandardBulletFactoryLogic
{
    public StandardBulletFactoryLogic()
    {
        // ステートを事前確保
        Set(new StandardBulletFactoryState.Ready());
        Set(new StandardBulletFactoryState.CoolDown());
    }


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

}

[StateDiagram]
public abstract partial record StandardBulletFactoryState : LogicBlockState
{
    /// <summary>
    /// 発射可能状態
    /// </summary>
    public record Ready : StandardBulletFactoryState, IGet<StandardBulletFactoryLogic.Input.FireRequested>
    {
        public Type On(in StandardBulletFactoryLogic.Input.FireRequested input)
        {
            Output(new StandardBulletFactoryLogic.Output.Fire());
            return To<CoolDown>();
        }
    }

    /// <summary>
    /// クールダウン状態
    /// </summary>
    public record CoolDown : StandardBulletFactoryState, IGet<StandardBulletFactoryLogic.Input.CoolDownComplete>
    {
        public CoolDown()
        {
            this.OnEnter(() => Output(new StandardBulletFactoryLogic.Output.StartCoolDown()));
        }

        public Type On(in StandardBulletFactoryLogic.Input.CoolDownComplete input) => To<Ready>();
    }
}
