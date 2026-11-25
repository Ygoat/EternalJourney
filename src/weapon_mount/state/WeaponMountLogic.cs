namespace EternalJourney.WeaponMount.State;

using System;
using System.Collections.Generic;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Battle.Domain;
using EternalJourney.Cores.Models.Weapon;
using EternalJourney.Game.Domain;

/// <summary>
/// 武器搭載ロジックインターフェース
/// </summary>
public interface IWeaponMountLogic : ILogicBlock;

/// <summary>
/// 武器搭載ロジック（バトル開始時に、選択された武器を各銃座へ搭載する）
/// </summary>
[Meta]
public partial class WeaponMountLogic : LogicBlock, IWeaponMountLogic
{
    public WeaponMountLogic()
    {
        // ステートを事前確保
        Set(new WeaponMountState.Waiting());
        Set(new WeaponMountState.Mounted());
    }

    /// <summary>
    /// 入力定義
    /// </summary>
    public static class Input
    {
        /// <summary>
        /// バトル開始（Repo から通知）
        /// </summary>
        public readonly record struct BattleStarted;
    }

    /// <summary>
    /// 出力定義
    /// </summary>
    public static class Output
    {
        /// <summary>
        /// 銃座に武器を搭載する
        /// </summary>
        public readonly record struct Mount(int Slot, string ScenePath);
    }
}

/// <summary>
/// 武器搭載状態定義
/// </summary>
[StateDiagram]
public abstract partial record WeaponMountState : LogicBlockState
{
    /// <summary>
    /// バトル開始待ち
    /// </summary>
    public record Waiting : WeaponMountState, IGet<WeaponMountLogic.Input.BattleStarted>
    {
        public Waiting()
        {
            this.OnEnter(() => Get<IBattleRepo>().BattleStarted += OnBattleStarted);
            this.OnExit(() => Get<IBattleRepo>().BattleStarted -= OnBattleStarted);
        }

        private void OnBattleStarted() => Input(new WeaponMountLogic.Input.BattleStarted());

        public Type On(in WeaponMountLogic.Input.BattleStarted input) => To<Mounted>();
    }

    /// <summary>
    /// 搭載済み
    /// </summary>
    public record Mounted : WeaponMountState
    {
        public Mounted()
        {
            this.OnEnter(() =>
            {
                IReadOnlyList<WeaponType> weapons = Get<IGameRepo>().SelectedWeapons;
                for (int slot = 0; slot < WeaponCatalog.SLOT_COUNT; slot++)
                {
                    WeaponType type = slot < weapons.Count ? weapons[slot] : WeaponCatalog.DefaultLoadout[slot];
                    Output(new WeaponMountLogic.Output.Mount(slot, WeaponCatalog.Get(type).ScenePath));
                }
            });
        }
    }
}
