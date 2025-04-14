namespace EternalJourney.Common.Traits;

using EternalJourney.Common.DurabilityModule;

public interface IDestructible
{
    public double Def { get; set; }
    public double MaxDurability { get; set; }
    public IDurabilityModule DurabilityModule { get; set; }
    public void RemoveSelf();
}
