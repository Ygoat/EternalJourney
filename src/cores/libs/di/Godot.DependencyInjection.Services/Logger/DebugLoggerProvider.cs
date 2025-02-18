using System;
using Microsoft.Extensions.Logging;

namespace Godot.DependencyInjection.Services.Logger;

/// <summary>
/// The provider for the <see cref="DebugLogger"/>.
/// </summary>
[ProviderAlias("Godot")]
public class DebugLoggerProvider : ILoggerProvider
{
    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName)
    {
        return new GodotLogger(categoryName);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
