namespace EternalJourney.Enemy;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Cores.Consts;
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
    public IEnemyLogic EnemyLogic { get; set; } = default!;

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
    #endregion Exports

    #region Nodes
    [Node]
    public IArea2D Area2D { get; set; } = default!;
    #endregion Nodes

    public void Setup()
    {
        EnemyLogic = new EnemyLogic();
        EnemyBinding = EnemyLogic.Bind();
        Area2D.CollisionLayer = CollisionEntity.Enemy;
        Area2D.CollisionMask = CollisionEntity.Ship | CollisionEntity.Bullet;
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
                GetParent().RemoveChild(this);
                SetPhysicsProcess(false);
                EnemyLogic.Input(new EnemyLogic.Input.Removed());
            })
            .Handle((in EnemyLogic.Output.SpawnEnable _) =>
            {
            });
        Area2D.AreaEntered += OnAreaEntered;
        EnemyLogic.Start();
    }

    public void OnPhysicsProcess(double delta)
    {
        EnemyLogic.Input(new EnemyLogic.Input.TakeDamage());
        GlobalPosition += new Vector2(Speed, Speed);
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
        EnemyLogic.Input(new EnemyLogic.Input.Removed());
    }
}
