namespace EternalJourney.Skills;

using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using EternalJourney.Battle.Domain;
using EternalJourney.Cores.Models.Skill;
using EternalJourney.RegenSkill.State;
using Godot;

public interface IRegenSkill : ISkillNode { }

[Meta(typeof(IAutoNode))]
public partial class RegenSkill : Node, IRegenSkill
{
    public override void _Notification(int what) => this.Notify(what);

    public string Description => SkillInfo.GetDescription(SkillType.Regen);

    public RegenSkillLogic Logic { get; set; } = default!;
    public RegenSkillLogic.IBinding Binding { get; set; } = default!;
    public Timer HealTimer { get; set; } = default!;
    public Timer DurationTimer { get; set; } = default!;
    public float DurationSec { get; set; } = 10.0f;

    [Dependency] public IBattleRepo BattleRepo => this.DependOn<IBattleRepo>();

    public void Initialize()
    {
        HealTimer = new Timer();
        DurationTimer = new Timer();
    }

    public void OnReady()
    {
        AddChild(HealTimer);
        AddChild(DurationTimer);
    }

    public void Setup()
    {
        Logic = new RegenSkillLogic();
        Binding = Logic.Bind();
    }

    public void OnResolved()
    {
        HealTimer.WaitTime = 1.0;
        DurationTimer.WaitTime = DurationSec;
        HealTimer.Timeout += () => Logic.Input(new RegenSkillLogic.Input.Tick());
        DurationTimer.Timeout += () => Logic.Input(new RegenSkillLogic.Input.Remove());
        Binding
            .Handle((in RegenSkillLogic.Output.Activated _) =>
            {
                HealTimer.Stop();
                HealTimer.Start();
                DurationTimer.Stop();
                DurationTimer.Start();
            })
            .Handle((in RegenSkillLogic.Output.TikHeal o) =>
            {
                BattleRepo.RequestShipHeal(o.Amount);
            })
            .Handle((in RegenSkillLogic.Output.Deactivated _) =>
            {
                HealTimer.Stop();
                DurationTimer.Stop();
            });
        Logic.Start();
    }

    public void Activate() => Logic?.Input(new RegenSkillLogic.Input.Apply());

    public void OnTreeExiting()
    {
        Binding.Dispose();
        ((System.IDisposable)Logic).Dispose();
    }
}
