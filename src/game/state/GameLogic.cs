namespace EternalJourney.Game.State;

using System;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Game.Domain;

/// <summary>
/// ゲームロジックインターフェース
/// </summary>
public interface IGameLogic : ILogicBlock;

/// <summary>
/// ゲームロジック
/// </summary>
[Meta]
public partial class GameLogic : LogicBlock, IGameLogic
{
    public GameLogic()
    {
        // ステートを事前確保
        Set(new GameState.SelectSkillPhase());
        Set(new GameState.SelectSpSkillPhase());
        Set(new GameState.BattleInitPhase());
        Set(new GameState.BattleRunningPhase());
        Set(new GameState.BattleEndingPhase());
        Set(new GameState.ResultPhase());
    }


    public static class Input
    {
        /// <summary>
        /// スキル選択完了
        /// </summary>
        public readonly record struct SkillSelected;

        /// <summary>
        /// SPスキル選択完了
        /// </summary>
        public readonly record struct SpSkillSelected;

        /// <summary>
        /// バトル初期化完了
        /// </summary>
        public readonly record struct BattleInitialized;

        /// <summary>
        /// ゲームオーバー
        /// </summary>
        public readonly record struct GameOver;

        /// <summary>
        /// バトル終了完了
        /// </summary>
        public readonly record struct BattleEnded;
    }

    public static class Output
    {
        /// <summary>
        /// スキル選択画面表示
        /// </summary>
        public readonly record struct ShowSelectSkill;

        /// <summary>
        /// SPスキル選択画面表示
        /// </summary>
        public readonly record struct ShowSelectSpSkill;

        /// <summary>
        /// バトル初期化
        /// </summary>
        public readonly record struct InitializeBattle;

        /// <summary>
        /// バトル開始
        /// </summary>
        public readonly record struct StartBattle;

        /// <summary>
        /// バトル終了
        /// </summary>
        public readonly record struct EndBattle;

        /// <summary>
        /// リザルト表示
        /// </summary>
        public readonly record struct ShowResult;
    }

}

[StateDiagram]
public abstract partial record GameState : LogicBlockState
{
    /// <summary>
    /// スキル選択フェーズ
    /// </summary>
    public record SelectSkillPhase : GameState, IGet<GameLogic.Input.SkillSelected>
    {
        public SelectSkillPhase()
        {
            this.OnEnter(() => Get<IGameRepo>().SkillSelected += OnSkillSelected);
            this.OnEnter(() => Output(new GameLogic.Output.ShowSelectSkill()));
            this.OnExit(() => Get<IGameRepo>().SkillSelected -= OnSkillSelected);
        }

        private void OnSkillSelected() => Input(new GameLogic.Input.SkillSelected());

        public Type On(in GameLogic.Input.SkillSelected input) => To<SelectSpSkillPhase>();
    }

    /// <summary>
    /// SPスキル選択フェーズ
    /// </summary>
    public record SelectSpSkillPhase : GameState, IGet<GameLogic.Input.SpSkillSelected>
    {
        public SelectSpSkillPhase()
        {
            this.OnEnter(() => Get<IGameRepo>().SpSkillSelected += OnSpSkillSelected);
            this.OnEnter(() => Output(new GameLogic.Output.ShowSelectSpSkill()));
            this.OnExit(() => Get<IGameRepo>().SpSkillSelected -= OnSpSkillSelected);
        }

        private void OnSpSkillSelected() => Input(new GameLogic.Input.SpSkillSelected());

        public Type On(in GameLogic.Input.SpSkillSelected input) => To<BattleInitPhase>();
    }

    /// <summary>
    /// バトル初期化フェーズ
    /// </summary>
    public record BattleInitPhase : GameState, IGet<GameLogic.Input.BattleInitialized>
    {
        public BattleInitPhase()
        {
            this.OnEnter(() => Get<IGameRepo>().BattleInitialized += OnBattleInitialized);
            this.OnEnter(() => Output(new GameLogic.Output.InitializeBattle()));
            this.OnExit(() => Get<IGameRepo>().BattleInitialized -= OnBattleInitialized);
        }

        private void OnBattleInitialized() => Input(new GameLogic.Input.BattleInitialized());

        public Type On(in GameLogic.Input.BattleInitialized input) => To<BattleRunningPhase>();
    }

    /// <summary>
    /// バトル実行フェーズ
    /// </summary>
    public record BattleRunningPhase : GameState, IGet<GameLogic.Input.GameOver>
    {
        public BattleRunningPhase()
        {
            this.OnEnter(() => Get<IGameRepo>().GameOver += OnGameOver);
            this.OnEnter(() => Output(new GameLogic.Output.StartBattle()));
            this.OnExit(() => Get<IGameRepo>().GameOver -= OnGameOver);
        }

        private void OnGameOver() => Input(new GameLogic.Input.GameOver());

        public Type On(in GameLogic.Input.GameOver input) => To<BattleEndingPhase>();
    }

    /// <summary>
    /// バトル終了フェーズ
    /// </summary>
    public record BattleEndingPhase : GameState, IGet<GameLogic.Input.BattleEnded>
    {
        public BattleEndingPhase()
        {
            this.OnEnter(() => Get<IGameRepo>().BattleEnded += OnBattleEnded);
            this.OnEnter(() => Output(new GameLogic.Output.EndBattle()));
            this.OnExit(() => Get<IGameRepo>().BattleEnded -= OnBattleEnded);
        }

        private void OnBattleEnded() => Input(new GameLogic.Input.BattleEnded());

        public Type On(in GameLogic.Input.BattleEnded input) => To<ResultPhase>();
    }

    /// <summary>
    /// リザルトフェーズ
    /// </summary>
    public record ResultPhase : GameState
    {
        public ResultPhase()
        {
            this.OnEnter(() => Output(new GameLogic.Output.ShowResult()));
        }
    }
}
