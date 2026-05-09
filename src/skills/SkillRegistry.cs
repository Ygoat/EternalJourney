namespace EternalJourney.Skills;

using EternalJourney.Cores.Models.Skill;

public static class SkillRegistry
{
    public static readonly SkillType[] SelectableSkills =
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

    public static ISkillNode CreateNode(SkillType type) => type switch
    {
        SkillType.ShipAtkUp   => new ShipAtkUpSkill(),
        SkillType.ShipSpdUp   => new ShipSpdUpSkill(),
        SkillType.ShipDefUp   => new ShipDefUpSkill(),
        SkillType.WeaponAtkUp => new WeaponAtkUpSkill(),
        SkillType.WeaponSpdUp => new WeaponSpdUpSkill(),
        SkillType.BulletAtkUp => new BulletAtkUpSkill(),
        SkillType.BulletSpdUp => new BulletSpdUpSkill(),
        SkillType.BulletDefUp => new BulletDefUpSkill(),
        SkillType.Heal        => new HealSkill(),
        SkillType.Regen       => new RegenSkill(),
        _                     => throw new System.ArgumentOutOfRangeException(nameof(type)),
    };
}
