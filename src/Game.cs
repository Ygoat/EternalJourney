namespace EternalJourney;

using Godot;
using Godot.DependencyInjection.Attributes;


public partial class Game : Control
{
    public Button TestButton { get; private set; } = default!;
    public int ButtonPresses { get; private set; }

    [Inject]
    private TestService TestServie { get; set; } = default;

    public override void _Ready()
      => TestButton = GetNode<Button>("%TestButton");

    // public void OnTestButtonPressed() => ButtonPresses++;
    public void OnTestButtonPressed()
    {
        GD.Print(ButtonPresses++);
        TestServie.Hey();
    }
}
