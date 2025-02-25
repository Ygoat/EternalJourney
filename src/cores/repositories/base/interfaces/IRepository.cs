namespace EternalJourney.Cores.Repositories.Base.Interfaces;

using System;
using System.Collections.Generic;

public interface IRepository<T>
{
    public List<T> GetMany();
    public List<T> GetMany(Func<T, bool> predicate);
    public T? Get(Func<T, bool> predicate);

    public void Update(T entity);
}
