namespace EternalJourney.SelectUI.State;

using System;
using System.Collections.Generic;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Cores.Models.Skill;
using EternalJourney.Game.Domain;
using EternalJourney.SelectUI;
using EternalJourney.Skills;

/// <summary>
/// スキル選択UIロジックインターフェース
/// </summary>
public interface ISelectSkillUILogic : ILogicBlock;

/// <summary>
/// スキル選択UIロジック
/// </summary>
[Meta]
public partial class SelectSkillUILogic : LogicBlock, ISelectSkillUILogic
{
    public SelectSkillUILogic()
    {
        // ステートを事前確保
        Set(new SelectSkillUIState.Idle());
    }


    public static class Input
    {
        /// <summary>
        /// スキルトグル
        /// </summary>
        public readonly record struct SkillToggled(int Index, bool Pressed);

        /// <summary>
        /// 選択ボタン押下
        /// </summary>
        public readonly record struct SelectButtonPressed;
    }

}

[StateDiagram]
public abstract partial record SelectSkillUIState : LogicBlockState
{
    /// <summary>
    /// 待機
    /// </summary>
    public record Idle : SelectSkillUIState, IGet<SelectSkillUILogic.Input.SkillToggled>, IGet<SelectSkillUILogic.Input.SelectButtonPressed>
    {
        public Idle()
        {
            this.OnEnter(() =>
            {
                // TODO: メソッド化
                ISelectSkillUI ui = Get<ISelectSkillUI>();
                IButton[] buttons = GetButtons(ui);
                for (int i = 0; i < buttons.Length; i++)
                {
                    int index = i;
                    buttons[i].ToggleMode = true;
                    buttons[i].Text = SkillInfo.GetName(SkillRegistry.SelectableSkills[i]);
                    buttons[i].Toggled += pressed => Input(new SelectSkillUILogic.Input.SkillToggled(index, pressed));
                }

                ui.SelectButton.Pressed += () => Input(new SelectSkillUILogic.Input.SelectButtonPressed());
            });
        }

        public Type On(in SelectSkillUILogic.Input.SkillToggled input)
        {
            // TODO: メソッド化
            var selected = Get<HashSet<int>>();
            if (input.Pressed)
            {
                if (selected.Count >= 4)
                {
                    GetButtons(Get<ISelectSkillUI>())[input.Index].ButtonPressed = false;
                    return ToSelf();
                }
                selected.Add(input.Index);
                Get<ISelectSkillUI>().SkillDescription.Text = SkillInfo.GetDescription(SkillRegistry.SelectableSkills[input.Index]);
            }
            else
            {
                selected.Remove(input.Index);
            }
            return ToSelf();
        }

        public Type On(in SelectSkillUILogic.Input.SelectButtonPressed input)
        {
            // TODO: メソッド化
            var selected = Get<HashSet<int>>();
            var skills = new List<SkillType>();
            foreach (int i in selected)
                skills.Add(SkillRegistry.SelectableSkills[i]);
            IGameRepo gameRepo = Get<IGameRepo>();
            gameRepo.SetSelectedSkills(skills);
            gameRepo.NotifySkillSelected();
            return ToSelf();
        }

        private static IButton[] GetButtons(ISelectSkillUI ui) =>
            new[] { ui.Skill1, ui.Skill2, ui.Skill3, ui.Skill4, ui.Skill5, ui.Skill6, ui.Skill7, ui.Skill8 };

    }
}
