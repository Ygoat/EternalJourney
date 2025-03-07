namespace EternalJourney;

using Godot;

#if DEBUG
using System.Reflection;
using Chickensoft.GoDotTest;
using Godot.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using EternalJourney.Cores.Repositories;
#endif

// This entry-point file is responsible for determining if we should run tests.
//
// If you want to edit your game's main entry-point, please see Game.tscn and
// Game.cs instead.



public partial class Main : Node2D, IServicesConfigurator
{
#if DEBUG
    public TestEnvironment Environment = default!;
#endif

    public override void _Ready()
    {
#if DEBUG
        // If this is a debug build, use GoDotTest to examine the
        // command line arguments and determine if we should run tests.
        Environment = TestEnvironment.From(OS.GetCmdlineArgs());
        if (Environment.ShouldRunTests)
        {
            CallDeferred("RunTests");
            return;
        }
#endif

        // If we don't need to run tests, we can just switch to the game scene.
        CallDeferred("RunScene");
    }

    /// <summary>
    /// サービス設定
    /// </summary>
    /// <param name="services"></param>
    public void ConfigureServices(IServiceCollection services)
    {
        // サービス追加
        services.AddGodotServices();
        // services.AddSingleton<IAppRepo, AppRepo>();
        // services.AddSingleton<IInstantiator>(new Instantiator(GetTree()));
        services.AddSingleton<ClueCsvRepository>();
    }

#if DEBUG
    private void RunTests()
      => _ = GoTest.RunTests(Assembly.GetExecutingAssembly(), this, Environment);
#endif

    private void RunScene()
      => GetTree().ChangeSceneToFile("res://src/app/App.tscn");
}
