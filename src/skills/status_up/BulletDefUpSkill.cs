namespace EternalJourney.Skills;

using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using EternalJourney.Cores.Models.Skill;

public interface IBulletDefUpSkill : ISkillNode { }

[Meta(typeof(IAutoNode))]
public partial class BulletDefUpSkill : StatusUpSkillBase, IBulletDefUpSkill
{
    protected override SkillType SkillKind => SkillType.BulletDefUp;
    protected override void ApplyEffect(int stackCount) => BattleRepo.BulletDefBonus = stackCount * 3.0f;
    protected override void ResetEffect() => BattleRepo.BulletDefBonus = 0.0f;
}
