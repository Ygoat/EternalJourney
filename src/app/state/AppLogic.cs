namespace EternalJourney.App.State;

using System;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.App.Domain;

/// <summary>
/// アプリケーションロジックインターフェース
/// </summary>
public interface IAppLogic : ILogicBlock;

/// <summary>
/// アプリケーションロジック
/// </summary>
[Meta]
public partial class AppLogic : LogicBlock, IAppLogic
{
    public AppLogic()
    {
        // ステートを事前確保
        Set(new AppState.SplashScreen());
        Set(new AppState.MainMenu());
        Set(new AppState.InGame());
        Set(new AppState.LeavingGame());
    }


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

        /// <summary>
        /// メニューへ戻る
        /// </summary>
        public readonly record struct GoToMenu;
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

}

/// <summary>
/// 状態定義
/// </summary>
[StateDiagram]
public abstract partial record AppState : LogicBlockState
{
    /// <summary>
    /// SplasScreen（スプラッシュ画面）状態
    /// </summary>
    public record SplashScreen : AppState, IGet<AppLogic.Input.SplashFinished>
    {
        public SplashScreen()
        {
            // この状態がアクティブになった時の処理
            this.OnEnter(
                // スプラッシュ画面スキップイベント設定
                () => Get<IAppRepo>().SplashScreenSkipped += OnSplashScreenSkipped
            );

            // この状態に入る時の処理
            // ShowSplashScreen（スプラッシュ画面表示）を出力
            this.OnEnter(() => Output(new AppLogic.Output.ShowSplashScreen()));

            // この状態が非アクティブになった時の処理
            this.OnExit(
                // スプラッシュ画面スキップイベント設定
                () => Get<IAppRepo>().SplashScreenSkipped -= OnSplashScreenSkipped
            );
        }

        /// <summary>
        /// 状態の遷移を定義
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public Type On(in AppLogic.Input.SplashFinished input) => To<MainMenu>();

        /// <summary>
        /// スプラッシュスクリーンスキップイベントファンクション
        /// </summary>
        public void OnSplashScreenSkipped()
        {
            // HideSplashScreen（スプラッシュ画面非表示）を出力
            Output(new AppLogic.Output.HideSplashScreen());
        }
    }

    /// <summary>
    /// メインメニュー
    /// </summary>
    public record MainMenu : AppState, IGet<AppLogic.Input.StartGame>//, IGet<AppLogic.Input.LoadGame>
    {
        public MainMenu()
        {
            // この状態に入る時の処理
            this.OnEnter(
                () =>
                {

                    Output(new AppLogic.Output.SetupGameScene());

                    Get<IAppRepo>().OnMainMenuEntered();

                    Output(new AppLogic.Output.ShowMainMenu());
                }
            );
        }

        public Type On(in AppLogic.Input.StartGame input) => To<InGame>();
    }

    public partial record InGame : AppState, IGet<AppLogic.Input.EndGame>, IGet<AppLogic.Input.GoToMenu>
    {
        public InGame()
        {
            this.OnEnter(() => Get<IAppRepo>().GoToMenuRequested += OnGoToMenuRequested);
            this.OnEnter(() =>
            {
                Get<IAppRepo>().OnEnterGame();
                Output(new AppLogic.Output.ShowGame());
            });
            this.OnExit(() => Get<IAppRepo>().GoToMenuRequested -= OnGoToMenuRequested);
        }

        private void OnGoToMenuRequested() => Input(new AppLogic.Input.GoToMenu());

        public Type On(in AppLogic.Input.EndGame input)
        {
            return To<LeavingGame>();
        }

        public Type On(in AppLogic.Input.GoToMenu input)
        {
            Output(new AppLogic.Output.RemoveExistingGame());
            return To<MainMenu>();
        }
    }

    /// <summary>
    /// ダミーの実装
    /// </summary>
    public partial record LeavingGame : AppState, IGet<AppLogic.Input.FadeOutFinished>
    {

        public Type On(in AppLogic.Input.FadeOutFinished input)
        {
            // We are either supposed to restart the game or go back to the main
            // menu. More complex games might have more post-game destinations,
            // but it's pretty simple for us.
            Output(new AppLogic.Output.RemoveExistingGame());

            // Output(new AppLogic.Output.SetupGameScene());
            return To<InGame>();
        }
    }
}
