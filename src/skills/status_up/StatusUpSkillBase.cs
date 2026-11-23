namespace EternalJourney.Skills;

using Chickensoft.AutoInject;
using Chickensoft.LogicBlocks;
using Chickensoft.Introspection;
using EternalJourney.Battle.Domain;
using EternalJourney.Cores.Models.Skill;
using EternalJourney.Skills.StatusUp.State;
using Godot;

[Meta(typeof(IAutoNode))]
public abstract partial class StatusUpSkillBase : Node, ISkillNode
{
    public override void _Notification(int what) => this.Notify(what);

    protected abstract SkillType SkillKind { get; }
    public string Description => SkillInfo.GetDescription(SkillKind);

    public IStatusUpSkillLogic Logic { get; set; } = default!;
    public LogicBlock.Binding Binding { get; set; } = default!;
    public Timer BuffTimer { get; set; } = default!;

    [Dependency] public IBattleRepo BattleRepo => this.DependOn<IBattleRepo>();

    public void Initialize() { BuffTimer = new Timer(); }
    public void OnReady() { AddChild(BuffTimer); }

    public void Setup()
    {
        Logic = new StatusUpSkillLogic();
        Binding = Logic.Bind();
    }

    protected abstract void ApplyEffect(int stackCount);
    protected abstract void ResetEffect();

    public void OnResolved()
    {
        BuffTimer.WaitTime = 10.0f;
        BuffTimer.OneShot = true;
        BuffTimer.Timeout += () => Logic.Input(new StatusUpSkillLogic.Input.Remove());
        Logic.Set(new StatusUpSkillLogic.StackState());
        Binding
            .OnOutput((in StatusUpSkillLogic.Output.Activated o) =>
            {
                ApplyEffect(o.StackCount);
                BuffTimer.Stop();
                BuffTimer.Start();
            })
            .OnOutput((in StatusUpSkillLogic.Output.Deactivated _) => ResetEffect());
        Logic.Start<StatusUpSkillState.InActive>();
    }

    public void Activate() => Logic?.Input(new StatusUpSkillLogic.Input.Apply());

    public void OnTreeExiting()
    {
        Binding.Dispose();
        ((System.IDisposable)Logic).Dispose();
    }
}
