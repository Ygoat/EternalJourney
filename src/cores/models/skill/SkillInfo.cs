namespace EternalJourney.Cores.Models.Skill;

public static class SkillInfo
{
    public static string GetName(SkillType type) => type switch
    {
        SkillType.ShipAtkUp   => "Ship ATK Up",
        SkillType.ShipSpdUp   => "Ship SPD Up",
        SkillType.ShipDefUp   => "Ship DEF Up",
        SkillType.WeaponAtkUp => "Weapon ATK Up",
        SkillType.WeaponSpdUp => "Weapon SPD Up",
        SkillType.BulletAtkUp => "Bullet ATK Up",
        SkillType.BulletSpdUp => "Bullet SPD Up",
        SkillType.BulletDefUp => "Bullet DEF Up",
        SkillType.Heal        => "Heal",
        SkillType.Regen       => "Regen",
        _                     => "",
    };

    public static string GetDescription(SkillType type) => type switch
    {
        SkillType.ShipAtkUp   => "自機の攻撃補正を強化する。\nスタックごとに攻撃力+30%。最大4スタック。持続10秒。",
        SkillType.ShipSpdUp   => "自機の移動速度を上昇させる。\nスタックごとに速度+20%。最大4スタック。持続10秒。",
        SkillType.ShipDefUp   => "自機の防御力を強化する。\nスタックごとに防御力+5。最大4スタック。持続10秒。",
        SkillType.WeaponAtkUp => "武器の攻撃倍率を強化する。\nスタックごとに攻撃力+50%。最大4スタック。持続10秒。",
        SkillType.WeaponSpdUp => "武器の発射速度を上昇させる。\nスタックごとに発射速度+20%。最大4スタック。持続10秒。",
        SkillType.BulletAtkUp => "弾丸の攻撃倍率を強化する。\nスタックごとに攻撃力+25%。最大4スタック。持続10秒。",
        SkillType.BulletSpdUp => "弾丸の飛行速度を上昇させる。\nスタックごとに弾速+15%。最大4スタック。持続10秒。",
        SkillType.BulletDefUp => "弾丸の防御貫通力を高める。\nスタックごとに貫通値+3。最大4スタック。持続10秒。",
        SkillType.Heal        => "自機のHPを即時回復する。",
        SkillType.Regen       => "一定時間、毎秒HPを回復し続ける。\n効果は重ねがけ可能。",
        _                     => "",
    };
}
