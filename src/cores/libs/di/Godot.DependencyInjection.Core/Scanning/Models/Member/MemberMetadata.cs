namespace Godot.DependencyInjection.Scanning.Models.Member;

using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

[DebuggerDisplay("{DebugDisplay(),nq}")]
internal readonly struct MemberMetadata(Type serviceType, MemberMetadata.MemberSetter memberSetter, bool isRequired) : IMemberMetadata
{
    public delegate void MemberSetter(object? instance, object? value);

    private readonly Type _serviceType = serviceType;
    private readonly MemberSetter _memberSetter = memberSetter;
    private readonly bool _isRequired = isRequired;


    /// <inheritdoc/>
    public void Inject(IServiceProvider serviceProvider, object instance)
    {
        var service = _isRequired
            ? serviceProvider.GetRequiredService(_serviceType)
            : serviceProvider.GetService(_serviceType);
        _memberSetter.Invoke(instance, service);
    }

    public override string ToString()
    {
        var toString = $@"
            {{
                ""type"": ""{_serviceType.FullName}"",
                ""isRequired"": {_isRequired.ToString().ToLowerInvariant()}
            }}";
        return toString;
    }

    /// <inheritdoc/>
    public string DebugDisplay()
    {
        var display = $@"{_serviceType.FullName}, isRequired: {_isRequired}";
        return display;
    }

}
