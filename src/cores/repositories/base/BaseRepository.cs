namespace EternalJourney.Cores.Repositories.Base;

using System;
using System.Collections.Generic;
using System.Linq;
using EternalJourney.Cores.Consts;
using EternalJourney.Cores.Helpers;
using EternalJourney.Cores.Repositories.Base.Interfaces;
using EternalJourney.Cores.Settings.Loader;
using Godot;

/// <summary>
/// ベースレポジトリ
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class BaseRepository<T> : IRepository<T>
{
    /// <summary>
    /// 全件取得
    /// </summary>
    /// <returns></returns>
    public List<T> GetMany()
    {
        GD.Print($"{AppSetting.Instance.CsvFileBasePath}{typeof(T).Name}{Const.CSV_EXTENSION}");
        return GDCsvHelper.CsvMap<T>($"{AppSetting.Instance.CsvFileBasePath}{typeof(T).Name}{Const.CSV_EXTENSION}");
    }

    /// <summary>
    /// 複数取得
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public List<T> GetMany(Func<T, bool> predicate)
    {
        List<T> entity = GetMany();
        return entity.Where(predicate).ToList();
    }

    /// <summary>
    /// 単数取得
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public T? Get(Func<T, bool> predicate)
    {
        List<T> entity = GetMany();
        return entity.SingleOrDefault(predicate);
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="entity"></param>
    public void Update(T entity) { }
}
