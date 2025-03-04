namespace EternalJourney;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;

public interface IApp : ICanvasLayer, IProvide<IAppRepo>;

[Meta(typeof(IAutoNode))]
public partial class App : CanvasLayer, IApp
{
    public override void _Notification(int what) => this.Notify(what);

    public IGame Game { get; set; } = default!;

    IAppRepo IProvide<IAppRepo>.Value() => AppRepo;

    public IAppRepo AppRepo { get; set; } = default!;


}
