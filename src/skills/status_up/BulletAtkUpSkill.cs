namespace EternalJourney.Skills;

using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using EternalJourney.Cores.Models.Skill;

public interface IBulletAtkUpSkill : ISkillNode { }

[Meta(typeof(IAutoNode))]
public partial class BulletAtkUpSkill : StatusUpSkillBase, IBulletAtkUpSkill
{
    protected override SkillType SkillKind => SkillType.BulletAtkUp;
    protected override void ApplyEffect(int stackCount) => BattleRepo.BulletAtkMultiplier = 1.0f + stackCount * 0.25f;
    protected override void ResetEffect() => BattleRepo.BulletAtkMultiplier = 1.0f;
}
