namespace EternalJourney.App.Domain;

using System;

/// <summary>
/// アプリケーションレポジトリインターフェース
/// </summary>
public interface IAppRepo : IDisposable
{
    /// <summary>
    /// ゲーム開始イベント
    /// </summary>
    event Action? GameEntered;

    /// <summary>
    /// スプラッシュ画面スキップイベント
    /// </summary>
    event Action? SplashScreenSkipped;

    /// <summary>
    /// メインメニュー開始イベント
    /// </summary>
    event Action? MainMenuEntered;

    /// <summary>
    /// スプラッシュ画面スキップイベントファンクション
    /// </summary>

    void SkipSplashScreen();

    /// <summary>
    /// ゲーム開始イベントファンクション
    /// </summary>
    void OnEnterGame();

    /// <summary>
    /// メインメニュー開始イベントファンクション
    /// </summary>
    void OnMainMenuEntered();
}


public class AppRepo : IAppRepo
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event Action? GameEntered;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event Action? SplashScreenSkipped;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event Action? MainMenuEntered;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void SkipSplashScreen()
    {
        SplashScreenSkipped?.Invoke();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void OnMainMenuEntered()
    {
        MainMenuEntered?.Invoke();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void OnEnterGame()
    {
        GameEntered?.Invoke();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Dispose() { }
}
