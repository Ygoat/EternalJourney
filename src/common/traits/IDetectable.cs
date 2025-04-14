namespace EternalJourney.Common.Traits;

using EternalJourney.Common.BaseEntity;

public interface IDetactable
{
    public double DetectRange { get; set; }
    public bool CanDetect { get; set; }
    public BaseEntity Detect();
}
