namespace EternalJourney.Skills;

using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using EternalJourney.Cores.Models.Skill;

public interface IShipSpdUpSkill : ISkillNode { }

[Meta(typeof(IAutoNode))]
public partial class ShipSpdUpSkill : StatusUpSkillBase, IShipSpdUpSkill
{
    protected override SkillType SkillKind => SkillType.ShipSpdUp;
    protected override void ApplyEffect(int stackCount) => BattleRepo.ShipSpdMultiplier = 1.0f + stackCount * 0.2f;
    protected override void ResetEffect() => BattleRepo.ShipSpdMultiplier = 1.0f;
}
