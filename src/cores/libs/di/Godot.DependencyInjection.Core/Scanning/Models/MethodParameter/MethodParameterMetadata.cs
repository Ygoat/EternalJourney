namespace Godot.DependencyInjection.Scanning.Models.MethodParameter;

using System;
using System.Diagnostics;
using Godot.DependencyInjection.Scanning.Models.MethodParameterMetadata;
using Microsoft.Extensions.DependencyInjection;

[DebuggerDisplay("{DebugDisplay(),nq}")]
internal readonly struct MethodParameterMetadata(bool isRequired, Type parameterType) : IMethodParameterMetadata
{
    private readonly bool _isRequired = isRequired;
    private readonly Type _parameterType = parameterType;


    public object? GetService(IServiceProvider serviceProvider)
    {
        var service = _isRequired
        ? serviceProvider.GetRequiredService(_parameterType)
        : serviceProvider.GetService(_parameterType);

        return service;
    }


    public override string ToString()
    {
        var toString = $@"
                    {{
                        ""type"": ""{_parameterType.FullName}"",
                        ""isRequired"": {_isRequired.ToString().ToLowerInvariant()}
                    }}";
        return toString;
    }
    public string DebugDisplay()
    {
        var toString = $@"{_parameterType.FullName}({_isRequired})";
        return toString;
    }
}
