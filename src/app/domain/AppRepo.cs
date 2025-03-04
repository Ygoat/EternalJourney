namespace EternalJourney.App.Domain;

using System;

public interface IAppRepo : IDisposable
{
    event Action? GameEntered;

    event Action? SplashScreenSkipped;
    event Action? MainMenuEntered;

    void SkipSplashScreen();

    void OnEnterGame();
    void OnMainMenuEntered();
}


public class AppRepo : IAppRepo
{
    public event Action? GameEntered;
    public event Action? SplashScreenSkipped;
    public event Action? MainMenuEntered;

    public void SkipSplashScreen()
    {
        SplashScreenSkipped?.Invoke();
    }

    public void OnMainMenuEntered()
    {
        MainMenuEntered?.Invoke();
    }

    public void OnEnterGame()
    {
        GameEntered?.Invoke();
    }

    public void Dispose() { }
}
