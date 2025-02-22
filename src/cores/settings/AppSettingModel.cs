namespace EternalJourney.Cores.Settings;

/// <summary>
/// appsettings.jsonモデル
/// </summary>
public class AppSettingModel
{
    /// <summary>
    /// CSVファイルパス
    /// </summary>
    public string CsvFileBasePath { get; set; } = string.Empty;
    public Db Db { get; set; } = new Db();
}


public class Db
{
    public bool Test { get; set; }
    public bool Test2 { get; set; }

    public Connect Connect { get; set; } = new Connect();
}

public class Connect
{
    public int ConSt { get; set; }
}


