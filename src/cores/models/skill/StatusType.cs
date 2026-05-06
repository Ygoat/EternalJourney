namespace EternalJourney.Cores.Models.Skill;

using System;

/// <summary>
/// ステータスの種類（複数指定可）
/// </summary>
[Flags]
public enum StatusType
{
    None = 0,
    Atk = 1 << 0,
    Spd = 1 << 1,
}
