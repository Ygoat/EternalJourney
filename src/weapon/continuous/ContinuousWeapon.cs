namespace EternalJourney.Weapon.Continuous;

using System;
using System.Linq;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Battle.Domain;
using EternalJourney.Radar;
using EternalJourney.Weapon.Abstract.Base;
using EternalJourney.Weapon.Continuous.State;
using EternalJourney.WeaponTrait;
using Godot;

/// <summary>
/// 継続照射武器インターフェース
/// </summary>
public interface IContinuousWeapon : IBaseWeapon;

/// <summary>
/// 継続照射武器クラス（レーザー・放射武器の共通部分）
/// ターゲットがいる間、その方向へ照射し続ける。形状の更新は派生クラスが行う。
/// 距離の単位は武器ローカル座標（Ship上では武器の拡大率が掛かって表示される）
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class ContinuousWeapon : BaseWeapon, IContinuousWeapon
{
    public override void _Notification(int what) => this.Notify(what);

    #region State
    /// <summary>
    /// 継続照射武器ロジック
    /// </summary>
    public IContinuousWeaponLogic ContinuousWeaponLogic { get; set; } = default!;

    /// <summary>
    /// 継続照射武器ロジックバインド
    /// </summary>
    public LogicBlock.Binding ContinuousWeaponBinding { get; set; } = default!;

    /// <summary>
    /// ターゲット方向
    /// </summary>
    public Vector2 TargetDirection { get; set; }
    #endregion State

    #region Exports
    /// <summary>
    /// 回転速度
    /// </summary>
    [Export]
    public float RotationSpeed { get; set; } = 0.05f;

    /// <summary>
    /// 到達距離の伸びる速さ（武器ローカル座標/秒）
    /// </summary>
    [Export]
    public float ExtendSpeed { get; set; } = 500f;

    /// <summary>
    /// 最大到達距離（武器ローカル座標）
    /// </summary>
    [Export]
    public float MaxReach { get; set; } = 250f;

    /// <summary>
    /// ダメージ間隔（秒）
    /// </summary>
    [Export]
    public float DamageInterval { get; set; } = 0.1f;

    /// <summary>
    /// 1回あたりのダメージ
    /// </summary>
    [Export]
    public float Atk { get; set; } = 1f;

    /// <summary>
    /// 毒を付与するか
    /// </summary>
    [Export]
    public bool PoisonEnabled { get; set; }

    /// <summary>
    /// スタンを付与するか
    /// </summary>
    [Export]
    public bool StunEnabled { get; set; }

    /// <summary>
    /// 武器特性（リロード・オーバーヒート・チャージ・バッテリー。未設定は特性なし）
    /// </summary>
    [Export]
    public WeaponTraitConfig? TraitConfig { get; set; }
    #endregion Exports

    #region Nodes
    /// <summary>
    /// 照射口マーカー
    /// </summary>
    [Node]
    public IMarker2D Marker2D { get; set; } = default!;

    /// <summary>
    /// 武器の中心マーカー
    /// </summary>
    [Node]
    public IMarker2D CenterMarker { get; set; } = default!;

    /// <summary>
    /// レーダー
    /// </summary>
    [Node]
    public IRadar Radar { get; set; } = default!;

    /// <summary>
    /// 継続ダメージ判定エリア
    /// </summary>
    [Node]
    public ContinuousDamageArea DamageArea { get; set; } = default!;

    /// <summary>
    /// 武器特性
    /// </summary>
    [Node]
    public IWeaponTrait WeaponTrait { get; set; } = default!;
    #endregion Nodes

    #region Dependencies
    /// <summary>
    /// バトルリポジトリ（研究効果の参照に使う）
    /// </summary>
    [Dependency]
    public IBattleRepo BattleRepo => this.DependOn<IBattleRepo>();
    #endregion Dependencies

    public override void Setup()
    {
        base.Setup();
        ContinuousWeaponLogic = new ContinuousWeaponLogic();
        ContinuousWeaponBinding = ContinuousWeaponLogic.Bind();
    }

    public override void OnResolved()
    {
        base.OnResolved();

        ContinuousWeaponLogic.Set(new ContinuousWeaponLogic.Settings(ExtendSpeed, MaxReach, DamageInterval));
        ContinuousWeaponLogic.Set(BattleRepo);

        // 継続ダメージ判定に効果を伝える
        DamageArea.Atk = Atk;
        DamageArea.PoisonEnabled = PoisonEnabled;
        DamageArea.StunEnabled = StunEnabled;

        ContinuousWeaponBinding
            .OnOutput((in ContinuousWeaponLogic.Output.Idling _) =>
            {
                SetPhysicsProcess(false);
                DamageArea.SetActive(false);
                OnChannelEnded();
            })
            .OnOutput((in ContinuousWeaponLogic.Output.Channeling _) =>
            {
                SetPhysicsProcess(true);
                DamageArea.SetActive(true);
            })
            .OnOutput((in ContinuousWeaponLogic.Output.ReachChanged output) =>
            {
                OnReachChanged(output.Reach);
            })
            .OnOutput((in ContinuousWeaponLogic.Output.DamageTick _) =>
            {
                // 武器特性で撃てなくなったら照射を止める（撃てるようになったら Released で再開）
                if (!WeaponTrait.CanFire)
                {
                    ContinuousWeaponLogic.Input(new ContinuousWeaponLogic.Input.StopChannel());
                    return;
                }
                DamageArea.DealDamage(WeaponTrait.DamageMultiplier);
                WeaponTrait.NotifyFired();
            });

        // 武器特性: 敵を捕捉しているかを渡し、撃てるようになったら照射を再開する
        WeaponTrait.EngagedProvider = HasTarget;
        WeaponTrait.Released += OnTraitReleased;
        WeaponTrait.Configure(TraitConfig);

        // 敵発見・未発見イベント
        Radar.Searched += OnSearched;
        Radar.NotSearched += OnNotSearched;

        ContinuousWeaponLogic.Start<ContinuousWeaponState.Idle>();
    }

    public override void OnPhysicsProcess(double delta)
    {
        base.OnPhysicsProcess(delta);

        // レーダーで索敵された敵のうち一番近い敵の方向へ向ける
        Area2D? enemy = Radar.OnAreaEnemies.FirstOrDefault();
        if (enemy != null)
        {
            TargetDirection = CenterMarker.GlobalPosition.DirectionTo(enemy.GlobalPosition);
            GlobalRotation = Mathf.LerpAngle(GlobalRotation, TargetDirection.Angle(), RotationSpeed);
        }

        ContinuousWeaponLogic.Input(new ContinuousWeaponLogic.Input.PhysicsProcess(delta, GetObstructionDistance()));
    }

    /// <summary>
    /// 照射の先頭を遮る障害物までの距離（武器ローカル座標）を返す。遮られない場合は無限大
    /// </summary>
    protected virtual float GetObstructionDistance() => float.PositiveInfinity;

    /// <summary>
    /// 到達距離が更新された時の処理（派生クラスで形状を更新する）
    /// </summary>
    protected virtual void OnReachChanged(float reach)
    {
    }

    /// <summary>
    /// 照射が止まった時の処理（派生クラスで形状を隠す）
    /// </summary>
    protected virtual void OnChannelEnded()
    {
    }

    /// <summary>
    /// 敵発見イベントファンクション
    /// </summary>
    public void OnSearched()
    {
        // 武器特性で撃てない間は照射を始めない
        if (!WeaponTrait.CanFire)
        {
            return;
        }
        ContinuousWeaponLogic.Input(new ContinuousWeaponLogic.Input.StartChannel());
    }

    /// <summary>
    /// 敵を捕捉しているか
    /// </summary>
    private bool HasTarget() => Radar.OnAreaEnemies.Count > 0;

    /// <summary>
    /// 武器特性が撃てる状態に戻った時、敵がいれば照射を再開する
    /// </summary>
    private void OnTraitReleased()
    {
        if (HasTarget())
        {
            ContinuousWeaponLogic.Input(new ContinuousWeaponLogic.Input.StartChannel());
        }
    }

    /// <summary>
    /// 敵未発見イベントファンクション
    /// </summary>
    public void OnNotSearched()
    {
        ContinuousWeaponLogic.Input(new ContinuousWeaponLogic.Input.StopChannel());
    }

    public void OnExitTree()
    {
        WeaponTrait.Released -= OnTraitReleased;
        ContinuousWeaponBinding.Dispose();
        ((IDisposable)ContinuousWeaponLogic).Dispose();
    }
}
