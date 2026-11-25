namespace EternalJourney.Crew.State;

using System;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Battle.Domain;
using EternalJourney.Cores.Models.Crews;
using EternalJourney.Crew.Domain;

/// <summary>
/// 作業クルーロジックインターフェース
/// </summary>
public interface ICrewMemberLogic : ILogicBlock;

/// <summary>
/// 作業クルーロジック
/// 指揮官に指示された配置先（銃座または修理ポイント）へ移動し、着いたら射撃担当・修理を行う
/// </summary>
[Meta]
public partial class CrewMemberLogic : LogicBlock, ICrewMemberLogic
{
    public CrewMemberLogic()
    {
        // ステートを事前確保
        Set(new CrewMemberState.Standby());
        Set(new CrewMemberState.Moving());
        Set(new CrewMemberState.Manning());
        Set(new CrewMemberState.Repairing());
    }

    /// <summary>
    /// 修理ポイントで毎秒回復する船体 HP（修理速度の補正前）
    /// </summary>
    public const float REPAIR_PER_SECOND = 10f;

    /// <summary>
    /// 入力定義
    /// </summary>
    public static class Input
    {
        /// <summary>
        /// 配置先の指示（移動してから着任する）
        /// </summary>
        public readonly record struct Assign(int Station);

        /// <summary>
        /// 配置先へ直接着任する（バトル開始時の初期配置）
        /// </summary>
        public readonly record struct Place(int Station);

        /// <summary>
        /// 目的地に着いた
        /// </summary>
        public readonly record struct Arrived;

        /// <summary>
        /// 時間経過
        /// </summary>
        public readonly record struct Tick(double Delta);
    }

    /// <summary>
    /// 出力定義
    /// </summary>
    public static class Output
    {
        /// <summary>
        /// 配置先へ移動を始める（REPAIR は修理ポイント）
        /// </summary>
        public readonly record struct MoveTo(int Station);

        /// <summary>
        /// 配置先に直接置く
        /// </summary>
        public readonly record struct PlaceAt(int Station);

        /// <summary>
        /// 着任した（射撃担当または修理中）
        /// </summary>
        public readonly record struct Working(int Station);
    }
}

/// <summary>
/// 作業クルー状態定義
/// </summary>
[StateDiagram]
public abstract partial record CrewMemberState : LogicBlockState,
    IGet<CrewMemberLogic.Input.Assign>,
    IGet<CrewMemberLogic.Input.Place>
{
    /// <summary>
    /// 現在の配置先（着任中の状態では担当している場所）
    /// </summary>
    protected virtual int CurrentStation => int.MinValue;

    public Type On(in CrewMemberLogic.Input.Assign input)
    {
        if (input.Station == CurrentStation)
        {
            return ToSelf();
        }
        Get<Moving>().Destination = input.Station;
        if (this is Moving)
        {
            // 移動中の行き先変更は同じ状態への遷移で OnEnter が走らないため、ここで通知する
            Output(new CrewMemberLogic.Output.MoveTo(input.Station));
            return ToSelf();
        }
        return To<Moving>();
    }

    public Type On(in CrewMemberLogic.Input.Place input)
    {
        Output(new CrewMemberLogic.Output.PlaceAt(input.Station));
        return Arrive(input.Station);
    }

    /// <summary>
    /// 配置先に着任する
    /// </summary>
    protected Type Arrive(int station)
    {
        if (station == CrewCommander.REPAIR)
        {
            return To<Repairing>();
        }
        Get<Manning>().Station = station;
        return To<Manning>();
    }

    /// <summary>
    /// 配置待ち（バトル開始前）
    /// </summary>
    public record Standby : CrewMemberState;

    /// <summary>
    /// 移動中（射撃も修理もしない）
    /// </summary>
    public record Moving : CrewMemberState, IGet<CrewMemberLogic.Input.Arrived>
    {
        /// <summary>
        /// 目的地（遷移前に設定される）
        /// </summary>
        public int Destination { get; set; }

        protected override int CurrentStation => Destination;

        public Moving()
        {
            this.OnEnter(() => Output(new CrewMemberLogic.Output.MoveTo(Destination)));
        }

        public Type On(in CrewMemberLogic.Input.Arrived input) => Arrive(Destination);
    }

    /// <summary>
    /// 銃座を操作中
    /// </summary>
    public record Manning : CrewMemberState
    {
        /// <summary>
        /// 担当している銃座（遷移前に設定される）
        /// </summary>
        public int Station { get; set; }

        protected override int CurrentStation => Station;

        public Manning()
        {
            this.OnEnter(() =>
            {
                Get<ICrewRepo>().SetManned(Station, Get<CrewDefinition>());
                Output(new CrewMemberLogic.Output.Working(Station));
            });
            this.OnExit(() => Get<ICrewRepo>().SetManned(Station, null));
        }
    }

    /// <summary>
    /// 修理中
    /// </summary>
    public record Repairing : CrewMemberState, IGet<CrewMemberLogic.Input.Tick>
    {
        protected override int CurrentStation => CrewCommander.REPAIR;

        public Repairing()
        {
            this.OnEnter(() => Output(new CrewMemberLogic.Output.Working(CrewCommander.REPAIR)));
        }

        public Type On(in CrewMemberLogic.Input.Tick input)
        {
            float multiplier = CrewBonus.RepairMultiplier(Get<ICrewRepo>().Commander, Get<CrewDefinition>());
            Get<IBattleRepo>().RequestShipRepair(CrewMemberLogic.REPAIR_PER_SECOND * multiplier * (float)input.Delta);
            return ToSelf();
        }
    }
}
