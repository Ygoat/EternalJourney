namespace EternalJourney.BulletFactory;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.App.State;
using EternalJourney.Bullet;
using EternalJourney.Cores.Consts;
using EternalJourney.Cores.Utils;
using Godot;


public interface IBulletFactory : INode;

[Meta(typeof(IAutoNode))]
public partial class BulletFactory : Node, IBulletFactory
{
    public override void _Notification(int what) => this.Notify(what);

    #region State
    public BulletFactoryLogic BulletFactoryLogic { get; set; } = default!;
    public BulletFactoryLogic.IBinding BulletFactoryBinding { get; set; } = default!;
    #endregion State

    #region Exports
    public int WaitTime { get; set; } = 1;
    public Bullet Bullet { get; set; } = default!;
    #endregion Exports

    #region Nodes
    [Node]
    public ITimer Timer { get; set; } = default!;
    #endregion Nodes

    [Dependency]
    public IInstantiator Instantiator => this.DependOn<IInstantiator>();


    public void Setup()
    {
        BulletFactoryLogic = new BulletFactoryLogic();
        BulletFactoryBinding = BulletFactoryLogic.Bind();
    }

    public void OnReady()
    {
        BulletFactoryBinding
            .Handle((in BulletFactoryLogic.Output.ReadyComplete _) =>
            {
                SetPhysicsProcess(true);
            })
            .Handle((in BulletFactoryLogic.Output.StartCoolDown _) =>
            {
                SetPhysicsProcess(false);
                SetTimer();
            });
        Timer.OneShot = true;
        Timer.SetWaitTime(WaitTime);
        Timer.Timeout += OnTimeout;
        BulletFactoryLogic.Start();
    }

    public void OnResolved()
    {
        // Instantiator.ToString();
        Bullet = Instantiator.LoadAndInstantiate<Bullet>(Const.BulletNodePath);
        Bullet.Show();
    }

    public void OnTimeout()
    {
        BulletFactoryLogic.Input(new BulletFactoryLogic.Input.CoolDownComplete());
    }

    public void OnPhysicsProcess(double delta)
    {
        GD.Print("Shot");
        Shoot();
    }

    public void SetTimer()
    {
        GD.Print("SetTimer");
        Timer.Start();
    }

    public void Shoot()
    {
        BulletFactoryLogic.Input(new BulletFactoryLogic.Input.Fire());
    }

}
