namespace EternalJourney.Cores.Models.Skill;

public enum SPSkillType
{
    Stun,
    BigDamage,
    AttackDown,
    FireRateDown,
}

public static class SPSkillInfo
{
    public static string GetName(SPSkillType type) => type switch
    {
        SPSkillType.Stun         => "スタン",
        SPSkillType.BigDamage    => "大ダメージ",
        SPSkillType.AttackDown   => "攻撃力減少",
        SPSkillType.FireRateDown => "発射間隔増大",
        _                        => string.Empty,
    };
}
