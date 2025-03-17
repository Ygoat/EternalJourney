namespace EternalJourney;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.App.Domain;
using Godot;

/// <summary>
/// スプラッシュインターフェース
/// </summary>
public interface ISplash : IControl
{
}

/// <summary>
/// スプラッシュクラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class Splash : Control, ISplash
{
    public override void _Notification(int what) => this.Notify(what);

    #region Nodes
    /// <summary>
    /// スプラッシュ表示時間
    /// </summary>
    [Node]
    public ITimer SplashTimer { get; set; } = default!;
    #endregion Nodes

    #region Dependencies
    /// <summary>
    /// アプリケーションレポジトリ
    /// </summary>
    [Dependency]
    public IAppRepo AppRepo => this.DependOn<IAppRepo>();
    #endregion Dependencies

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void OnReady()
    {
        SplashTimer.Timeout += SplashTimeOut;
        SplashTimer.SetWaitTime(2);
        SplashTimer.Start();
    }

    /// <summary>
    /// タイムアウトイベントファンクション
    /// </summary>
    public void SplashTimeOut()
    {
        GD.Print("splash timeout");
        AppRepo.SkipSplashScreen();
    }
}
