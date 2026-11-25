namespace EternalJourney.SelectUI.State;

using System;
using System.Collections.Generic;
using System.Linq;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Cores.Models.Skill;
using EternalJourney.Game.Domain;

/// <summary>
/// SPスキル選択UIロジックインターフェース
/// </summary>
public interface ISelectSpSkillUILogic : ILogicBlock;

/// <summary>
/// SPスキル選択UIロジック（SPスキルは1つだけ選択できる）
/// </summary>
[Meta]
public partial class SelectSpSkillUILogic : LogicBlock, ISelectSpSkillUILogic
{
    public SelectSpSkillUILogic()
    {
        // ステートを事前確保
        Set(new SelectSpSkillUIState.Idle());
    }

    /// <summary>
    /// 未選択時の説明文
    /// </summary>
    public const string NOT_SELECTED_DESCRIPTION = "SPスキルを1つ選択してください";

    /// <summary>
    /// 未選択を表すインデックス
    /// </summary>
    public const int NO_SELECTION = -1;

    /// <summary>
    /// 入力定義
    /// </summary>
    public static class Input
    {
        /// <summary>
        /// SPスキルボタンのトグル
        /// </summary>
        public readonly record struct SpSkillToggled(int Index, bool Pressed);

        /// <summary>
        /// 決定ボタン押下
        /// </summary>
        public readonly record struct ConfirmPressed;

        /// <summary>
        /// ランダムボタン押下
        /// </summary>
        public readonly record struct RandomPressed;
    }

    /// <summary>
    /// 出力定義
    /// </summary>
    public static class Output
    {
        /// <summary>
        /// 選択肢の表示名
        /// </summary>
        public readonly record struct Initialized(IReadOnlyList<string> Names);

        /// <summary>
        /// 選択状態の変化
        /// </summary>
        /// <param name="SelectedIndex">選択中のインデックス（未選択は NO_SELECTION）</param>
        /// <param name="Description">説明欄に表示する文字列</param>
        /// <param name="CanConfirm">決定できるか</param>
        public readonly record struct SelectionChanged(int SelectedIndex, string Description, bool CanConfirm);
    }
}

/// <summary>
/// SPスキル選択UI状態定義
/// </summary>
[StateDiagram]
public abstract partial record SelectSpSkillUIState : LogicBlockState
{
    /// <summary>
    /// 選択待ち
    /// </summary>
    public record Idle : SelectSpSkillUIState,
        IGet<SelectSpSkillUILogic.Input.SpSkillToggled>,
        IGet<SelectSpSkillUILogic.Input.ConfirmPressed>,
        IGet<SelectSpSkillUILogic.Input.RandomPressed>
    {
        /// <summary>
        /// 選択中のインデックス（OnEnterで初期化する）
        /// </summary>
        private int _selected = SelectSpSkillUILogic.NO_SELECTION;

        public Idle()
        {
            this.OnEnter(() =>
            {
                _selected = SelectSpSkillUILogic.NO_SELECTION;
                Output(new SelectSpSkillUILogic.Output.Initialized(SpSkillCatalog.All.Select(d => d.Name).ToArray()));
                NotifySelection();
            });
        }

        public Type On(in SelectSpSkillUILogic.Input.SpSkillToggled input)
        {
            if (input.Index < 0 || input.Index >= SpSkillCatalog.All.Count)
            {
                return ToSelf();
            }
            if (input.Pressed)
            {
                // 押されたものだけを選択状態にする（前の選択は外れる）
                _selected = input.Index;
            }
            else if (input.Index == _selected)
            {
                _selected = SelectSpSkillUILogic.NO_SELECTION;
            }
            NotifySelection();
            return ToSelf();
        }

        public Type On(in SelectSpSkillUILogic.Input.RandomPressed input)
        {
            _selected = Get<Random>().Next(SpSkillCatalog.All.Count);
            NotifySelection();
            return ToSelf();
        }

        public Type On(in SelectSpSkillUILogic.Input.ConfirmPressed input)
        {
            if (_selected == SelectSpSkillUILogic.NO_SELECTION)
            {
                return ToSelf();
            }
            IGameRepo gameRepo = Get<IGameRepo>();
            gameRepo.SetSelectedSpSkill(SpSkillCatalog.All[_selected].Type);
            gameRepo.NotifySpSkillSelected();
            return ToSelf();
        }

        private void NotifySelection()
        {
            bool selected = _selected != SelectSpSkillUILogic.NO_SELECTION;
            string description = selected
                ? SpSkillCatalog.All[_selected].Description
                : SelectSpSkillUILogic.NOT_SELECTED_DESCRIPTION;
            Output(new SelectSpSkillUILogic.Output.SelectionChanged(_selected, description, selected));
        }
    }
}
