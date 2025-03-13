namespace EternalJourney.Enemy;

using Chickensoft.AutoInject;
using Chickensoft.Collections;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Cores.Consts;
using EternalJourney.EnemyFactory;
using EternalJourney.EnemySpawner;
using EternalJourney.Ship;
using Godot;

/// <summary>
/// エネミーインターフェース
/// </summary>
public interface IEnemy : INode2D { }

/// <summary>
/// エネミークラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class Enemy : Node2D, IEnemy
{
    public override void _Notification(int what) => this.Notify(what);

    #region State
    /// <summary>
    /// エネミーロジック
    /// </summary>
    public EnemyLogic EnemyLogic { get; set; } = default!;

    /// <summary>
    /// エネミーロジックバインド
    /// </summary>
    public EnemyLogic.IBinding EnemyBinding { get; set; } = default!;
    #endregion State

    #region Exports
    /// <summary>
    /// 移動速度
    /// </summary>
    public int Speed { get; set; } = 3;

    /// <summary>
    /// 標的対象位置
    /// </summary>
    public Vector2 TargetPosition { get; set; } = default!;

    /// <summary>
    /// 進行方向
    /// </summary>
    public Vector2 Direction { get; set; } = default!;
    #endregion Exports

    #region Nodes
    [Node]
    public IArea2D Area2D { get; set; } = default!;

    [Node]
    public IVisibleOnScreenNotifier2D VisibleOnScreenNotifier2D { get; set; } = default!;
    #endregion Nodes

    #region Dependency
    [Dependency]
    public IEnemyFactory EnemyFactory => this.DependOn<IEnemyFactory>();

    [Dependency]
    public IEnemySpawner EnemySpawner => this.DependOn<IEnemySpawner>();

    [Dependency]
    public EntityTable<int> EntityTable => this.DependOn<EntityTable<int>>();
    #endregion Dependency

    public void OnReady()
    {
    }

    public void Setup()
    {
        EnemyLogic = new EnemyLogic();
        EnemyBinding = EnemyLogic.Bind();
        Area2D.CollisionLayer = CollisionEntity.Enemy;
        Area2D.CollisionMask = CollisionEntity.Ship | CollisionEntity.Bullet;
        TargetPosition = EntityTable.Get<IShip>(0)!.EnemyTargetMarker.GlobalPosition;
    }

    public void OnResolved()
    {
        EnemyBinding
            .Handle((in EnemyLogic.Output.StartClose _) =>
            {
                SetPhysicsProcess(true);
            })
            .Handle((in EnemyLogic.Output.Damaged _) =>
            {
                CallDeferred("RemoveSelf");
                SetPhysicsProcess(false);
                EnemyFactory.EnemiesQueue.Enqueue(this);
                EnemyLogic.Input(new EnemyLogic.Input.Removed());
            })
            .Handle((in EnemyLogic.Output.SpawnEnable _) =>
            {
            });
        Area2D.AreaEntered += OnAreaEntered;
        VisibleOnScreenNotifier2D.ScreenExited += OnScreenExited;
        EnemyLogic.Start();
    }

    public void OnPhysicsProcess(double delta)
    {
        Direction = TargetPosition - GlobalPosition;
        GlobalPosition += Speed * Direction.Normalized();
    }

    public void OnAreaEntered(Area2D area)
    {
        EnemyLogic.Input(new EnemyLogic.Input.TakeDamage());
    }

    /// <summary>
    /// 敵スポーン
    /// </summary>
    public void EnemySpawn()
    {
        EnemyLogic.Input(new EnemyLogic.Input.TargetDiscover());
        GlobalPosition = EnemySpawner.PathFollow2D.GlobalPosition;
    }

    /// <summary>
    /// エリア外に出た時消す
    /// TODO:それ用のstateを作成したい
    /// </summary>
    public void OnScreenExited()
    {
        EnemyLogic.Input(new EnemyLogic.Input.TakeDamage());
    }

    public void RemoveSelf()
    {
        GetParent().RemoveChild(this);
    }
}
