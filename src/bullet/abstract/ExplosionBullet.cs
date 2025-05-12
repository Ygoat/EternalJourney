namespace EternalJourney.Bullet.Abstract;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Bullet.Abstract.Base;
using Godot;


public interface IExplosionBullet : IBaseBullet
{
}

[Meta(typeof(IAutoNode))]
public partial class ExplosionBullet : BaseBullet, IExplosionBullet
{
    public override void _Notification(int what) => this.Notify(what);

    [Node]
    public ITimer BlastTimer { get; set; } = default!;

    [Node]
    public ICollisionShape2D BulletCollisionShape2D { get; set; } = default!;

    [Node]
    public IColorRect BulletColorRect { get; set; } = default!;

    [Node]
    public IColorRect BlastColorRect { get; set; } = default!;

    [Node]
    public ICollisionShape2D BlastCollisionShape2D { get; set; } = default!;

    public override void Setup()
    {
        base.Setup();
    }

    public override void OnResolved()
    {
        base.OnResolved();

        BlastTimer.Timeout += OnBlastTimerTimeout;
        BlastTimer.SetWaitTime(5);
        BlastTimer.OneShot = true;
        BlastTimer.Start();

        SetPhysicsProcess(true);
    }

    public void OnPhysicsProcess(double delta)
    {
        GlobalPosition += new Vector2(1, 0);
    }

    private void OnBlastTimerTimeout()
    {
        BlastColorRect.Show();
        SetPhysicsProcess(false);
    }
}
