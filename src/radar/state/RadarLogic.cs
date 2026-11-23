namespace EternalJourney.Radar.State;

using System;
using System.Collections.Generic;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using Godot;

/// <summary>
/// レーダーロジックインターフェース
/// </summary>
public interface IRadarLogic : ILogicBlock;

/// <summary>
/// レーダーロジッククラス
/// </summary>
[Meta]
public partial class RadarLogic : LogicBlock, IRadarLogic
{
    public RadarLogic()
    {
        // ステートを事前確保
        Set(new RadarState.Idle());
        Set(new RadarState.EnemySearched());
    }


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

}

/// <summary>
/// 状態
/// </summary>
[StateDiagram]
public abstract partial record RadarState : LogicBlockState
{
    /// <summary>
    /// アイドル
    /// </summary>
    public record Idle : RadarState, IGet<RadarLogic.Input.WatchEnemy>
    {
        public Idle()
        {
            this.OnEnter(() => Output(new RadarLogic.Output.StatusChanged(IsOn: true)));
        }

        public Type On(in RadarLogic.Input.WatchEnemy input)
        {
            return To<EnemySearched>();
        }
    }

    /// <summary>
    /// 敵発見
    /// </summary>
    public record EnemySearched : RadarState, IGet<RadarLogic.Input.PhysicProcess>
    {
        public EnemySearched()
        {
            this.OnEnter(() => Output(new RadarLogic.Output.StatusChanged(IsOn: false)));
        }

        public Type On(in RadarLogic.Input.PhysicProcess input)
        {
            if (input.Enemies.Count == 0)
            {
                return To<Idle>();
            }
            return ToSelf();
        }
    }
}
