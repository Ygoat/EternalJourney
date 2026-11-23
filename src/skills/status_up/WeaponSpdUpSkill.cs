namespace EternalJourney.Skills;

using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using EternalJourney.Cores.Models.Skill;

public interface IWeaponSpdUpSkill : ISkillNode { }

[Meta(typeof(IAutoNode))]
public partial class WeaponSpdUpSkill : StatusUpSkillBase, IWeaponSpdUpSkill
{
    protected override SkillType SkillKind => SkillType.WeaponSpdUp;
    protected override void ApplyEffect(int stackCount) => BattleRepo.WeaponSpdMultiplier = 1.0f + stackCount * 0.2f;
    protected override void ResetEffect() => BattleRepo.WeaponSpdMultiplier = 1.0f;
}
