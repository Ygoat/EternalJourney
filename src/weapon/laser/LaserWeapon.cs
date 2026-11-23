namespace EternalJourney.Weapon.Laser;

using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using EternalJourney.Cores.Consts;
using EternalJourney.Weapon.Continuous;
using Godot;

/// <summary>
/// レーザー武器インターフェース
/// </summary>
public interface ILaserWeapon : IContinuousWeapon;

/// <summary>
/// レーザー武器クラス（ターゲットに向けてビームを伸ばし、照射し続ける）
/// 貫通なし: 最初に当たった敵で止まり、その敵だけにダメージを与える
/// 貫通あり: 最大距離まで伸び、ビームに触れた全ての敵にダメージを与える
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class LaserWeapon : ContinuousWeapon, ILaserWeapon
{
    public override void _Notification(int what) => this.Notify(what);

    #region Exports
    /// <summary>
    /// 貫通するか
    /// </summary>
    [Export]
    public bool Piercing { get; set; }

    /// <summary>
    /// レーザーの太さ（武器ローカル座標）
    /// </summary>
    [Export]
    public float Thickness { get; set; } = 6f;

    /// <summary>
    /// ビームの色
    /// </summary>
    [Export]
    public Color BeamColor { get; set; } = new Color(1f, 0.3f, 0.3f, 0.9f);
    #endregion Exports

    #region Nodes
    /// <summary>
    /// ビームの表示
    /// </summary>
    [Node]
    public Line2D Beam { get; set; } = default!;

    /// <summary>
    /// 非貫通時に最初の敵を探すレイ
    /// </summary>
    [Node]
    public RayCast2D Ray { get; set; } = default!;

    /// <summary>
    /// ビームの当たり判定形状
    /// </summary>
    [Node]
    public CollisionShape2D BeamShape { get; set; } = default!;
    #endregion Nodes

    public override void OnResolved()
    {
        base.OnResolved();

        // 見た目
        Beam.Width = Thickness;
        Beam.DefaultColor = BeamColor;
        Beam.Visible = false;

        // 当たり判定（共有されないよう実行時に生成する）
        BeamShape.Shape = new RectangleShape2D { Size = new Vector2(0f, Thickness) };
        DamageArea.MaxTargets = Piercing ? 0 : 1;

        // 最初の敵を探すレイ（最大距離まで伸ばしておく）
        Ray.TargetPosition = new Vector2(MaxReach, 0f);
        Ray.CollideWithAreas = true;
        Ray.CollideWithBodies = false;
        Ray.CollisionMask = CollisionEntity.Enemy;
        Ray.Enabled = !Piercing;
    }

    protected override float GetObstructionDistance()
    {
        if (Piercing)
        {
            return float.PositiveInfinity;
        }

        Ray.ForceRaycastUpdate();
        if (!Ray.IsColliding())
        {
            return float.PositiveInfinity;
        }

        // ワールド距離を武器ローカル座標に直す
        return Ray.GlobalPosition.DistanceTo(Ray.GetCollisionPoint()) / GlobalScale.X;
    }

    protected override void OnReachChanged(float reach)
    {
        Beam.Visible = true;
        Beam.SetPointPosition(1, new Vector2(reach, 0f));

        ((RectangleShape2D)BeamShape.Shape).Size = new Vector2(reach, Thickness);
        BeamShape.Position = new Vector2(reach / 2f, 0f);
    }

    protected override void OnChannelEnded()
    {
        Beam.Visible = false;
    }
}
