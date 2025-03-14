namespace EternalJourney.Weapon;

using System.Linq;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Radar;
using Godot;

/// <summary>
/// 武器インターフェース
/// </summary>
public interface IWeapon : INode2D, IProvide<IWeapon>
{
    public IMarker2D Marker2D { get; set; }
    public IMarker2D CenterMarker { get; set; }
};

[Meta(typeof(IAutoNode))]
public partial class Weapon : Node2D, IWeapon
{
    public override void _Notification(int what) => this.Notify(what);

    #region State
    #endregion State
    #region Exports

    public int RotateSpeed { get; set; } = 1;
    public Vector2 TargetDirection { get; set; } = default!;

    public Vector2 WeaponDirection { get; set; } = default!;
    #endregion Exports
    #region PackedScenes
    #endregion PackedScenes
    #region Nodes
    [Node]
    public IMarker2D Marker2D { get; set; } = default!;

    [Node]
    public IMarker2D CenterMarker { get; set; } = default!;

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
    #region Dependencies
    #endregion Dependencies

    public void Initialize()
    {
        this.Provide();
    }

    public void Setup()
    {
        SetPhysicsProcess(true);
    }

    public void OnPhysicsProcess(double delta)
    {
        WeaponDirection = CenterMarker.GlobalPosition.DirectionTo(Marker2D.GlobalPosition);
        Area2D? enemy = Radar.OnAreaEnemies.FirstOrDefault();
        if (enemy != null)
        {
            TargetDirection = CenterMarker.GlobalPosition.DirectionTo(enemy.GlobalPosition);
            float rotation = WeaponDirection.AngleTo(TargetDirection);
            Rotate(rotation);
        }
    }
}
