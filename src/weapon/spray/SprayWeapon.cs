namespace EternalJourney.Weapon.Spray;

using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using EternalJourney.Weapon.Continuous;
using Godot;

/// <summary>
/// 放射武器インターフェース
/// </summary>
public interface ISprayWeapon : IContinuousWeapon;

/// <summary>
/// 放射武器クラス（火炎放射・冷気放射など、扇状に広がって照射し続ける）
/// 到達距離の伸びる速さは ExtendSpeed、最大射程は MaxReach で指定する
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class SprayWeapon : ContinuousWeapon, ISprayWeapon
{
    public override void _Notification(int what) => this.Notify(what);

    /// <summary>
    /// この射程未満では扇形を作らない（頂点が潰れるため）
    /// </summary>
    private const float MIN_RANGE = 1f;

    #region Exports
    /// <summary>
    /// 扇の開き角度（度）
    /// </summary>
    [Export]
    public float SpreadAngle { get; set; } = 50f;

    /// <summary>
    /// 円弧の分割数
    /// </summary>
    [Export]
    public int ArcSegments { get; set; } = 12;

    /// <summary>
    /// 放射の色
    /// </summary>
    [Export]
    public Color ConeColor { get; set; } = new Color(1f, 0.45f, 0.1f, 0.55f);
    #endregion Exports

    #region Nodes
    /// <summary>
    /// 扇形の表示
    /// </summary>
    [Node]
    public Polygon2D Cone { get; set; } = default!;

    /// <summary>
    /// 扇形の当たり判定
    /// </summary>
    [Node]
    public CollisionPolygon2D ConeShape { get; set; } = default!;
    #endregion Nodes

    public override void OnResolved()
    {
        base.OnResolved();

        Cone.Color = ConeColor;
        Cone.Visible = false;
    }

    protected override void OnReachChanged(float reach)
    {
        if (reach < MIN_RANGE)
        {
            return;
        }

        Vector2[] points = ConeGeometry.Build(reach, SpreadAngle, ArcSegments);
        Cone.Polygon = points;
        ConeShape.Polygon = points;
        Cone.Visible = true;
    }

    protected override void OnChannelEnded()
    {
        Cone.Visible = false;
    }
}
