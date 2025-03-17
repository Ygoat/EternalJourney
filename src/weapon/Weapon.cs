namespace EternalJourney.Weapon;

using System;
using System.Linq;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.BulletFactory;
using EternalJourney.Radar;
using Godot;


/// <summary>
/// 武器インターフェース
/// </summary>
public interface IWeapon : INode2D, IProvide<IWeapon>
{
    /// <summary>
    /// 発射口マーカー
    /// </summary>
    public IMarker2D Marker2D { get; set; }

    /// <summary>
    /// 武器センターマーカー
    /// </summary>
    public IMarker2D CenterMarker { get; set; }
}

/// <summary>
/// 武器クラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class Weapon : Node2D, IWeapon
{
    public override void _Notification(int what) => this.Notify(what);

    #region State
    /// <summary>
    /// 武器ロジック
    /// </summary>
    public WeaponLogic WeaponLogic { get; set; } = default!;

    /// <summary>
    /// 武器ロジックバインド
    /// </summary>
    public WeaponLogic.IBinding WeaponBind { get; set; } = default!;
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
    public float RotationSpeed { get; set; } = 0.05f;
    #endregion Exports

    #region Nodes
    /// <summary>
    /// 弾丸ファクトリ
    /// </summary>
    [Node]
    public IBulletFactory BulletFactory { get; set; } = default!;

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

    #region Provisions
    /// <summary>
    /// 武器プロバイダー
    /// </summary>
    /// <returns></returns>
    IWeapon IProvide<IWeapon>.Value() => this;
    #endregion Provisions

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Initialize()
    {
        // 依存性提供
        this.Provide();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Setup()
    {
        // 武器ロジックインスタンス化
        WeaponLogic = new WeaponLogic();
        // 武器ロジックバインド
        WeaponBind = WeaponLogic.Bind();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void OnResolved()
    {
        WeaponBind
            // Idling出力時
            .Handle((in WeaponLogic.Output.Idling _) =>
            {
                // 物理処理無効化
                SetPhysicsProcess(false);
            })
            // Attacking出力時
            .Handle((in WeaponLogic.Output.Attacking _) =>
            {
                // 物理処理有効化
                SetPhysicsProcess(true);
            });
        // 敵発見イベント
        Radar.Searched += OnSearched;
        // 敵未発見イベント
        Radar.NotSearched += OnNotSearched;
        // 武器ロジック初期状態開始
        WeaponLogic.Start();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="delta"></param>
    public void OnPhysicsProcess(double delta)
    {
        // センターマーカーと発射口マーカーから武器の方向を計算
        WeaponDirection = CenterMarker.GlobalPosition.DirectionTo(Marker2D.GlobalPosition);
        // レーダーにて索敵された敵のうち一番近い敵のArea2Dを取得
        Area2D? enemy = Radar.OnAreaEnemies.FirstOrDefault();
        if (enemy != null)
        {
            // 敵の方向を取得
            TargetDirection = CenterMarker.GlobalPosition.DirectionTo(enemy.GlobalPosition);
            // 敵方向の角度を計算
            float AngleToTarget = Math.Abs(WeaponDirection.AngleTo(TargetDirection));
            // 外積計算
            float Gaiseki = WeaponDirection.Cross(TargetDirection);

            // 外積・敵方向の角度・ローテーションスピード・シグモイド関数から
            // 回転角度を計算
            float rotation = Math.Sign(Gaiseki) * Math.Sign(AngleToTarget) * RotationSpeed * Sigmoid(1, AngleToTarget);

            // 回転する
            Rotate(rotation);
        }
        // 射撃を行う
        BulletFactory.Shoot();
    }

    /// <summary>
    /// 敵発見イベントファンクション
    /// </summary>
    public void OnSearched()
    {
        // StartAttackを入力
        WeaponLogic.Input(new WeaponLogic.Input.StartAttack());
    }

    /// <summary>
    /// 敵未発見イベントファンクション
    /// </summary>
    public void OnNotSearched()
    {
        // StartIdleを入力
        WeaponLogic.Input(new WeaponLogic.Input.StartIdle());
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
