namespace EternalJourney.Cores.Models.Skill;

public enum SpSkillType
{
    Stun,
    BigDamage,
    AttackDown,
    FireRateDown,
}

public static class SpSkillInfo
{
    public static string GetName(SpSkillType type) => type switch
    {
        SpSkillType.Stun         => "スタン",
        SpSkillType.BigDamage    => "大ダメージ",
        SpSkillType.AttackDown   => "攻撃力減少",
        SpSkillType.FireRateDown => "発射間隔増大",
        _                        => string.Empty,
    };
}
