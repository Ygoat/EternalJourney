namespace EternalJourney.Radar.State;

using System.Collections.Generic;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using Godot;


public interface IRadarLogic : ILogicBlock<RadarLogic.State>;

[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class RadarLogic : LogicBlock<RadarLogic.State>, IRadarLogic
{
    // Define your initial state here.
    public override Transition GetInitialState() => To<State.Idle>();

    // By convention, inputs are defined in a static nested class called Input.
    public static class Input
    {
        public readonly record struct WatchEnemy(Area2D Enemy);
        public readonly record struct PhysicProcess(List<Node2D> Enemies);
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
        public record Idle : State, IGet<Input.WatchEnemy>
        {
            public Idle()
            {
                // Announce that we are now on.
                this.OnEnter(() => Output(new Output.StatusChanged(IsOn: true)));
            }

            public Transition On(in Input.WatchEnemy input)
            {
                // var target = input.Enemies;
                // GD.Print(target[0].Name);
                return To<EnemySearched>();
            }
        }

        // Off state.
        public record EnemySearched : State, IGet<Input.PhysicProcess>
        {
            public EnemySearched()
            {
                // Announce that we are now off.
                this.OnEnter(() => Output(new Output.StatusChanged(IsOn: false)));
            }

            public Transition On(in Input.PhysicProcess input)
            {
                if (input.Enemies.Count == 0)
                {
                    return To<Idle>();
                }
                // input.Enemies[0].QueueFree();
                return ToSelf();
            }
        }
    }
}
