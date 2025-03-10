namespace EternalJourney.Weapon;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;

/// <summary>
/// 武器インターフェース
/// </summary>
public interface IWeapon : INode2D, IProvide<IWeapon>
{
    public IMarker2D Marker2D { get; set; }
};

[Meta(typeof(IAutoNode))]
public partial class Weapon : Node2D, IWeapon
{
    public override void _Notification(int what) => this.Notify(what);

    #region State
    #endregion State
    #region Exports
    #endregion Exports
    #region PackedScenes
    #endregion PackedScenes
    #region Nodes
    [Node]
    public IMarker2D Marker2D { get; set; } = default!;
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
}
