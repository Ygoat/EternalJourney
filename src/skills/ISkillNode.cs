namespace EternalJourney.Skills;

using Chickensoft.GodotNodeInterfaces;

public interface ISkillNode : INode
{
    void Activate();
    string Description { get; }
}
