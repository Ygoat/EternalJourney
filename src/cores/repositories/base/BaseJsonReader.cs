namespace EternalJourney.Cores.Repositories.Base;

using System;
using System.Collections.Generic;
using System.Linq;
using EternalJourney.Cores.Helpers;

/// <summary>
/// JSONリーダーインターフェース
/// </summary>
/// <typeparam name="TItem">アイテム型</typeparam>
public interface IBaseJsonReader<TItem>
{
    List<TItem> GetMany();
    List<TItem> GetMany(Func<TItem, bool> predicate);
    TItem? Get(Func<TItem, bool> predicate);
    void InvalidateCache();
}

/// <summary>
/// JSONリーダー基底クラス
/// </summary>
/// <typeparam name="TRoot">JSONルート型</typeparam>
/// <typeparam name="TItem">アイテム型</typeparam>
public abstract class BaseJsonReader<TRoot, TItem> : IBaseJsonReader<TItem> where TRoot : class
{
    private TRoot? _cachedData;

    /// <summary>
    /// ファイルパスを取得
    /// </summary>
    protected abstract string GetFilePath();

    /// <summary>
    /// ルートからアイテムリストを取得
    /// </summary>
    protected abstract List<TItem> GetItemsFromRoot(TRoot root);

    /// <summary>
    /// ルートデータを読み込み
    /// </summary>
    protected TRoot LoadRoot()
    {
        if (_cachedData == null)
        {
            _cachedData = GDJsonHelper.JsonMap<TRoot>(GetFilePath());
        }
        return _cachedData;
    }

    /// <summary>
    /// 全件取得
    /// </summary>
    public List<TItem> GetMany()
    {
        return GetItemsFromRoot(LoadRoot());
    }

    /// <summary>
    /// 複数取得
    /// </summary>
    public List<TItem> GetMany(Func<TItem, bool> predicate)
    {
        return GetMany().Where(predicate).ToList();
    }

    /// <summary>
    /// 単数取得
    /// </summary>
    public TItem? Get(Func<TItem, bool> predicate)
    {
        return GetMany().SingleOrDefault(predicate);
    }

    /// <summary>
    /// キャッシュ無効化
    /// </summary>
    public void InvalidateCache()
    {
        _cachedData = null;
    }
}
