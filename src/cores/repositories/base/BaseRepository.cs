namespace EternalJourney.Cores.Repositories.Base;

using System;
using System.Collections.Generic;
using System.Linq;
using EternalJourney.Cores.Helpers;
using EternalJourney.Cores.Repositories.Base.Interfaces;
using Godot;

public abstract class BaseRepository<T> : IRepository<T>
{
    public List<T> GetMany()
    {
        GD.Print($"res://data/{typeof(T).Name}.csv");
        return GDCsvHelper.CsvMap<T>($"res://data/{typeof(T).Name}.csv");
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
