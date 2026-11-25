namespace EternalJourney.SelectUI.State;

using System;
using System.Collections.Generic;
using System.Linq;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Cores.Models.Weapon;
using EternalJourney.Game.Domain;

/// <summary>
/// 武器選択UIロジックインターフェース
/// </summary>
public interface ISelectWeaponUILogic : ILogicBlock;

/// <summary>
/// 武器選択UIロジック（4つの銃座それぞれに武器を選ぶ。重複可）
/// </summary>
[Meta]
public partial class SelectWeaponUILogic : LogicBlock, ISelectWeaponUILogic
{
    public SelectWeaponUILogic()
    {
        // ステートを事前確保
        Set(new SelectWeaponUIState.Idle());
    }

    /// <summary>
    /// 入力定義
    /// </summary>
    public static class Input
    {
        /// <summary>
        /// 銃座の武器を選んだ
        /// </summary>
        public readonly record struct WeaponChosen(int Slot, int Index);

        /// <summary>
        /// ランダムボタン押下
        /// </summary>
        public readonly record struct RandomPressed;

        /// <summary>
        /// 決定ボタン押下
        /// </summary>
        public readonly record struct ConfirmPressed;
    }

    /// <summary>
    /// 出力定義
    /// </summary>
    public static class Output
    {
        /// <summary>
        /// 銃座名と武器名
        /// </summary>
        public readonly record struct Initialized(IReadOnlyList<string> SlotNames, IReadOnlyList<string> WeaponNames);

        /// <summary>
        /// 選択状態の変化
        /// </summary>
        /// <param name="Selected">銃座ごとに選ばれている武器の番号</param>
        /// <param name="Description">説明欄の文字列</param>
        public readonly record struct SelectionChanged(IReadOnlyList<int> Selected, string Description);
    }

    /// <summary>
    /// 選択中の武器構成の説明文（銃座ごとに1行）
    /// </summary>
    public static string Describe(IReadOnlyList<int> selected) =>
        string.Join("\n", selected.Select((index, slot) =>
        {
            WeaponDefinition def = WeaponCatalog.All[index];
            return $"{WeaponCatalog.SlotName(slot)}：{def.Name}（{def.Description}）";
        }));
}

/// <summary>
/// 武器選択UI状態定義
/// </summary>
[StateDiagram]
public abstract partial record SelectWeaponUIState : LogicBlockState
{
    /// <summary>
    /// 選択待ち
    /// </summary>
    public record Idle : SelectWeaponUIState,
        IGet<SelectWeaponUILogic.Input.WeaponChosen>,
        IGet<SelectWeaponUILogic.Input.RandomPressed>,
        IGet<SelectWeaponUILogic.Input.ConfirmPressed>
    {
        /// <summary>
        /// 銃座ごとの武器の番号（OnEnterで初期化する）
        /// </summary>
        private readonly int[] _selected = new int[WeaponCatalog.SLOT_COUNT];

        public Idle()
        {
            this.OnEnter(() =>
            {
                IReadOnlyList<WeaponType> current = Get<IGameRepo>().SelectedWeapons;
                for (int slot = 0; slot < _selected.Length; slot++)
                {
                    WeaponType type = slot < current.Count ? current[slot] : WeaponCatalog.DefaultLoadout[slot];
                    _selected[slot] = Math.Max(0, WeaponCatalog.IndexOf(type));
                }
                Output(new SelectWeaponUILogic.Output.Initialized(
                    Enumerable.Range(0, WeaponCatalog.SLOT_COUNT).Select(WeaponCatalog.SlotName).ToArray(),
                    WeaponCatalog.All.Select(d => d.Name).ToArray()));
                NotifySelection();
            });
        }

        public Type On(in SelectWeaponUILogic.Input.WeaponChosen input)
        {
            if (input.Slot >= 0 && input.Slot < _selected.Length
                && input.Index >= 0 && input.Index < WeaponCatalog.All.Count)
            {
                _selected[input.Slot] = input.Index;
            }
            NotifySelection();
            return ToSelf();
        }

        public Type On(in SelectWeaponUILogic.Input.RandomPressed input)
        {
            Random rng = Get<Random>();
            for (int slot = 0; slot < _selected.Length; slot++)
            {
                _selected[slot] = rng.Next(WeaponCatalog.All.Count);
            }
            NotifySelection();
            return ToSelf();
        }

        public Type On(in SelectWeaponUILogic.Input.ConfirmPressed input)
        {
            IGameRepo gameRepo = Get<IGameRepo>();
            gameRepo.SetSelectedWeapons(_selected.Select(i => WeaponCatalog.All[i].Type).ToArray());
            gameRepo.NotifyWeaponSelected();
            return ToSelf();
        }

        private void NotifySelection()
        {
            int[] snapshot = _selected.ToArray();
            Output(new SelectWeaponUILogic.Output.SelectionChanged(snapshot, SelectWeaponUILogic.Describe(snapshot)));
        }
    }
}
