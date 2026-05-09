namespace EternalJourney.Cores.Repositories;

using System.Collections.Generic;
using EternalJourney.Cores.Models.Weapon;
using EternalJourney.Cores.Repositories.Base;
using EternalJourney.Cores.Settings;

/// <summary>
/// 武器設定リーダーインターフェース
/// </summary>
public interface IWeaponConfigReader : IBaseJsonReader<WeaponEntry>
{
    WeaponEntry? GetById(string id);
}

/// <summary>
/// 武器設定JSONリーダー
/// </summary>
public class WeaponConfigReader : BaseJsonReader<WeaponConfigRoot, WeaponEntry>, IWeaponConfigReader
{
    protected override string GetFilePath()
    {
        return $"{AppSetting.CsvFileBasePath}WeaponConfig.json";
    }

    protected override List<WeaponEntry> GetItemsFromRoot(WeaponConfigRoot root)
    {
        return root.Weapons;
    }

    public WeaponEntry? GetById(string id)
    {
        return Get(e => e.Id == id);
    }
}
