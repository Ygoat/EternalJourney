namespace EternalJourney;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Cores.Utils;
using Godot;
using Godot.DependencyInjection.Attributes;

public interface IApp : ICanvasLayer, IProvide<IAppRepo>;

[Meta(typeof(IAutoNode))]
public partial class App : CanvasLayer, IApp
{
    public override void _Notification(int what) => this.Notify(what);

    public IGame Game { get; set; } = default!;

    [Inject]
    private readonly IInstantiator instantiator = default!;

    [Inject]
    private readonly IAppRepo appRepo = default!;

    IAppRepo IProvide<IAppRepo>.Value() => AppRepo;

    public IAppRepo AppRepo { get; set; } = default!;

    public void Initialize()
    {

    }
}
