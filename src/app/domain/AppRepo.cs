namespace EternalJourney;

using System;

public interface IAppRepo : IDisposable
{
    event Action? GameEntered;

    event Action? SplashScreenSkipped;

    void SkipSplashScreen();
}


public class AppRepo : IAppRepo
{
    public event Action? GameEntered;

    public event Action? SplashScreenSkipped;

    public void SkipSplashScreen()
    {
        SplashScreenSkipped?.Invoke();
    }

    public void Dispose() { }
}
