namespace EternalJourney.App;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.App.Domain;
using EternalJourney.App.State;
using EternalJourney.Cores.Consts;
using EternalJourney.Cores.Repositories;
using EternalJourney.Cores.Utils;
using EternalJourney.Game;
using Godot;

public interface IApp : ICanvasLayer, IProvide<IAppRepo>, IProvide<ICrewCsvReader>, IProvide<IInstantiator>;

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

    public ICrewCsvReader CrewCsvReader { get; set; } = default!;

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
    IAppRepo IProvide<IAppRepo>.Value() => AppRepo;

    /// <summary>
    /// クルーCSVリーダ
    /// </summary>
    ICrewCsvReader IProvide<ICrewCsvReader>.Value() => CrewCsvReader;

    /// <summary>
    /// シーンインスタンス化部品
    /// </summary>
    IInstantiator IProvide<IInstantiator>.Value() => Instantiator;

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

        // クルーCSVリーダインスタンス化
        CrewCsvReader = new CrewCsvReader();

        // メニュー：スタートゲームシグナル受信時のイベント設定
        Menu.StartGame += OnStartGame;

        // メニュー：デバッグモードゲームスタートシグナル受信時のイベント設定
        Menu.StartDebugGame += OnStartDebugGame;

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
    /// <inheritdoc/>
    /// </summary>
    public void OnReady()
    {
        AppBinding = AppLogic.Bind();

        AppBinding
            // ShowSplashScreen（スプラッシュ表示）が出力された時の処理
            .Handle((in AppLogic.Output.ShowSplashScreen _) =>
            {
                HideMenus();
                BlankScreen.Hide();
                Splash.Show();
            })
            // HideSplashScreen（スプラッシュ非表示）が出力された時の処理
            .Handle((in AppLogic.Output.HideSplashScreen _) =>
            {
                AppLogic.Input(new AppLogic.Input.SplashFinished());
            })
            // ShowMainMenu（メインメニュー表示）が出力された時の処理
            .Handle((in AppLogic.Output.ShowMainMenu _) =>
            {
                HideMenus();
                Menu.Show();
            })
            // SetupGameScene（ゲームシーンセットアップ）が出力された時の処理
            .Handle((in AppLogic.Output.SetupGameScene _) =>
            {
                Game = Instantiator.LoadAndInstantiate<Game>(Const.GameNodePath);
            })
            // ShowGame（ゲームシーン表示）が出力された時の処理
            .Handle((in AppLogic.Output.ShowGame _) =>
            {
                GameView.AddChildEx(Game);
                HideMenus();
            })
            // RemoveExistingGame（ゲーム終了）が出力された時の処理
            .Handle((in AppLogic.Output.RemoveExistingGame _) =>
            {
                Game.QueueFree();
                Game = default!;
            });

        // 初期状態（SplashScreen）開始
        AppLogic.Start();
    }

    /// <summary>
    /// ゲーム開始イベント
    /// </summary>
    public void OnStartGame()
    {
        AppLogic.Input(new AppLogic.Input.StartGame());
    }

    /// <summary>
    /// デバッグモードゲーム開始イベント
    /// </summary>
    public void OnStartDebugGame()
    {
        AppRepo.StartDebugMode();
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

    /// <summary>
    /// ノードがツリーから離脱する時の後始末
    /// </summary>
    public void OnTreeExiting()
    {
        Menu.StartGame -= OnStartGame;
        Menu.StartDebugGame -= OnStartDebugGame;
        AppBinding.Dispose();
        ((System.IDisposable)AppLogic).Dispose();
        AppRepo.Dispose();
    }
}
