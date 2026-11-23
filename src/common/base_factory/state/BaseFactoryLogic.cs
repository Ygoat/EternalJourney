namespace EternalJourney.Common.BaseFactory.State;

using System;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

/// <summary>
/// ベースファクトリロジックインターフェース
/// </summary>
public interface IBaseFactoryLogic : ILogicBlock;

/// <summary>
/// ベースファクトリロジック
/// オブジェクトプールを使用したファクトリの共通状態遷移を管理します
/// Ready → Generate → CoolDown → Ready のサイクルで動作
/// </summary>
[Meta]
public partial class BaseFactoryLogic : LogicBlock, IBaseFactoryLogic
{
    public BaseFactoryLogic()
    {
        // ステートを事前確保
        Set(new BaseFactoryState.Ready());
        Set(new BaseFactoryState.Generating());
        Set(new BaseFactoryState.CoolDown());
    }


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

}

/// <summary>
/// 状態定義
/// </summary>
[StateDiagram]
public abstract partial record BaseFactoryState : LogicBlockState
{
    /// <summary>
    /// 生成可能状態
    /// </summary>
    public record Ready : BaseFactoryState, IGet<BaseFactoryLogic.Input.Generate>
    {
        public Ready()
        {
        }

        public Type On(in BaseFactoryLogic.Input.Generate input) => To<Generating>();
    }

    /// <summary>
    /// 生成中状態
    /// </summary>
    public record Generating : BaseFactoryState, IGet<BaseFactoryLogic.Input.StartCoolDown>
    {
        public Generating()
        {
            this.OnEnter(() => Output(new BaseFactoryLogic.Output.Generated()));
        }

        public Type On(in BaseFactoryLogic.Input.StartCoolDown input) => To<CoolDown>();
    }

    /// <summary>
    /// クールダウン状態
    /// </summary>
    public record CoolDown : BaseFactoryState, IGet<BaseFactoryLogic.Input.CoolDownComplete>
    {
        public CoolDown()
        {
            this.OnEnter(() => Output(new BaseFactoryLogic.Output.Cooling()));
        }

        public Type On(in BaseFactoryLogic.Input.CoolDownComplete input) => To<Ready>();
    }
}
