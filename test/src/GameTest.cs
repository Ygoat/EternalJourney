namespace EternalJourney.Game;

using System.Threading.Tasks;
using Chickensoft.GoDotTest;
using Chickensoft.GodotTestDriver;
using Godot;

public class GameTest : TestClass
{
    private Game _game = default!;
    private Fixture _fixture = default!;

    public GameTest(Node testScene) : base(testScene) { }

    [SetupAll]
    public async Task Setup()
    {
        _fixture = new Fixture(TestScene.GetTree());
        _game = await _fixture.LoadAndAddScene<Game>();
    }

    [CleanupAll]
    public void Cleanup() => _fixture.Cleanup();

    [Test]
    public void TestButtonUpdatesCounter()
    {
        // var buttonDriver = new ButtonDriver(() => _game.TestButton);
        // buttonDriver.ClickCenter();
        // _game.ButtonPresses.ShouldBe(1);
    }
}
