namespace EternalJourney.Enemy.Orbit;

using System;
using System.Collections.Generic;
using System.Linq;
using Chickensoft.AutoInject;
using Chickensoft.Collections;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Battle.Domain;
using EternalJourney.Bullet.Abstract.Base;
using EternalJourney.Cores.Consts;
using EternalJourney.Enemy.Base;
using EternalJourney.Enemy.Orbit.State;
using EternalJourney.Ship;
using EternalJourney.Weapon.Abstract;
using Godot;

/// <summary>
/// 周回エネミーインターフェース
/// </summary>
public interface IOrbitEnemy : IBaseEnemy
{
    /// <summary>
    /// スコア値
    /// </summary>
    int ScoreValue { get; }

    /// <summary>
    /// 接近速度（px/秒）
    /// </summary>
    float ApproachSpeed { get; }

    /// <summary>
    /// 周回半径（px）。この距離まで近づくと周回を始める
    /// </summary>
    float OrbitRadius { get; }

    /// <summary>
    /// 周回の角速度（度/秒）
    /// </summary>
    float OrbitAngularSpeed { get; }

    /// <summary>
    /// 時計回りに周回するか
    /// </summary>
    bool OrbitClockwise { get; }

    /// <summary>
    /// 周回する時間（秒）。経過すると逃走する
    /// </summary>
    float OrbitDuration { get; }

    /// <summary>
    /// 逃走速度（px/秒）
    /// </summary>
    float FleeSpeed { get; }
}

/// <summary>
/// 周回エネミークラス
/// 画面外からShipへ一直線に接近し、一定距離を保って周回しながら攻撃し、一定時間後に画面外へ逃げる
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class OrbitEnemy : BaseEnemy, IOrbitEnemy
{
    public override void _Notification(int what) => this.Notify(what);

    #region State
    /// <summary>
    /// 周回エネミーロジック
    /// </summary>
    public IOrbitEnemyLogic OrbitEnemyLogic { get; set; } = default!;

    /// <summary>
    /// 周回エネミーロジックバインド
    /// </summary>
    public LogicBlock.Binding OrbitEnemyBinding { get; set; } = default!;

    /// <summary>
    /// 搭載武器一覧（子ノードから動的に検出する）
    /// </summary>
    private List<IStandardWeapon> _weapons = new();

    /// <summary>
    /// 船体の回転補間の強さ
    /// </summary>
    private const float BODY_ROTATION_SPEED = 0.03f;
    #endregion State

    #region Exports
    /// <summary>
    /// スコア値
    /// </summary>
    [Export]
    public int ScoreValue { get; set; } = 30;

    /// <summary>
    /// 接近速度（px/秒）
    /// </summary>
    [Export]
    public float ApproachSpeed { get; set; } = 180f;

    /// <summary>
    /// 周回半径（px）
    /// </summary>
    [Export]
    public float OrbitRadius { get; set; } = 300f;

    /// <summary>
    /// 周回の角速度（度/秒）
    /// </summary>
    [Export]
    public float OrbitAngularSpeed { get; set; } = 40f;

    /// <summary>
    /// 時計回りに周回するか
    /// </summary>
    [Export]
    public bool OrbitClockwise { get; set; } = true;

    /// <summary>
    /// 周回する時間（秒）
    /// </summary>
    [Export]
    public float OrbitDuration { get; set; } = 10f;

    /// <summary>
    /// 逃走速度（px/秒）
    /// </summary>
    [Export]
    public float FleeSpeed { get; set; } = 260f;
    #endregion Exports

    #region Nodes
    /// <summary>
    /// 画面外検知用
    /// </summary>
    [Node]
    public IVisibleOnScreenNotifier2D VisibleOnScreenNotifier2D { get; set; } = default!;

    /// <summary>
    /// エネミー画像
    /// </summary>
    [Node]
    public ISprite2D Sprite2D { get; set; } = default!;
    #endregion Nodes

    #region Dependencies
    [Dependency]
    public EntityTable<int> EntityTable => this.DependOn<EntityTable<int>>();
    #endregion Dependencies

    public override void Setup()
    {
        base.Setup();

        OrbitEnemyLogic = new OrbitEnemyLogic();
        OrbitEnemyLogic.Set(this as IOrbitEnemy);
        OrbitEnemyBinding = OrbitEnemyLogic.Bind();

        // エネミー層に置き、Shipと自機の弾丸を検知する
        CollisionLayer = CollisionEntity.Enemy;
        CollisionMask = CollisionEntity.Ship | CollisionEntity.PlayerBullet;
        TopLevel = true;
    }

    public override void OnResolved()
    {
        base.OnResolved();

        // DI解決後に依存を設定
        OrbitEnemyLogic.Set(BattleRepo);
        OrbitEnemyLogic.Set(EntityTable.Get<IShip>(0)!);

        // 子ノードから武器を動的検出（名前・数は自由）
        _weapons = GetChildren().OfType<IStandardWeapon>().ToList();
        foreach (IStandardWeapon weapon in _weapons)
        {
            // 武器のターゲットをShipに設定
            weapon.SetTargetMask(CollisionEntity.Ship);
            weapon.SetPlayerOwned(false);
        }

        OrbitEnemyBinding
            .OnOutput((in OrbitEnemyLogic.Output.StartInvade output) =>
            {
                GlobalPosition = output.SpawnGlobalPosition;
                Rotation = output.SpawnGlobalAngle;
                SetPhysicsProcess(true);
            })
            .OnOutput((in OrbitEnemyLogic.Output.CurrentDurChange output) =>
            {
                Status.CurrentDur = output.CurrentDur;
            })
            .OnOutput((in OrbitEnemyLogic.Output.Move output) =>
            {
                GlobalPosition += output.PositionDelta;
            })
            .OnOutput((in OrbitEnemyLogic.Output.UpdateColor output) =>
            {
                Sprite2D.Modulate = output.Color;
            })
            .OnOutput((in OrbitEnemyLogic.Output.WeaponEnabled output) =>
            {
                foreach (IStandardWeapon weapon in _weapons)
                {
                    weapon.SetActive(output.Enabled);
                }
            })
            .OnOutput((in OrbitEnemyLogic.Output.Destroyed _) =>
            {
                CallDeferred(nameof(RemoveSelf));
            })
            .OnOutput((in OrbitEnemyLogic.Output.Escaped _) =>
            {
                CallDeferred(nameof(RemoveSelf));
            });

        AreaEntered += OnAreaEntered;
        VisibleOnScreenNotifier2D.ScreenExited += OnScreenExited;

        OrbitEnemyLogic.Start<OrbitEnemyState.SpawnWait>();
    }

    public override void OnPhysicsProcess(double delta)
    {
        base.OnPhysicsProcess(delta);

        // 船体をShipの方へ向ける
        IShip? ship = EntityTable.Get<IShip>(0);
        if (ship != null)
        {
            Vector2 dir = GlobalPosition.DirectionTo(ship.EnemyTargetMarker.GlobalPosition);
            GlobalRotation = Mathf.LerpAngle(GlobalRotation, dir.Angle(), BODY_ROTATION_SPEED);
        }

        OrbitEnemyLogic.Input(new OrbitEnemyLogic.Input.PhysicsProcess(delta));
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override void Spawn(Vector2 spawnGlobalPosition, float spawnGlobalAngle)
    {
        OrbitEnemyLogic.Input(new OrbitEnemyLogic.Input.Spawn(spawnGlobalPosition, spawnGlobalAngle));
    }

    /// <summary>
    /// スタン開始：移動を停止する
    /// </summary>
    protected override void OnStunStart() => SetPhysicsProcess(false);

    /// <summary>
    /// スタン終了：移動を再開する
    /// </summary>
    protected override void OnStunEnd() => SetPhysicsProcess(true);

    private void OnAreaEntered(Area2D area)
    {
        if (area is IBaseBullet baseBullet)
        {
            OrbitEnemyLogic.Input(new OrbitEnemyLogic.Input.BulletHit(baseBullet));
        }
    }

    private void OnScreenExited()
    {
        OrbitEnemyLogic.Input(new OrbitEnemyLogic.Input.OutOfArea());
    }

    /// <summary>
    /// 自インスタンスをツリーから一時的に取り除く（プールへ返却される）
    /// </summary>
    public override void RemoveSelf()
    {
        GetParent().RemoveChild(this);
        InitializeEnemy();
        SetPhysicsProcess(false);
        EmitSignal(BaseEnemy.SignalName.Removed, this);
    }

    /// <summary>
    /// エネミー初期化
    /// </summary>
    private void InitializeEnemy()
    {
        GlobalPosition = Vector2.Zero;
        Status.CurrentDur = Status.MaxDur;
        StatusEffectReceiverManager.RemoveAll();
    }
}
