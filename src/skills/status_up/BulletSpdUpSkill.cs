namespace EternalJourney.Skills;

using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using EternalJourney.Cores.Models.Skill;

public interface IBulletSpdUpSkill : ISkillNode { }

[Meta(typeof(IAutoNode))]
public partial class BulletSpdUpSkill : StatusUpSkillBase, IBulletSpdUpSkill
{
    protected override SkillType SkillKind => SkillType.BulletSpdUp;
    protected override void ApplyEffect(int stackCount) => BattleRepo.BulletSpdMultiplier = 1.0f + stackCount * 0.15f;
    protected override void ResetEffect() => BattleRepo.BulletSpdMultiplier = 1.0f;
}
