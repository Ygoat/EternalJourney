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
    public override void _Notification(int what) => this.Notify(what);

    public IGame Game { get; set; } = default!;

    public IInstantiator Instantiator { get; set; } = default!;

    [Node]
    public IMenu Menu { get; set; } = default!;

    [Node]
    public ISplash Splash { get; set; } = default!;

    [Node]
    public IColorRect BlankScreen { get; set; } = default!;

    [Node]
    public ISubViewport GameView { get; set; } = default!;


    IAppRepo IProvide<IAppRepo>.Value() => AppRepo;

    public IAppRepo AppRepo { get; set; } = default!;

    public IAppLogic AppLogic { get; set; } = default!;
    public AppLogic.IBinding AppBinding { get; set; } = default!;

    public void Initialize()
    {
        Instantiator = new Instantiator(GetTree());
        AppRepo = new AppRepo();
        Menu.StartGame += OnStartGame;
        AppLogic = new AppLogic();
        AppLogic.Set(AppRepo);
        Menu.StartGame += OnStartGame;
        this.Provide();
    }

    public void OnReady()
    {
        AppBinding = AppLogic.Bind();

        AppBinding
          .Handle((in AppLogic.Output.ShowSplashScreen _) =>
          {
              HideMenus();
              BlankScreen.Hide();
              Splash.Show();
          })
          .Handle((in AppLogic.Output.HideSplashScreen _) =>
          {
              //   BlankScreen.Hide();
              //   Splash.Hide();
              //   FadeToBlack();
              AppLogic.Input(new AppLogic.Input.SplashFinished());
          })
          .Handle((in AppLogic.Output.RemoveExistingGame _) =>
          {
              //   Game.QueueFree();
              //   Game = default!;
          })
          .Handle((in AppLogic.Output.SetupGameScene _) =>
          {
              GD.Print("Load Game");
              Game = Instantiator.LoadAndInstantiate<Game>("res://src/Game.tscn");
          })
          .Handle((in AppLogic.Output.ShowMainMenu _) =>
          {
              // Load everything while we're showing a black screen, then fade in.
              HideMenus();
              Menu.Show();

              //   FadeInFromBlack();
          })
          .Handle((in AppLogic.Output.ShowGame _) =>
          {
              GD.Print("game start");
              GameView.AddChildEx(Game);
              HideMenus();
              //   Game.Show();
              //   FadeInFromBlack();
          });

        // Enter the first state to kick off the binding side effects.
        AppLogic.Start();
    }

    public void OnStartGame()
    {
        GD.Print("game start");
        AppLogic.Input(new AppLogic.Input.StartGame());
    }

    public void HideMenus()
    {
        Splash.Hide();
        Menu.Hide();
    }
}
