namespace EternalJourney.Cores.Models.Skill;

using System;

/// <summary>
/// スキルのターゲット対象（複数指定可）
/// </summary>
[Flags]
public enum SkillTarget
{
    None = 0,
    Ship = 1 << 0,
    Weapon = 1 << 1,
    Bullet = 1 << 2,
}
