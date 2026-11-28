namespace EternalJourney.WeaponMount;

using System;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Battle.Domain;
using EternalJourney.Cores.Utils;
using EternalJourney.Game.Domain;
using EternalJourney.Weapon.Base;
using EternalJourney.WeaponMount.State;
using Godot;

/// <summary>
/// 武器搭載インターフェース
/// </summary>
public interface IWeaponMount : INode2D
{
}

/// <summary>
/// 武器搭載クラス（銃座の位置を持ち、指示された武器シーンを生成して配置する）
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class WeaponMount : Node2D, IWeaponMount
{
    public override void _Notification(int what) => this.Notify(what);

    #region State
    public IWeaponMountLogic Logic { get; set; } = default!;
    public LogicBlock.Binding Binding { get; set; } = default!;
    #endregion State

    #region Exports
    /// <summary>
    /// 搭載する武器の拡大率
    /// </summary>
    [Export]
    public Vector2 WeaponScale { get; set; } = new(2f, 2f);
    #endregion Exports

    #region Nodes
    [Node]
    public IMarker2D Slot0 { get; set; } = default!;

    [Node]
    public IMarker2D Slot1 { get; set; } = default!;

    [Node]
    public IMarker2D Slot2 { get; set; } = default!;

    [Node]
    public IMarker2D Slot3 { get; set; } = default!;
    #endregion Nodes

    #region Dependencies
    [Dependency]
    public IBattleRepo BattleRepo => this.DependOn<IBattleRepo>();

    [Dependency]
    public IGameRepo GameRepo => this.DependOn<IGameRepo>();
    #endregion Dependencies

    public void Setup()
    {
        Logic = new WeaponMountLogic();
        Binding = Logic.Bind();
    }

    public void OnResolved()
    {
        Logic.Set(BattleRepo);
        Logic.Set(GameRepo);
        Binding.OnOutput((in WeaponMountLogic.Output.Mount output) => Mount(output.Slot, output.ScenePath));
        Logic.Start<WeaponMountState.Waiting>();
    }

    /// <summary>
    /// 武器シーンを生成して銃座に置く
    /// </summary>
    private void Mount(int slot, string scenePath)
    {
        Node2D weapon = new Instantiator(GetTree()).LoadAndInstantiate<Node2D>(scenePath);
        weapon.Name = $"Weapon{slot}";
        weapon.Position = SlotPosition(slot);
        weapon.Scale = WeaponScale;
        if (weapon is IStationWeapon station)
        {
            station.StationIndex = slot;
        }
        AddChild(weapon);
    }

    private Vector2 SlotPosition(int slot) => slot switch
    {
        0 => Slot0.Position,
        1 => Slot1.Position,
        2 => Slot2.Position,
        3 => Slot3.Position,
        _ => Vector2.Zero,
    };

    public void OnExitTree()
    {
        Binding.Dispose();
        ((IDisposable)Logic).Dispose();
    }
}
