namespace EternalJourney.EnemyFactory;

using System.Collections.Generic;
using System.Linq;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.App.State;
using EternalJourney.Cores.Consts;
using EternalJourney.Cores.Utils;
using EternalJourney.Enemy;
using EternalJourney.EnemyFactory.State;
using Godot;

public interface IEnemyFactory : INode, IProvide<IEnemyFactory>
{
    public Queue<Enemy> EnemiesQueue { get; set; }
};

[Meta(typeof(IAutoNode))]
public partial class EnemyFactory : Node, IEnemyFactory
{
    public override void _Notification(int what) => this.Notify(what);

    #region State
    /// <summary>
    /// エネミーファクトリロジック
    /// </summary>
    public EnemyFactoryLogic EnemyFactoryLogic { get; set; } = default!;

    /// <summary>
    /// エネミーファクトリバインド
    /// </summary>
    public EnemyFactoryLogic.IBinding EnemyFactoryBinding { get; set; } = default!;
    #endregion State

    #region Exports
    /// <summary>
    /// 待機時間
    /// </summary>
    public double WaitTime { get; set; } = 0.5;

    /// <summary>
    ///　弾丸配列
    /// </summary>
    public Enemy[] Enemies { get; set; } = new Enemy[100];

    /// <summary>
    /// 弾丸キュー
    /// </summary>
    public Queue<Enemy> EnemiesQueue { get; set; } = new Queue<Enemy>();
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
    IEnemyFactory IProvide<IEnemyFactory>.Value() => this;
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
        EnemyFactoryLogic = new EnemyFactoryLogic();
        EnemyFactoryBinding = EnemyFactoryLogic.Bind();

        Enemies = Enemies.Select(e =>
        {
            e = Instantiator.LoadAndInstantiate<Enemy>(Const.EnemyNodePath);

            EnemiesQueue.Enqueue(e);
            return e;
        }).ToArray();
    }

    public void OnReady()
    {
        EnemyFactoryBinding
            .Handle((in EnemyFactoryLogic.Output.ReadyComplete _) =>
            {
                SetPhysicsProcess(true);
            })
            .Handle((in EnemyFactoryLogic.Output.StartCoolDown _) =>
            {
                SetPhysicsProcess(false);
                SetTimer();
            });
        Timer.OneShot = true;
        Timer.SetWaitTime(WaitTime);
        Timer.Timeout += OnTimeout;
        EnemyFactoryLogic.Start();
    }

    public void OnPhysicsProcess(double delta)
    {
        GD.Print("Shot");
        Spawn();
    }

    /// <summary>
    /// タイムアウトイベント
    /// </summary>
    public void OnTimeout()
    {
        EnemyFactoryLogic.Input(new EnemyFactoryLogic.Input.CoolDownComplete());
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
    public void Spawn()
    {
        EnemyFactoryLogic.Input(new EnemyFactoryLogic.Input.Spawn());
        Enemy enemy = EnemiesQueue.Dequeue();
        GD.Print(EnemiesQueue.Count());
        GD.Print("AddChild");
        AddChild(enemy);
        enemy.EnemySpawn();
    }
}
