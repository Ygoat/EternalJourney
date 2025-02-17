namespace Godot.DependencyInjection.Scanning.Models.MethodParameterMetadata;

using System;


internal interface IMethodParameterMetadata
{
    object? GetService(IServiceProvider serviceProvider);
    string ToString();

    string DebugDisplay();
}
