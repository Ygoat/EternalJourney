namespace EternalJourney.AutoConnectTest.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public interface ILightSwitchLogic : ILogicBlock<LightSwitchLogic.State>;

[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class LightSwitchLogic : LogicBlock<LightSwitchLogic.State>
{
    // Define your initial state here.
    public override Transition GetInitialState() => To<State.PoweredOff>();

    // By convention, inputs are defined in a static nested class called Input.
    public static class Input
    {
        public readonly record struct Toggle;
    }

    // By convention, outputs are defined in a static nested class called Output.
    public static class Output
    {
        public readonly record struct StatusChanged(bool IsOn);
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
        public record PoweredOn : State, IGet<Input.Toggle>
        {
            public PoweredOn()
            {
                // Announce that we are now on.
                this.OnEnter(() => Output(new Output.StatusChanged(IsOn: true)));
            }

            public Transition On(in Input.Toggle input) => To<PoweredOff>();
        }

        // Off state.
        public record PoweredOff : State, IGet<Input.Toggle>
        {
            public PoweredOff()
            {
                // Announce that we are now off.
                this.OnEnter(() => Output(new Output.StatusChanged(IsOn: false)));
            }

            public Transition On(in Input.Toggle input) => To<PoweredOn>();
        }
    }
}
