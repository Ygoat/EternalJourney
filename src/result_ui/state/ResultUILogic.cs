namespace EternalJourney.ResultUI.State;

using System;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.App.Domain;
using EternalJourney.Game.Domain;

/// <summary>
/// リザルトUIロジックインターフェース
/// </summary>
public interface IResultUILogic : ILogicBlock;

/// <summary>
/// リザルトUIロジック
/// </summary>
[Meta]
public partial class ResultUILogic : LogicBlock, IResultUILogic
{
    public ResultUILogic()
    {
        // ステートを事前確保
        Set(new ResultUIState.Waiting());
    }


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

}

/// <summary>
/// 状態定義
/// </summary>
[StateDiagram]
public abstract partial record ResultUIState : LogicBlockState
{
    /// <summary>
    /// 待機
    /// </summary>
    public record Waiting : ResultUIState
    {
        public Waiting()
        {
            this.OnEnter(() =>
            {
                IResultUI resultUI = Get<IResultUI>();
                Get<IGameRepo>().GameOver += OnGameOver;
                resultUI.EndButton.Pressed += OnEndButtonPressed;
            });
            this.OnExit(() =>
            {
                IResultUI resultUI = Get<IResultUI>();
                Get<IGameRepo>().GameOver -= OnGameOver;
                resultUI.EndButton.Pressed -= OnEndButtonPressed;
            });
        }

        private void OnGameOver()
        {
            IGameRepo gameRepo = Get<IGameRepo>();
            Output(new ResultUILogic.Output.UpdateDisplay(gameRepo.FinalScore, gameRepo.FinalTime));
        }

        private void OnEndButtonPressed()
        {
            IAppRepo appRepo = Get<IAppRepo>();
            appRepo.RequestGoToMenu();
        }
    }
}
