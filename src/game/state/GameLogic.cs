namespace EternalJourney.Game.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Game.Domain;

/// <summary>
/// ゲームロジックインターフェース
/// </summary>
public interface IGameLogic : ILogicBlock<GameLogic.State>;

/// <summary>
/// ゲームロジック
/// </summary>
[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class GameLogic : LogicBlock<GameLogic.State>, IGameLogic
{
    public override Transition GetInitialState() => To<State.SelectSkillPhase>();

    public static class Input
    {
        /// <summary>
        /// スキル選択完了
        /// </summary>
        public readonly record struct SkillSelected;

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

    public abstract record State : StateLogic<State>
    {
        /// <summary>
        /// スキル選択フェーズ
        /// </summary>
        public record SelectSkillPhase : State, IGet<Input.SkillSelected>
        {
            public SelectSkillPhase()
            {
                this.OnEnter(() => Output(new Output.ShowSelectSkill()));
                OnAttach(() => Get<IGameRepo>().SkillSelected += OnSkillSelected);
                OnDetach(() => Get<IGameRepo>().SkillSelected -= OnSkillSelected);
            }

            private void OnSkillSelected() => Input(new Input.SkillSelected());

            public Transition On(in Input.SkillSelected input) => To<BattleInitPhase>();
        }

        /// <summary>
        /// バトル初期化フェーズ
        /// </summary>
        public record BattleInitPhase : State, IGet<Input.BattleInitialized>
        {
            public BattleInitPhase()
            {
                this.OnEnter(() => Output(new Output.InitializeBattle()));
                OnAttach(() => Get<IGameRepo>().BattleInitialized += OnBattleInitialized);
                OnDetach(() => Get<IGameRepo>().BattleInitialized -= OnBattleInitialized);
            }

            private void OnBattleInitialized() => Input(new Input.BattleInitialized());

            public Transition On(in Input.BattleInitialized input) => To<BattleRunningPhase>();
        }

        /// <summary>
        /// バトル実行フェーズ
        /// </summary>
        public record BattleRunningPhase : State, IGet<Input.GameOver>
        {
            public BattleRunningPhase()
            {
                this.OnEnter(() => Output(new Output.StartBattle()));
                OnAttach(() => Get<IGameRepo>().GameOver += OnGameOver);
                OnDetach(() => Get<IGameRepo>().GameOver -= OnGameOver);
            }

            private void OnGameOver() => Input(new Input.GameOver());

            public Transition On(in Input.GameOver input) => To<BattleEndingPhase>();
        }

        /// <summary>
        /// バトル終了フェーズ
        /// </summary>
        public record BattleEndingPhase : State, IGet<Input.BattleEnded>
        {
            public BattleEndingPhase()
            {
                this.OnEnter(() => Output(new Output.EndBattle()));
                OnAttach(() => Get<IGameRepo>().BattleEnded += OnBattleEnded);
                OnDetach(() => Get<IGameRepo>().BattleEnded -= OnBattleEnded);
            }

            private void OnBattleEnded() => Input(new Input.BattleEnded());

            public Transition On(in Input.BattleEnded input) => To<ResultPhase>();
        }

        /// <summary>
        /// リザルトフェーズ
        /// </summary>
        public record ResultPhase : State
        {
            public ResultPhase()
            {
                this.OnEnter(() => Output(new Output.ShowResult()));
            }
        }
    }
}
