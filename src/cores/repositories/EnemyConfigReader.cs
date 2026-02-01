namespace EternalJourney.Cores.Repositories;

using System.Collections.Generic;
using EternalJourney.Cores.Models.Enemy;
using EternalJourney.Cores.Repositories.Base;
using EternalJourney.Cores.Settings;

/// <summary>
/// エネミー設定リーダーインターフェース
/// </summary>
public interface IEnemyConfigReader : IBaseJsonReader<EnemyConfig>
{
    EnemyConfig? GetById(string id);
}

/// <summary>
/// エネミー設定JSONリーダー
/// </summary>
public class EnemyConfigReader : BaseJsonReader<EnemyConfigRoot, EnemyConfig>, IEnemyConfigReader
{
    /// <summary>
    /// ファイルパスを取得
    /// </summary>
    protected override string GetFilePath()
    {
        return $"{AppSetting.CsvFileBasePath}EnemyConfig.json";
    }

    /// <summary>
    /// ルートからエネミーリストを取得
    /// </summary>
    protected override List<EnemyConfig> GetItemsFromRoot(EnemyConfigRoot root)
    {
        return root.Enemies;
    }

    /// <summary>
    /// IDで取得
    /// </summary>
    public EnemyConfig? GetById(string id)
    {
        return Get(e => e.Id == id);
    }
}
