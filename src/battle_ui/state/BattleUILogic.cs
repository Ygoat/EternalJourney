namespace EternalJourney.BattleUI.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Battle.Domain;
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
        /// ゲーム終了（ダミー）
        /// </summary>
        public readonly record struct ScoreChanged(int CurrentScore);
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
                    }
                );

                // この状態が非アクティブになった時の処理
                OnDetach(() =>
                    {
                        // スプラッシュ画面スキップイベント設定
                        IBattleRepo battleRepo = Get<IBattleRepo>();
                        battleRepo.Score.Sync -= OnScoreCountUp;
                    }
                );
            }

            /// <summary>
            /// スコアカウントアップイベントファンクション
            /// </summary>
            /// <param name="currentScore"></param>
            public void OnScoreCountUp(int currentScore)
            {
                Output(new Output.ScoreChanged(currentScore));
            }
        }
    }
}
