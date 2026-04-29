namespace EternalJourney.ResultUI.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.App.Domain;
using EternalJourney.Battle.Domain;
using EternalJourney.Game.Domain;

/// <summary>
/// リザルトUIロジックインターフェース
/// </summary>
public interface IResultUILogic : ILogicBlock<ResultUILogic.State>;

/// <summary>
/// リザルトUIロジック
/// </summary>
[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class ResultUILogic : LogicBlock<ResultUILogic.State>, IResultUILogic
{
    /// <summary>
    /// 初期状態
    /// </summary>
    public override Transition GetInitialState() => To<State.Waiting>();

    /// <summary>
    /// 出力定義
    /// </summary>
    public static class Output
    {
        /// <summary>
        /// 表示内容更新
        /// </summary>
        public readonly record struct UpdateDisplay(int Score, float Time);
    }

    /// <summary>
    /// 状態定義
    /// </summary>
    public abstract record State : StateLogic<State>
    {
        /// <summary>
        /// 待機
        /// </summary>
        public record Waiting : State
        {
            public Waiting()
            {
                OnAttach(() =>
                {
                    IBattleRepo battleRepo = Get<IBattleRepo>();
                    IResultUI resultUI = Get<IResultUI>();
                    battleRepo.GameOverOccurred += OnGameOver;
                    resultUI.EndButton.Pressed += OnEndButtonPressed;
                });

                OnDetach(() =>
                {
                    IBattleRepo battleRepo = Get<IBattleRepo>();
                    IResultUI resultUI = Get<IResultUI>();
                    battleRepo.GameOverOccurred -= OnGameOver;
                    resultUI.EndButton.Pressed -= OnEndButtonPressed;
                });
            }

            private void OnGameOver()
            {
                IGameRepo gameRepo = Get<IGameRepo>();
                Output(new Output.UpdateDisplay(gameRepo.FinalScore, gameRepo.FinalTime));
            }

            private void OnEndButtonPressed()
            {
                IAppRepo appRepo = Get<IAppRepo>();
                appRepo.RequestGoToMenu();
            }
        }
    }
}
