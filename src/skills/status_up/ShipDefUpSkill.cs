namespace EternalJourney.Skills;

using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using EternalJourney.Cores.Models.Skill;

public interface IShipDefUpSkill : ISkillNode { }

[Meta(typeof(IAutoNode))]
public partial class ShipDefUpSkill : StatusUpSkillBase, IShipDefUpSkill
{
    protected override SkillType SkillKind => SkillType.ShipDefUp;
    protected override void ApplyEffect(int stackCount) => BattleRepo.ShipDefBonus = stackCount * 5.0f;
    protected override void ResetEffect() => BattleRepo.ShipDefBonus = 0.0f;
}
