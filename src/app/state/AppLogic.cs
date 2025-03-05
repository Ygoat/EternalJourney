namespace EternalJourney.App.State;

using System;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.App.Domain;

/// <summary>
/// アプリケーションロジックインターフェース
/// </summary>
public interface IAppLogic : ILogicBlock<AppLogic.State>;

/// <summary>
/// アプリケーションロジック
/// </summary>
[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class AppLogic : LogicBlock<AppLogic.State>, IAppLogic
{
    /// <summary>
    /// 初期状態
    /// </summary>
    /// <returns></returns>
    public override Transition GetInitialState() => To<State.SplashScreen>();

    /// <summary>
    /// 入力定義
    /// </summary>
    public static class Input
    {
        /// <summary>
        /// スプラッシュ終了
        /// </summary>
        public readonly record struct SplashFinished;

        /// <summary>
        /// ゲームスタート
        /// </summary>
        public readonly record struct StartGame;

        /// <summary>
        /// ゲーム終了
        /// </summary>
        public readonly record struct EndGame;

        /// <summary>
        /// フェードアウト完了
        /// </summary>
        public readonly record struct FadeOutFinished;
    }

    /// <summary>
    /// 出力定義
    /// </summary>
    public static class Output
    {
        /// <summary>
        /// スプラッシュ画面表示
        /// </summary>
        public readonly record struct ShowSplashScreen;

        /// <summary>
        /// スプラッシュ画面非表示
        /// </summary>
        public readonly record struct HideSplashScreen;

        /// <summary>
        /// ゲームシーンセットアップ
        /// </summary>
        public readonly record struct SetupGameScene;

        /// <summary>
        /// メインメニュー表示
        /// </summary>
        public readonly record struct ShowMainMenu;

        /// <summary>
        /// ゲーム表示
        /// </summary>
        public readonly record struct ShowGame;

        /// <summary>
        /// ゲーム終了（ダミー）
        /// </summary>
        public readonly record struct RemoveExistingGame;
    }

    // 不必要なヒープの割り当てを減らすために、入力と出力は読み取り専用のレコード構造体（readonly record struct）にすべき

    /// <summary>
    /// 状態定義
    /// </summary>
    public abstract record State : StateLogic<State>
    {
        /// <summary>
        /// SplasScreen（スプラッシュ画面）状態
        /// </summary>
        public record SplashScreen : State, IGet<Input.SplashFinished>
        {
            public SplashScreen()
            {
                // この状態に入る時の処理
                // ShowSplashScreen（スプラッシュ画面表示）を出力
                this.OnEnter(() => Output(new Output.ShowSplashScreen()));

                // この状態がアクティブになった時の処理
                OnAttach(
                    // スプラッシュ画面スキップイベント設定
                    () => Get<IAppRepo>().SplashScreenSkipped += OnSplashScreenSkipped
                );

                // この状態が非アクティブになった時の処理
                OnDetach(
                    // スプラッシュ画面スキップイベント設定
                    () => Get<IAppRepo>().SplashScreenSkipped -= OnSplashScreenSkipped
                );
            }

            /// <summary>
            /// 状態の遷移を定義
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            public Transition On(in Input.SplashFinished input) => To<MainMenu>();

            /// <summary>
            /// スプラッシュスクリーンスキップイベントメソッド
            /// </summary>
            public void OnSplashScreenSkipped()
            {
                // HideSplashScreen（スプラッシュ画面非表示）を出力
                Output(new Output.HideSplashScreen());
            }
        }

        /// <summary>
        /// メインメニュー
        /// </summary>
        public record MainMenu : State, IGet<Input.StartGame>//, IGet<Input.LoadGame>
        {
            public MainMenu()
            {
                // この状態に入る時の処理
                this.OnEnter(
                    () =>
                    {

                        Output(new Output.SetupGameScene());

                        Get<IAppRepo>().OnMainMenuEntered();

                        Output(new Output.ShowMainMenu());
                    }
                );
            }

            public Transition On(in Input.StartGame input) => To<InGame>();
        }

        public partial record InGame : State, IGet<Input.EndGame>
        {
            public InGame()
            {
                this.OnEnter(() =>
                {
                    Get<IAppRepo>().OnEnterGame();
                    Output(new Output.ShowGame());
                });
                // this.OnExit(() => Output(new Output.HideGame()));

                // OnAttach(() => Get<IAppRepo>().GameExited += OnGameExited);
                // OnDetach(() => Get<IAppRepo>().GameExited -= OnGameExited);
            }

            // public void OnGameExited(PostGameAction reason) =>
            //   Input(new Input.EndGame(reason));

            public Transition On(in Input.EndGame input)
            {
                return To<LeavingGame>();
            }
        }

        /// <summary>
        /// ダミーの実装
        /// </summary>
        public partial record LeavingGame : State, IGet<Input.FadeOutFinished>
        {

            public Transition On(in Input.FadeOutFinished input)
            {
                // We are either supposed to restart the game or go back to the main
                // menu. More complex games might have more post-game destinations,
                // but it's pretty simple for us.
                Output(new Output.RemoveExistingGame());

                // Output(new Output.SetupGameScene());
                return To<InGame>();
            }
        }
    }
}
