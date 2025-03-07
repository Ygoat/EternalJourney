namespace EternalJourney.Cores.Repositories;

using EternalJourney.Cores.Models;
using EternalJourney.Cores.Repositories.Base;

public interface ICrewCsvReader : IBaseCsvReader<Crew>;

/// <summary>
/// クルーCSVレポジトリ
/// </summary>
public class CrewCsvReader : BaseCsvReader<Crew>, ICrewCsvReader
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    public CrewCsvReader() : base() { }
}
