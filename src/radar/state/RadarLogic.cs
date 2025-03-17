namespace EternalJourney.Radar.State;

using System.Collections.Generic;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using Godot;

/// <summary>
/// レーダーロジックインターフェース
/// </summary>
public interface IRadarLogic : ILogicBlock<RadarLogic.State>;

/// <summary>
/// レーダーロジッククラス
/// </summary>
[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class RadarLogic : LogicBlock<RadarLogic.State>, IRadarLogic
{
    /// <summary>
    /// 初期状態
    /// </summary>
    /// <returns></returns>
    public override Transition GetInitialState() => To<State.Idle>();

    /// <summary>
    /// 入力
    /// </summary>
    public static class Input
    {
        /// <summary>
        /// 敵発見
        /// TODO:変える
        /// </summary>
        /// <param name="Enemy"></param>
        public readonly record struct WatchEnemy(Area2D Enemy);

        /// <summary>
        /// 物理処理
        /// TODO:変える
        /// </summary>
        /// <param name="Enemies"></param>
        public readonly record struct PhysicProcess(List<Node2D> Enemies);
    }

    /// <summary>
    /// 出力
    /// </summary>
    public static class Output
    {
        /// <summary>
        /// ステータス変更
        /// TODO:変える
        /// </summary>
        /// <param name="IsOn"></param>
        public readonly record struct StatusChanged(bool IsOn);
    }

    /// <summary>
    /// 状態
    /// </summary>
    public abstract record State : StateLogic<State>
    {
        /// <summary>
        /// アイドル
        /// </summary>
        public record Idle : State, IGet<Input.WatchEnemy>
        {
            public Idle()
            {
                this.OnEnter(() => Output(new Output.StatusChanged(IsOn: true)));
            }

            public Transition On(in Input.WatchEnemy input)
            {
                return To<EnemySearched>();
            }
        }

        /// <summary>
        /// 敵発見
        /// </summary>
        public record EnemySearched : State, IGet<Input.PhysicProcess>
        {
            public EnemySearched()
            {
                this.OnEnter(() => Output(new Output.StatusChanged(IsOn: false)));
            }

            public Transition On(in Input.PhysicProcess input)
            {
                if (input.Enemies.Count == 0)
                {
                    return To<Idle>();
                }
                return ToSelf();
            }
        }
    }
}
