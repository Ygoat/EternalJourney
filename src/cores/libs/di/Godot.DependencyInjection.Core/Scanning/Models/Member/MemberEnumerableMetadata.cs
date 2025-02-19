namespace Godot.DependencyInjection.Scanning.Models.Member;

using System;
using System.Diagnostics;
using Godot.DependencyInjection.Scanning.Models.Shared;
using static Godot.DependencyInjection.Scanning.Models.Member.MemberMetadata;

[DebuggerDisplay("{DebugDisplay(),nq}")]
internal readonly struct MemberEnumerableMetadata(Type serviceType, MemberSetter memberSetter) : IMemberMetadata
{
    private readonly Type _serviceType = serviceType;
    private readonly MemberSetter _memberSetter = memberSetter;


    /// <inheritdoc/>
    public void Inject(IServiceProvider serviceProvider, object instance)
    {
        var services = serviceProvider.GetServicesEnumerable(_serviceType);
        _memberSetter.Invoke(instance, services);
    }

    public override string ToString()
    {
        var toString = $@"
            {{
                ""type"": ""System.Collections.Generic.IEnumerable<{_serviceType.FullName}>"",
            }}";
        return toString;
    }

    /// <inheritdoc/>
    public string DebugDisplay()
    {
        var toString = $@"System.Collections.Generic.IEnumerable<{_serviceType.FullName}>";
        return toString;
    }

}
