namespace EternalJourney.BulletFactory;

using System.Collections.Generic;
using System.Linq;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.App.State;
using EternalJourney.Bullet;
using EternalJourney.Cores.Consts;
using EternalJourney.Cores.Utils;
using Godot;


public interface IBulletFactory : INode, IProvide<IBulletFactory>
{
    public Queue<Bullet> BulletsQueue { get; set; }
};

[Meta(typeof(IAutoNode))]
public partial class BulletFactory : Node, IBulletFactory
{
    public override void _Notification(int what) => this.Notify(what);

    #region State
    public BulletFactoryLogic BulletFactoryLogic { get; set; } = default!;
    public BulletFactoryLogic.IBinding BulletFactoryBinding { get; set; } = default!;
    #endregion State

    #region Exports
    public double WaitTime { get; set; } = 0.1;
    public Bullet Bullet { get; set; } = default!;
    public Bullet[] Bullets { get; set; } = new Bullet[100];
    public Queue<Bullet> BulletsQueue { get; set; } = new Queue<Bullet>();

    #endregion Exports

    #region Nodes
    [Node]
    public ITimer Timer { get; set; } = default!;
    #endregion Nodes

    IBulletFactory IProvide<IBulletFactory>.Value() => this;

    [Dependency]
    public IInstantiator Instantiator => this.DependOn<IInstantiator>(() => new Instantiator(GetTree()));

    public void Initialize()
    {
        this.Provide();
    }

    public void Setup()
    {
        BulletFactoryLogic = new BulletFactoryLogic();
        BulletFactoryBinding = BulletFactoryLogic.Bind();

        // AddChild(Bullet);
        // Bullet.Show();
        Bullets = Bullets.Select(e =>
        {
            e = Instantiator.LoadAndInstantiate<Bullet>(Const.BulletNodePath);

            BulletsQueue.Enqueue(e);
            return e;
        }).ToArray();
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
        Bullet bullet = BulletsQueue.Dequeue();
        GD.Print(BulletsQueue.Count());
        GD.Print("AddChild");
        AddChild(bullet);
        bullet.Emit();
    }

}
