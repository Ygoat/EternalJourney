namespace EternalJourney;

using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;
using Godot.DependencyInjection.Attributes;

[Meta(typeof(IAutoNode))] // 子Nodeへ親Nodeの値をDIするために必要なミックスイン
public partial class Game : Control, IProvide<string>
{
    // 子Nodeへ親Nodeの値をDIするために必要
    public override void _Notification(int what) => this.Notify(what);

    string IProvide<string>.Value() => "Hello from Provider!";

    public Button TestButton { get; private set; } = default!;
    public int ButtonPresses { get; private set; }

    // サービスの注入
    [Inject]
    private readonly TestService TestServie = null!;

    public override void _Ready()
      => TestButton = GetNode<Button>("%TestButton");

    // OnReadyでProvide()を呼び出して依存関係を提供します。
    public void OnReady()
    {
        this.Provide(); // 依存関係の提供を通知
    }

    public void OnTestButtonPressed()
    {
        GD.Print(ButtonPresses++);
        TestServie.Hey("Hello Service");
    }
}
