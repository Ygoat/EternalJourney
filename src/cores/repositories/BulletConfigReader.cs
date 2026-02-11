namespace EternalJourney.Cores.Repositories;

using System.Collections.Generic;
using EternalJourney.Cores.Models.Bullet;
using EternalJourney.Cores.Repositories.Base;
using EternalJourney.Cores.Settings;

/// <summary>
/// 弾丸設定リーダーインターフェース
/// </summary>
public interface IBulletConfigReader : IBaseJsonReader<BulletConfig>
{
    BulletConfig? GetById(string id);
}

/// <summary>
/// 弾丸設定JSONリーダー
/// </summary>
public class BulletConfigReader : BaseJsonReader<BulletConfigRoot, BulletConfig>, IBulletConfigReader
{
    /// <summary>
    /// ファイルパスを取得
    /// </summary>
    protected override string GetFilePath()
    {
        return $"{AppSetting.CsvFileBasePath}BulletConfig.json";
    }

    /// <summary>
    /// ルートから弾丸リストを取得
    /// </summary>
    protected override List<BulletConfig> GetItemsFromRoot(BulletConfigRoot root)
    {
        return root.Bullets;
    }

    /// <summary>
    /// IDで取得
    /// </summary>
    public BulletConfig? GetById(string id)
    {
        return Get(e => e.Id == id);
    }
}
