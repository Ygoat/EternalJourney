namespace EternalJourney.CLASSNAME;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;

public interface ICLASSNAME : INODETYPE
{
}

[Meta(typeof(IAutoNode))]
public partial class CLASSNAME : NODETYPE, ICLASSNAME 
{
    public override void _Notification(int what) => this.Notify(what);

    #region Signals
    #endregion Signals
    #region State
    #endregion State
    #region Exports
    #endregion Exports
    #region PackedScenes
    #endregion PackedScenes
    #region Nodes
    #endregion Nodes
    #region Provisions
    #endregion Provisions
    #region Dependencies
    #endregion Dependencies

    public void OnInitialized()
    {
    }

    public void OnReady()
    {
    }

    public void OnResolved()
    {
    }

    public void Setup()
    {
    }
}
