namespace EternalJourney.Cores.Repositories.Interfaces;

using System;
using System.Collections.Generic;
using EternalJourney.Cores.Models;

public interface IClueRepository
{
    public List<Clue> GetClueMany();
    public List<Clue> GetClueMany(Func<Clue, bool> predicate);
    public Clue? GetClue(Func<Clue, bool> predicate);

}
