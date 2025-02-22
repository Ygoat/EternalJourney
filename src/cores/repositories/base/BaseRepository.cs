namespace EternalJourney.Cores.Repositories.Base;

using System;
using System.Collections.Generic;
using System.Linq;
using EternalJourney.Cores.Helpers;
using EternalJourney.Cores.Repositories.Base.Interfaces;
using EternalJourney.Cores.Settings;
using Godot;
using Godot.DependencyInjection.Attributes;


public abstract class BaseRepository<T> : IRepository<T>
{
    [Inject]
    private readonly IAppSetting appSetting = default!;

    public List<T> GetMany()
    {
        GD.Print($"res://data/masters/{typeof(T).Name}.csv");
        return GDCsvHelper.CsvMap<T>($"res://data/masters/{typeof(T).Name}.csv");
    }

    public List<T> GetMany(Func<T, bool> predicate)
    {
        List<T> entity = GetMany();
        return entity.Where(predicate).ToList();
    }

    public T? Get(Func<T, bool> predicate)
    {
        List<T> entity = GetMany();
        return entity.SingleOrDefault(predicate);
    }

    public void Update(T entity) { }
}
