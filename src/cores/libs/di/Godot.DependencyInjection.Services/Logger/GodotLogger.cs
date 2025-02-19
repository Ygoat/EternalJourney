namespace Godot.DependencyInjection.Services.Logger;

using System;
using Microsoft.Extensions.Logging;

/// <summary>
/// A logger that writes messages in the debug output window only when a debugger is attached.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GodotLogger"/> class.
/// </remarks>
/// <param name="name">The name of the logger.</param>
internal sealed class GodotLogger(string name) : ILogger
{
    private readonly string _name = name;


    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel)
    {
        // If the filter is null, everything is enabled
        var isEnabled = logLevel != LogLevel.None;
        return isEnabled;
    }

    /// <inheritdoc />
    public IDisposable BeginScope<TState>(TState state)
    {
        IDisposable iDisposable = NullScope.Instance;
        return iDisposable;
    }


    /// <inheritdoc />
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }
        ArgumentNullException.ThrowIfNull(formatter);

        var message = formatter(state, exception);

        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        message = $"{logLevel}: {_name}: {message}";

        if (exception != null)
        {
            message += Environment.NewLine + Environment.NewLine + exception;
        }
        PrintMessage(logLevel, message);
    }
    private static void PrintMessage(LogLevel logLevel, string message)
    {
        if (logLevel is LogLevel.Error or LogLevel.Critical)
        {
            GD.PrintErr(message);
        }
        else
        {
            GD.Print(message);
        }

    }
}
