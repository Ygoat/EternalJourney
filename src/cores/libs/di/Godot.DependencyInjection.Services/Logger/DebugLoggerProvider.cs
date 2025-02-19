namespace Godot.DependencyInjection.Services.Logger;

using System;
using Microsoft.Extensions.Logging;

/// <summary>
/// The provider for the <see cref="DebugLogger"/>.
/// </summary>
[ProviderAlias("Godot")]
public class DebugLoggerProvider : ILoggerProvider
{
    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName)
    {
        ILogger iLogger = new GodotLogger(categoryName);
        return iLogger;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        return;
    }
}
