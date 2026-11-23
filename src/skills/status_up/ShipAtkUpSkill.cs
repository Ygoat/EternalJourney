namespace EternalJourney.Skills;

using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using EternalJourney.Cores.Models.Skill;

public interface IShipAtkUpSkill : ISkillNode { }

[Meta(typeof(IAutoNode))]
public partial class ShipAtkUpSkill : StatusUpSkillBase, IShipAtkUpSkill
{
    protected override SkillType SkillKind => SkillType.ShipAtkUp;
    protected override void ApplyEffect(int stackCount) => BattleRepo.ShipAtkBonus = stackCount * 0.3f;
    protected override void ResetEffect() => BattleRepo.ShipAtkBonus = 0.0f;
}
