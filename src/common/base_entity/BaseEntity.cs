namespace EternalJourney.Common.BaseEntity;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;

/// <summary>
/// ベースエンティティインターフェース
/// </summary>
public interface IBaseEntity : INode2D
{
}

/// <summary>
/// ベースエンティティクラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class BaseEntity : Node2D, IBaseEntity
{
    public override void _Notification(int what) => this.Notify(what);
}
