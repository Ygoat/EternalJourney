namespace EternalJourney.App;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.App.Domain;
using EternalJourney.App.State;
using EternalJourney.Cores.Utils;
using Godot;

public interface IApp : ICanvasLayer, IProvide<IAppRepo>;

[Meta(typeof(IAutoNode))]
public partial class App : CanvasLayer, IApp
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="what"></param>
    public override void _Notification(int what) => this.Notify(what);

    /// <summary>
    /// ゲーム
    /// </summary>
    public IGame Game { get; set; } = default!;

    /// <summary>
    /// インスタンシエーター
    /// </summary>
    public IInstantiator Instantiator { get; set; } = default!;

    /// <summary>
    /// メニュー画面
    /// </summary>
    [Node]
    public IMenu Menu { get; set; } = default!;

    /// <summary>
    /// スプラッシュ画面
    /// </summary>
    [Node]
    public ISplash Splash { get; set; } = default!;

    /// <summary>
    /// ブランク画面
    /// </summary>
    [Node]
    public IColorRect BlankScreen { get; set; } = default!;

    /// <summary>
    /// ゲームビュー
    /// </summary>
    [Node]
    public ISubViewport GameView { get; set; } = default!;

    /// <summary>
    /// AppRepoプロバイダー
    /// </summary>
    /// <returns></returns>
    IAppRepo IProvide<IAppRepo>.Value() => AppRepo;

    /// <summary>
    /// アプリケーションレポジトリ
    /// </summary>
    public IAppRepo AppRepo { get; set; } = default!;

    /// <summary>
    /// アプリケーションロジック
    /// </summary>
    public IAppLogic AppLogic { get; set; } = default!;

    /// <summary>
    /// アプリケーションロジックステートバインド
    /// </summary>
    public AppLogic.IBinding AppBinding { get; set; } = default!;

    /// <summary>
    /// NodeがReady状態になる前の初期処理
    /// <inheritdoc/>
    /// </summary>
    public void Initialize()
    {
        // 共通部品インスタンス化
        Instantiator = new Instantiator(GetTree());

        // メニュー：スタートゲームシグナル受信時のイベント設定
        Menu.StartGame += OnStartGame;

        // アプリケーションレポジトリインスタンス化
        AppRepo = new AppRepo();

        // アプリケーションロジックインスタンス化
        AppLogic = new AppLogic();

        // アプリケーションレポジトリをアプリケーションロジックに注入
        AppLogic.Set(AppRepo);

        // 依存性解決
        this.Provide();
    }

    /// <summary>
    /// NodeがReady状態になった時の処理
    /// </summary>
    public void OnReady()
    {
        AppBinding = AppLogic.Bind();

        AppBinding
            // ShowSplashScreen（スプラッシュ表示）が出力された時の処理
            .Handle((in AppLogic.Output.ShowSplashScreen _) =>
            {
                // 表示されているメニューを閉じる
                HideMenus();
                // ブランク画面を閉じる（フェードイン用の画面のためいらない）
                BlankScreen.Hide();
                // スプラッシュ表示
                Splash.Show();
            })
            // HideSplashScreen（スプラッシュ非表示）が出力された時の処理
            .Handle((in AppLogic.Output.HideSplashScreen _) =>
            {
                // SplashFinished（スプラッシュ完了）を入力
                AppLogic.Input(new AppLogic.Input.SplashFinished());
            })
            // ShowMainMenu（メインメニュー表示）が出力された時の処理
            .Handle((in AppLogic.Output.ShowMainMenu _) =>
            {
                // 表示されているメニューを閉じる
                HideMenus();
                // メニューを表示する
                Menu.Show();
            })
            // SetupGameScene（ゲームシーンセットアップ）が出力された時の処理
            .Handle((in AppLogic.Output.SetupGameScene _) =>
            {
                // ゲームシーンをインスタンス化
                Game = Instantiator.LoadAndInstantiate<Game>("res://src/Game.tscn");
            })
            // ShowGame（ゲームシーン表示）が出力された時の処理
            .Handle((in AppLogic.Output.ShowGame _) =>
            {
                // ビューポートにシーンを表示
                GameView.AddChildEx(Game);
                // メニュー非表示
                HideMenus();
            })
            // RemoveExistingGame（ゲーム終了）が出力された時の処理
            .Handle((in AppLogic.Output.RemoveExistingGame _) =>
            {
                // 未実装
                //   Game.QueueFree();
                //   Game = default!;
            });

        // 初期状態（SplashScreen）開始
        AppLogic.Start();
    }

    /// <summary>
    /// ゲーム開始イベント
    /// </summary>
    public void OnStartGame()
    {
        GD.Print("game start");
        AppLogic.Input(new AppLogic.Input.StartGame());
    }

    /// <summary>
    /// メニュー非表示
    /// </summary>
    public void HideMenus()
    {
        Splash.Hide();
        Menu.Hide();
    }
}
