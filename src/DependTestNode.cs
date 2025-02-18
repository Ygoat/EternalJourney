using System;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using EternalJourney;
using Godot;
using Godot.DependencyInjection.Attributes;

[Meta(typeof(IAutoNode))]
public partial class DependTestNode : Node
{
    public override void _Notification(int what) => this.Notify(what);

    // 依存関係として提供されたstring型の値を受け取ります
    [Dependency]
    public string MyDependency => this.DependOn<string>();

    [Inject]
    private readonly TestService TestService;

    // 依存関係が解決された後に呼ばれるメソッド
    public void OnResolved()
    {
        // 依存関係が解決されたので、値を利用して処理を行います。
        TestService.Hey($"Resolved Dependency: {MyDependency}");
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
