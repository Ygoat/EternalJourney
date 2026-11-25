namespace EternalJourney.SelectUI.State;

using System;
using System.Collections.Generic;
using System.Linq;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Cores.Models.Skill;
using EternalJourney.Cores.Utils;
using EternalJourney.Game.Domain;
using EternalJourney.Skills;

/// <summary>
/// スキル選択UIロジックインターフェース
/// </summary>
public interface ISelectSkillUILogic : ILogicBlock;

/// <summary>
/// スキル選択UIロジック（最大4つまで選ぶ）
/// </summary>
[Meta]
public partial class SelectSkillUILogic : LogicBlock, ISelectSkillUILogic
{
    public SelectSkillUILogic()
    {
        // ステートを事前確保
        Set(new SelectSkillUIState.Idle());
    }

    /// <summary>
    /// 選べるスキルの最大数
    /// </summary>
    public const int MAX_SKILLS = 4;

    /// <summary>
    /// 何も選んでいないときの説明文
    /// </summary>
    public const string GUIDE_TEXT = "スキルを4つまで選択してください";

    /// <summary>
    /// 入力定義
    /// </summary>
    public static class Input
    {
        /// <summary>
        /// スキルトグル
        /// </summary>
        public readonly record struct SkillToggled(int Index, bool Pressed);

        /// <summary>
        /// ランダムボタン押下
        /// </summary>
        public readonly record struct RandomPressed;

        /// <summary>
        /// 選択ボタン押下
        /// </summary>
        public readonly record struct SelectButtonPressed;
    }

    /// <summary>
    /// 出力定義
    /// </summary>
    public static class Output
    {
        /// <summary>
        /// スキルの表示名
        /// </summary>
        public readonly record struct Initialized(IReadOnlyList<string> Names);

        /// <summary>
        /// 選択状態の変化
        /// </summary>
        /// <param name="Selected">スキルごとの選択状態</param>
        /// <param name="Description">説明欄の文字列</param>
        public readonly record struct SelectionChanged(IReadOnlyList<bool> Selected, string Description);
    }
}

/// <summary>
/// スキル選択UI状態定義
/// </summary>
[StateDiagram]
public abstract partial record SelectSkillUIState : LogicBlockState
{
    /// <summary>
    /// 待機
    /// </summary>
    public record Idle : SelectSkillUIState,
        IGet<SelectSkillUILogic.Input.SkillToggled>,
        IGet<SelectSkillUILogic.Input.RandomPressed>,
        IGet<SelectSkillUILogic.Input.SelectButtonPressed>
    {
        // 選択状態（選んだ順。OnEnterで初期化する）
        private readonly List<int> _selected = new();
        private string _description = SelectSkillUILogic.GUIDE_TEXT;

        public Idle()
        {
            this.OnEnter(() =>
            {
                _selected.Clear();
                _description = SelectSkillUILogic.GUIDE_TEXT;
                Output(new SelectSkillUILogic.Output.Initialized(
                    SkillRegistry.SelectableSkills.Select(SkillInfo.GetName).ToArray()));
                NotifySelection();
            });
        }

        public Type On(in SelectSkillUILogic.Input.SkillToggled input)
        {
            if (input.Index < 0 || input.Index >= SkillRegistry.SelectableSkills.Length)
            {
                return ToSelf();
            }
            if (input.Pressed && !_selected.Contains(input.Index) && _selected.Count < SelectSkillUILogic.MAX_SKILLS)
            {
                _selected.Add(input.Index);
                SkillType skill = SkillRegistry.SelectableSkills[input.Index];
                _description = $"{SkillInfo.GetDescription(skill)}\nクールタイム {SkillInfo.GetCooldown(skill):0}秒";
            }
            else if (!input.Pressed)
            {
                _selected.Remove(input.Index);
            }
            // 5つ目は選択状態に戻さない（NotifySelection で押下が解除される）
            NotifySelection();
            return ToSelf();
        }

        public Type On(in SelectSkillUILogic.Input.RandomPressed input)
        {
            _selected.Clear();
            _selected.AddRange(RandomPick.Distinct(Get<Random>(), SkillRegistry.SelectableSkills.Length, SelectSkillUILogic.MAX_SKILLS));
            _description = string.Join("\n", _selected.Select(i =>
            {
                SkillType skill = SkillRegistry.SelectableSkills[i];
                return $"{SkillInfo.GetName(skill)}（クールタイム {SkillInfo.GetCooldown(skill):0}秒）";
            }));
            NotifySelection();
            return ToSelf();
        }

        public Type On(in SelectSkillUILogic.Input.SelectButtonPressed input)
        {
            IGameRepo gameRepo = Get<IGameRepo>();
            gameRepo.SetSelectedSkills(_selected.Select(i => SkillRegistry.SelectableSkills[i]).ToList());
            gameRepo.NotifySkillSelected();
            return ToSelf();
        }

        private void NotifySelection()
        {
            bool[] selected = Enumerable.Range(0, SkillRegistry.SelectableSkills.Length).Select(_selected.Contains).ToArray();
            Output(new SelectSkillUILogic.Output.SelectionChanged(selected, _description));
        }
    }
}
