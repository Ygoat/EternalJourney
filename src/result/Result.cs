namespace EternalJourney.Result;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;

/// <summary>
/// リザルトインターフェース
/// </summary>
public interface IResult : IControl
{
}

/// <summary>
/// リザルトクラス（Show/Hide のみ担当）
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class Result : Control, IResult
{
    public override void _Notification(int what) => this.Notify(what);
}
