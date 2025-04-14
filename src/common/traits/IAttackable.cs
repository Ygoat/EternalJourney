namespace EternalJourney.Common.Traits;

public interface IAttackable
{
    public double Atk { get; set; }
    public bool CanAttack { get; set; }
    public void Attack();
}
