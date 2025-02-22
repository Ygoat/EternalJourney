namespace EternalJourney.Cores.Settings;

public interface IAppSetting
{
    public string JsonFilePath { get; set; }
    public string CsvFileBasePath { get; set; }
}
