namespace EternalJourney.Skills;

using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using EternalJourney.Battle.Domain;
using EternalJourney.Cores.Models.Skill;
using EternalJourney.StatusUpSkill.State;
using Godot;

public interface IShipAtkUpSkill : ISkillNode { }

[Meta(typeof(IAutoNode))]
public partial class ShipAtkUpSkill : Node, IShipAtkUpSkill
{
    public override void _Notification(int what) => this.Notify(what);

    public string Description => SkillInfo.GetDescription(SkillType.ShipAtkUp);

    public StatusUpSkillLogic Logic { get; set; } = default!;
    public StatusUpSkillLogic.IBinding Binding { get; set; } = default!;
    public Timer BuffTimer { get; set; } = default!;

    [Dependency] public IBattleRepo BattleRepo => this.DependOn<IBattleRepo>();

    public void Initialize() { BuffTimer = new Timer(); }
    public void OnReady() { AddChild(BuffTimer); }

    public void Setup()
    {
        Logic = new StatusUpSkillLogic();
        Binding = Logic.Bind();
    }

    public void OnResolved()
    {
        BuffTimer.WaitTime = 10.0f;
        BuffTimer.OneShot = true;
        BuffTimer.Timeout += () => Logic.Input(new StatusUpSkillLogic.Input.Remove());
        Logic.Set(new StatusUpSkillLogic.StackState());
        Binding
            .Handle((in StatusUpSkillLogic.Output.Activated o) =>
            {
                BattleRepo.ShipAtkBonus = o.StackCount * 0.3f;
                BuffTimer.Stop();
                BuffTimer.Start();
            })
            .Handle((in StatusUpSkillLogic.Output.Deactivated _) =>
            {
                BattleRepo.ShipAtkBonus = 0.0f;
            });
        Logic.Start();
    }

    public void Activate() => Logic?.Input(new StatusUpSkillLogic.Input.Apply());

    public void OnTreeExiting()
    {
        Binding.Dispose();
        ((System.IDisposable)Logic).Dispose();
    }
}
