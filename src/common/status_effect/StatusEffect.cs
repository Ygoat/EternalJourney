namespace EternalJourney.Common.StatusEffect;

using Chickensoft.GodotNodeInterfaces;
using Godot;

public interface IStatusEffect : INode
{
    public float RemoveTime { get; protected set; }
    public abstract void Apply();
    public abstract void Remove();
}

public abstract partial class StatusEffect : Node, IStatusEffect
{
    public float RemoveTime { get; set; }
    public abstract void Apply();
    public abstract void Remove();
}
