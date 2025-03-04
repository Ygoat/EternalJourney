namespace EternalJourney;

using System;

public interface IAppRepo : IDisposable
{
    event Action? GameEntered;
}


public class AppRepo : IAppRepo
{
    public event Action? GameEntered;

    public void Dispose() { }
}
