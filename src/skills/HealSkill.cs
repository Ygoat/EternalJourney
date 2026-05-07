namespace EternalJourney.Skills;

using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using EternalJourney.Battle.Domain;
using EternalJourney.Cores.Models.Skill;
using EternalJourney.HealSkill.State;
using Godot;

public interface IHealSkill : ISkillNode { }

[Meta(typeof(IAutoNode))]
public partial class HealSkill : Node, IHealSkill
{
    public override void _Notification(int what) => this.Notify(what);

    public string Description => SkillInfo.GetDescription(SkillType.Heal);

    public HealSkillLogic Logic { get; set; } = default!;
    public HealSkillLogic.IBinding Binding { get; set; } = default!;

    [Dependency] public IBattleRepo BattleRepo => this.DependOn<IBattleRepo>();

    public void Setup()
    {
        Logic = new HealSkillLogic();
        Binding = Logic.Bind();
    }

    public void OnResolved()
    {
        Binding
            .Handle((in HealSkillLogic.Output.Healed o) =>
            {
                BattleRepo.RequestShipHeal(o.Amount);
            });
        Logic.Start();
    }

    public void Activate() => Logic?.Input(new HealSkillLogic.Input.Apply());

    public void OnTreeExiting()
    {
        Binding.Dispose();
        ((System.IDisposable)Logic).Dispose();
    }
}
