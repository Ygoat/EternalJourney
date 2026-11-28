namespace EternalJourney.Weapon.Base;

/// <summary>
/// 船の銃座に搭載できる武器
/// </summary>
public interface IStationWeapon
{
    /// <summary>
    /// 銃座番号（-1 はクルー不要＝敵の武器など）
    /// </summary>
    int StationIndex { get; set; }
}
