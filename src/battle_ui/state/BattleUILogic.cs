namespace EternalJourney.BattleUI.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Battle.Domain;
using EternalJourney.Game.Domain;
using Godot;

/// <summary>
/// バトルUIロジック
/// </summary>
public interface IBattleUILogic : ILogicBlock<BattleUILogic.State>;

/// <summary>
/// アプリケーションロジック
/// </summary>
[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class BattleUILogic : LogicBlock<BattleUILogic.State>, IBattleUILogic
{
    /// <summary>
    /// 初期状態
    /// </summary>
    /// <returns></returns>
    public override Transition GetInitialState() => To<State.BattleUILogic>();

    /// <summary>
    /// 入力定義
    /// </summary>
    public static class Input
    {
    }

    /// <summary>
    /// 出力定義
    /// </summary>
    public static class Output
    {
        /// <summary>
        /// スコア変化
        /// </summary>
        public readonly record struct ScoreChanged(int CurrentScore);

        /// <summary>
        /// ゲーム終了
        /// </summary>
        public readonly record struct GameOver;

        /// <summary>
        /// 船HP変化
        /// </summary>
        public readonly record struct ShipHpChanged(float CurrentHp, float MaxHp);

        /// <summary>
        /// バトルUI起動
        /// </summary>
        public readonly record struct ActivateBattleUI;
    }

    // 不必要なヒープの割り当てを減らすために、入力と出力は読み取り専用のレコード構造体（readonly record struct）にすべき

    /// <summary>
    /// 状態定義
    /// </summary>
    public abstract record State : StateLogic<State>
    {
        /// <summary>
        /// バトルUIロジック
        /// </summary>
        public record BattleUILogic : State
        {
            public BattleUILogic()
            {
                // この状態がアクティブになった時の処理
                OnAttach(() =>
                    {
                        IBattleRepo battleRepo = Get<IBattleRepo>();
                        battleRepo.Score.Sync += OnScoreCountUp;
                        battleRepo.ShipHpChanged += OnShipHpChanged;
                        battleRepo.GameOverOccurred += OnGameOver;
                        battleRepo.ActivateBattleUI += OnActivateBattleUI;
                    }
                );

                // この状態が非アクティブになった時の処理
                OnDetach(() =>
                    {
                        IBattleRepo battleRepo = Get<IBattleRepo>();
                        battleRepo.Score.Sync -= OnScoreCountUp;
                        battleRepo.ShipHpChanged -= OnShipHpChanged;
                        battleRepo.GameOverOccurred -= OnGameOver;
                        battleRepo.ActivateBattleUI -= OnActivateBattleUI;
                    }
                );
            }

            /// <summary>
            /// スコアカウントアップイベントファンクション
            /// </summary>
            public void OnScoreCountUp(int currentScore)
            {
                Output(new Output.ScoreChanged(currentScore));
            }

            /// <summary>
            /// 船HP変化イベントファンクション
            /// </summary>
            public void OnShipHpChanged(float currentHp, float maxHp)
            {
                Output(new Output.ShipHpChanged(currentHp, maxHp));
            }

            /// <summary>
            /// ゲーム終了イベントファンクション
            /// </summary>
            public void OnGameOver()
            {
                IBattleRepo battleRepo = Get<IBattleRepo>();
                Get<IGameRepo>().SaveResult(battleRepo.Score.Value, Get<IBattleUI>().Count);
                Output(new Output.GameOver());
            }

            /// <summary>
            /// バトルUI起動イベントファンクション
            /// </summary>
            public void OnActivateBattleUI()
            {
                Output(new Output.ActivateBattleUI());
            }
        }
    }
}
