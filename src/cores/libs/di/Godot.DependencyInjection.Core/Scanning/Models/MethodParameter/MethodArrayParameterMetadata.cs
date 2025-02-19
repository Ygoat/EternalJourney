namespace Godot.DependencyInjection.Scanning.Models.MethodParameterMetadata;

using System;
using System.Diagnostics;
using Godot.DependencyInjection.Scanning.Models.Shared;

[DebuggerDisplay("{DebugDisplay(),nq}")]
internal readonly struct MethodArrayParameterMetadata(Type parameterType) : IMethodParameterMetadata
{
    private readonly Type _parameterType = parameterType;

    public object? GetService(IServiceProvider serviceProvider)
    {
        var services = serviceProvider.GetServicesArray(_parameterType);
        return services;
    }

    public override string ToString()
    {
        var toString = $@"
                    {{
                        ""type"": ""{_parameterType.FullName}[]"",
                    }}";
        return toString;
    }

    public string DebugDisplay()
    {
        var display = $@"{_parameterType.FullName}[]";
        return display;
    }
}
