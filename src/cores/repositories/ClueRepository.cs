namespace EternalJourney.Cores.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using EternalJourney.Cores.Helpers;
using EternalJourney.Cores.Models;
using EternalJourney.Cores.Repositories.Interfaces;

public class ClueCsvRepository : IClueRepository
{
    public List<Clue> GetClueMany()
    {
        return GDCsvHelper.CsvMap<Clue>("res://data/Clues.csv");
    }

    public List<Clue> GetClueMany(Func<Clue, bool> predicate)
    {
        List<Clue> clues = GetClueMany();
        return clues.Where(predicate).ToList();
    }

    public Clue? GetClue(Func<Clue, bool> predicate)
    {
        List<Clue> clues = GetClueMany();
        return clues.SingleOrDefault(predicate);
    }
}
