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

/// <summary>
/// 弾丸ファクトリインターフェース
/// </summary>
public interface IBulletFactory : INode, IProvide<IBulletFactory>
{
    public Queue<Bullet> BulletsQueue { get; set; }
};

/// <summary>
/// 弾丸ファクトリ
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class BulletFactory : Node, IBulletFactory
{
    public override void _Notification(int what) => this.Notify(what);

    #region State
    /// <summary>
    /// 弾丸ファクトリロジック
    /// </summary>
    public BulletFactoryLogic BulletFactoryLogic { get; set; } = default!;

    /// <summary>
    /// 弾丸ファクトリバインド
    /// </summary>
    public BulletFactoryLogic.IBinding BulletFactoryBinding { get; set; } = default!;
    #endregion State

    #region Exports
    /// <summary>
    /// 待機時間
    /// </summary>
    public double WaitTime { get; set; } = 0.1;

    /// <summary>
    ///　弾丸配列
    /// </summary>
    public Bullet[] Bullets { get; set; } = new Bullet[100];

    /// <summary>
    /// 弾丸キュー
    /// </summary>
    public Queue<Bullet> BulletsQueue { get; set; } = new Queue<Bullet>();
    #endregion Exports

    #region Nodes
    /// <summary>
    /// タイマーノード
    /// </summary>
    [Node]
    public ITimer Timer { get; set; } = default!;
    #endregion Nodes

    #region Provisions
    /// <summary>
    /// 弾丸ファクトリプロバイダー
    /// </summary>
    /// <returns></returns>
    IBulletFactory IProvide<IBulletFactory>.Value() => this;
    #endregion Provisions

    #region Dependencies
    /// <summary>
    /// インスタンス化部品
    /// </summary>
    [Dependency]
    public IInstantiator Instantiator => this.DependOn<IInstantiator>(() => new Instantiator(GetTree()));
    #endregion Dependencies

    public void Initialize()
    {
        this.Provide();
    }

    public void Setup()
    {
        BulletFactoryLogic = new BulletFactoryLogic();
        BulletFactoryBinding = BulletFactoryLogic.Bind();

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

    public void OnPhysicsProcess(double delta)
    {
        // GD.Print("Shot");
        // Shoot();
    }

    /// <summary>
    /// タイムアウトイベント
    /// </summary>
    public void OnTimeout()
    {
        BulletFactoryLogic.Input(new BulletFactoryLogic.Input.CoolDownComplete());
    }

    /// <summary>
    /// タイマーセット
    /// </summary>
    public void SetTimer()
    {
        GD.Print("SetTimer");
        Timer.Start();
    }

    /// <summary>
    /// 射撃
    /// </summary>
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
