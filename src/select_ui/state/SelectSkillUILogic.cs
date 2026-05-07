namespace EternalJourney.SelectUI.State;

using System.Collections.Generic;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Cores.Models.Skill;
using EternalJourney.Game.Domain;
using EternalJourney.SelectUI;

/// <summary>
/// スキル選択UIロジックインターフェース
/// </summary>
public interface ISelectSkillUILogic : ILogicBlock<SelectSkillUILogic.State>;

/// <summary>
/// スキル選択UIロジック
/// </summary>
[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class SelectSkillUILogic : LogicBlock<SelectSkillUILogic.State>, ISelectSkillUILogic
{
    public override Transition GetInitialState() => To<State.Idle>();

    private static readonly SkillType[] SkillTypeMap =
    {
        SkillType.ShipAtkUp,
        SkillType.ShipSpdUp,
        SkillType.ShipDefUp,
        SkillType.WeaponAtkUp,
        SkillType.WeaponSpdUp,
        SkillType.BulletAtkUp,
        SkillType.BulletSpdUp,
        SkillType.BulletDefUp,
    };

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

    public abstract record State : StateLogic<State>
    {
        /// <summary>
        /// 待機
        /// </summary>
        public record Idle : State, IGet<Input.SkillToggled>, IGet<Input.SelectButtonPressed>
        {
            public Idle()
            {
                OnAttach(() =>
                {
                    // TODO: メソッド化
                    ISelectSkillUI ui = Get<ISelectSkillUI>();
                    IButton[] buttons = GetButtons(ui);
                    for (int i = 0; i < buttons.Length; i++)
                    {
                        int index = i;
                        buttons[i].ToggleMode = true;
                        buttons[i].Text = SkillTypeMap[i] switch
                        {
                            SkillType.ShipAtkUp    => "Ship ATK Up",
                            SkillType.ShipSpdUp    => "Ship SPD Up",
                            SkillType.ShipDefUp    => "Ship DEF Up",
                            SkillType.WeaponAtkUp  => "Weapon ATK Up",
                            SkillType.WeaponSpdUp  => "Weapon SPD Up",
                            SkillType.BulletAtkUp  => "Bullet ATK Up",
                            SkillType.BulletSpdUp  => "Bullet SPD Up",
                            SkillType.BulletDefUp  => "Bullet DEF Up",
                            _                      => "",
                        };
                        buttons[i].Toggled += pressed => Input(new Input.SkillToggled(index, pressed));
                    }
                    ui.SelectButton.Pressed += () => Input(new Input.SelectButtonPressed());
                });
            }

            public Transition On(in Input.SkillToggled input)
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
                    Get<ISelectSkillUI>().SkillDescription.Text = SkillInfo.GetDescription(SkillTypeMap[input.Index]);
                }
                else
                {
                    selected.Remove(input.Index);
                }
                return ToSelf();
            }

            public Transition On(in Input.SelectButtonPressed input)
            {
                // TODO: メソッド化
                var selected = Get<HashSet<int>>();
                var skills = new List<SkillType>();
                foreach (int i in selected)
                    skills.Add(SkillTypeMap[i]);
                IGameRepo gameRepo = Get<IGameRepo>();
                gameRepo.SetSelectedSkills(skills);
                gameRepo.NotifySkillSelected();
                return ToSelf();
            }

            private static IButton[] GetButtons(ISelectSkillUI ui) =>
                new[] { ui.Skill1, ui.Skill2, ui.Skill3, ui.Skill4, ui.Skill5, ui.Skill6, ui.Skill7, ui.Skill8 };
        }
    }
}
