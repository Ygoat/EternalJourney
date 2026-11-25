namespace EternalJourney.Weapon.Abstract;

using System;
using System.Linq;
using Chickensoft.AutoInject;
using Chickensoft.LogicBlocks;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Battle.Domain;
using EternalJourney.Bullet.Abstract;
using EternalJourney.Common.Traits;
using EternalJourney.Radar;
using EternalJourney.Weapon.Abstract.Base;
using EternalJourney.Weapon.Abstract.State;
using Godot;

/// <summary>
/// スタンダード武器インターフェース
/// </summary>
public interface IStandardWeapon : IBaseWeapon
{
    /// <summary>
    /// レーダーの検知対象コリジョンマスクを設定する
    /// </summary>
    void SetTargetMask(uint mask);

    /// <summary>
    /// プレイヤー所有フラグを設定する
    /// </summary>
    void SetPlayerOwned(bool isPlayer);

    /// <summary>
    /// 攻撃の有効・無効を切り替える（無効の間は索敵せず、射撃しない）
    /// </summary>
    void SetActive(bool active);
}

/// <summary>
/// スタンダード武器クラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class StandardWeapon : BaseWeapon, IStandardWeapon
{
    public override void _Notification(int what) => this.Notify(what);

    #region State
    /// <summary>
    /// 武器ロジック
    /// </summary>
    public IStandardWeaponLogic StandardWeaponLogic { get; set; } = default!;

    /// <summary>
    /// 武器ロジックバインド
    /// </summary>
    public LogicBlock.Binding WeaponBind { get; set; } = default!;
    #endregion State

    #region Exports
    /// <summary>
    /// ターゲット方向
    /// </summary>
    public Vector2 TargetDirection { get; set; } = default!;

    /// <summary>
    /// 武器方向
    /// </summary>
    public Vector2 WeaponDirection { get; set; } = default!;

    /// <summary>
    /// 回転速度
    /// </summary>
    [Export]
    public float RotationSpeed { get; set; } = 0.05f;

    /// <summary>
    /// 弾丸の拡大率（見た目・当たり判定の両方に反映される）
    /// </summary>
    [Export]
    public float BulletScale { get; set; } = 1.0f;
    #endregion Exports

    #region Nodes
    /// <summary>
    /// 弾丸ファクトリ
    /// </summary>
    [Node]
    public IStandardBulletFactory StandardBulletFactory { get; set; } = default!;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [Node]
    public IMarker2D Marker2D { get; set; } = default!;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [Node]
    public IMarker2D CenterMarker { get; set; } = default!;

    /// <summary>
    /// レーダー
    /// </summary>
    [Node]
    public IRadar Radar { get; set; } = default!;
    #endregion Nodes

    #region Dependencies
    [Dependency]
    public IBattleRepo BattleRepo => this.DependOn<IBattleRepo>();
    #endregion Dependencies

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Initialize()
    {
        Status = new Status { Spd = 0.1f, MaxDur = 10.0f, CurrentDur = 10.0f };
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override void OnReady()
    {
        base.OnReady();
        // Status.Spd 未設定（0）のシーンはファクトリ既定の間隔を基準にする（0 だと Timer に設定できず倍率も効かない）
        _baseFireInterval = Status.Spd > 0f ? Status.Spd : StandardBulletFactory.WaitTime;
        StandardBulletFactory.WaitTime = _baseFireInterval;
    }

    /// <summary>
    /// 基準の発射間隔（秒）
    /// </summary>
    private float _baseFireInterval;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override void Setup()
    {
        base.Setup();
        // 武器ロジックインスタンス化
        StandardWeaponLogic = new StandardWeaponLogic();
        // 武器ロジックバインド
        WeaponBind = StandardWeaponLogic.Bind();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override void OnResolved()
    {
        base.OnResolved();
        // 弾丸サイズを弾丸ファクトリへ橋渡し
        StandardBulletFactory.BulletScale = BulletScale;
        WeaponBind
            // Idling出力時
            .OnOutput((in StandardWeaponLogic.Output.Idling _) =>
            {
                // 物理処理無効化
                SetPhysicsProcess(false);
            })
            // Attacking出力時
            .OnOutput((in StandardWeaponLogic.Output.Attacking _) =>
            {
                // 物理処理有効化
                SetPhysicsProcess(true);
            });
        // 敵発見イベント
        Radar.Searched += OnSearched;
        // 敵未発見イベント
        Radar.NotSearched += OnNotSearched;
        // 武器ロジック初期状態開始
        StandardWeaponLogic.Start<StandardWeaponState.Idle>();

        BattleRepo.WeaponSpdMultiplierChanged += UpdateWaitTime;
        BattleRepo.ResearchChanged += UpdateWaitTime;
        BattleRepo.EnemyDebuffChanged += UpdateWaitTime;
        _resolved = true;
        UpdateWaitTime();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="delta"></param>
    public override void OnPhysicsProcess(double delta)
    {
        base.OnPhysicsProcess(delta);

        // センターマーカーと発射口マーカーから武器の方向を計算
        WeaponDirection = CenterMarker.GlobalPosition.DirectionTo(Marker2D.GlobalPosition);
        // レーダーにて索敵された敵のうち一番近い敵のArea2Dを取得
        Area2D? enemy = Radar.OnAreaEnemies.FirstOrDefault();
        if (enemy != null)
        {
            // 敵の方向を取得
            TargetDirection = CenterMarker.GlobalPosition.DirectionTo(enemy.GlobalPosition);
            // LerpAngleで最短経路を補間（微振動なし）
            GlobalRotation = Mathf.LerpAngle(GlobalRotation, TargetDirection.Angle(), RotationSpeed);
        }
        // 射撃を行う
        Attack();
    }

    /// <summary>
    /// レーダーの検知対象コリジョンマスクを設定する
    /// </summary>
    public void SetTargetMask(uint mask)
    {
        Radar.TargetMask = mask;
        Radar.CollisionMask = mask;
        StandardBulletFactory.BulletCollisionMask = mask;
    }

    /// <summary>
    /// 攻撃の有効・無効を切り替える
    /// </summary>
    public void SetActive(bool active)
    {
        Radar.SetActive(active);
    }

    public void SetPlayerOwned(bool isPlayer)
    {
        IsPlayerOwned = isPlayer;
        StandardBulletFactory.IsPlayerBullet = isPlayer;
        // 所有者が DI 解決後に変わった場合も、所有者に応じた発射間隔へ更新する
        if (_resolved)
        {
            UpdateWaitTime();
        }
    }

    /// <summary>
    /// DI 解決済みか（BattleRepo を参照できるか）
    /// </summary>
    private bool _resolved;

    private void UpdateWaitTime()
    {
        StandardBulletFactory.WaitTime = IsPlayerOwned
            ? _baseFireInterval / BattleRepo.WeaponSpdMultiplier * BattleRepo.Research.Value.FireIntervalMultiplier
            : _baseFireInterval * BattleRepo.EnemyFireIntervalMultiplier;
    }

    // 注意: プール運用される敵の子として、返却のたびにツリーから外れるため、OnExitTree では Dispose / 購読解除をしない。

    public override void Attack()
    {
        StandardBulletFactory.GenerateBullet();
    }

    /// <summary>
    /// 敵発見イベントファンクション
    /// </summary>
    public void OnSearched()
    {
        // StartAttackを入力
        StandardWeaponLogic.Input(new StandardWeaponLogic.Input.StartAttack());
    }

    /// <summary>
    /// 敵未発見イベントファンクション
    /// </summary>
    public void OnNotSearched()
    {
        // StartIdleを入力
        StandardWeaponLogic.Input(new StandardWeaponLogic.Input.StartIdle());
    }

    /// <summary>
    /// シグモイド関数
    /// x->0の時、f(x)=0
    /// x->正の無限大の時、f(x)->1
    /// x->負の無限大の時、f(x)->-1
    /// </summary>
    /// <param name="k"></param>
    /// <param name="x"></param>
    /// <returns></returns>
    public float Sigmoid(double k, double x)
    {
        return (float)(2 * ((1 / (1 + Math.Pow(2.7, -k * x))) - (1 / 2)));
    }
}
