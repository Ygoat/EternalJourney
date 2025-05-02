namespace EternalJourney.Common.StatusEffect;

public interface IStatusEffectTarget
{
    void AddStatusEffect(StatusEffect effect);
    void RemoveStatusEffect(StatusEffect effect);
}

