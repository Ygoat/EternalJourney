namespace EternalJourney.Cores.Consts;

public static class CollisionEntity
{
    public const uint Ship = 1u << 0;
    public const uint Crew = 1u << 1;
    public const uint Enemy = 1u << 2;
    public const uint Weapon = 1u << 3;
    public const uint PlayerBullet = 1u << 4;
    public const uint Radar = 1u << 5;
    public const uint EnemyBullet = 1u << 6;
    public const uint Scrap = 1u << 7;
    public const uint MagnetPulse = 1u << 8;
}
