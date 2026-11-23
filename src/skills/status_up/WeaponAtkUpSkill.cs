namespace EternalJourney.Skills;

using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using EternalJourney.Cores.Models.Skill;

public interface IWeaponAtkUpSkill : ISkillNode { }

[Meta(typeof(IAutoNode))]
public partial class WeaponAtkUpSkill : StatusUpSkillBase, IWeaponAtkUpSkill
{
    protected override SkillType SkillKind => SkillType.WeaponAtkUp;
    protected override void ApplyEffect(int stackCount) => BattleRepo.WeaponAtkMultiplier = 1.0f + stackCount * 0.5f;
    protected override void ResetEffect() => BattleRepo.WeaponAtkMultiplier = 1.0f;
}
