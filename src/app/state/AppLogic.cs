namespace EternalJourney.App.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.App.Domain;


public interface IAppLogic : ILogicBlock<AppLogic.State>;

[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class AppLogic : LogicBlock<AppLogic.State>, IAppLogic
{
    // Define your initial state here.
    public override Transition GetInitialState() => To<State.SplashScreen>();

    // By convention, inputs are defined in a static nested class called Input.
    public static class Input
    {
        public readonly record struct SplashFinished;
        public readonly record struct StartGame;
        public readonly record struct EndGame;
        public readonly record struct FadeOutFinished;
    }

    // By convention, outputs are defined in a static nested class called Output.
    public static class Output
    {
        public readonly record struct ShowSplashScreen;
        public readonly record struct HideSplashScreen;
        public readonly record struct SetupGameScene;
        public readonly record struct ShowMainMenu;
        public readonly record struct ShowGame;
        public readonly record struct RemoveExistingGame;
    }

    // To reduce unnecessary heap allocations, inputs and outputs should be
    // readonly record structs.

    // By convention, the base state type is nested inside the logic block. This
    // helps the logic block diagram generator know where to search for state
    // types.
    public abstract record State : StateLogic<State>
    {
        // Substates are sometimes nested inside their parent states to help
        // organize the code.

        // On state.
        public record SplashScreen : State, IGet<Input.SplashFinished>
        {
            public SplashScreen()
            {
                this.OnEnter(() => Output(new Output.ShowSplashScreen()));

                OnAttach(
                  () => Get<IAppRepo>().SplashScreenSkipped += OnSplashScreenSkipped
                );

                OnDetach(
                  () => Get<IAppRepo>().SplashScreenSkipped -= OnSplashScreenSkipped
                );
            }

            public Transition On(in Input.SplashFinished input) => To<MainMenu>();

            public void OnSplashScreenSkipped() =>
              Output(new Output.HideSplashScreen());
        }

        public record MainMenu : State, IGet<Input.StartGame>//, IGet<Input.LoadGame>
        {
            public MainMenu()
            {
                this.OnEnter(
                  () =>
                  {
                      Output(new Output.SetupGameScene());

                      Get<IAppRepo>().OnMainMenuEntered();

                      Output(new Output.ShowMainMenu());
                  }
                );
            }

            public Transition On(in Input.StartGame input) => To<InGame>();
        }

        public partial record InGame : State, IGet<Input.EndGame>
        {
            public InGame()
            {
                this.OnEnter(() =>
                {
                    Get<IAppRepo>().OnEnterGame();
                    Output(new Output.ShowGame());
                });
                // this.OnExit(() => Output(new Output.HideGame()));

                // OnAttach(() => Get<IAppRepo>().GameExited += OnGameExited);
                // OnDetach(() => Get<IAppRepo>().GameExited -= OnGameExited);
            }

            // public void OnGameExited(PostGameAction reason) =>
            //   Input(new Input.EndGame(reason));

            public Transition On(in Input.EndGame input)
            {
                return To<LeavingGame>();
            }
        }

        /// <summary>
        /// ダミーの実装
        /// </summary>
        public partial record LeavingGame : State, IGet<Input.FadeOutFinished>
        {

            public Transition On(in Input.FadeOutFinished input)
            {
                // We are either supposed to restart the game or go back to the main
                // menu. More complex games might have more post-game destinations,
                // but it's pretty simple for us.
                Output(new Output.RemoveExistingGame());

                // Output(new Output.SetupGameScene());
                return To<InGame>();
            }
        }
    }
}
