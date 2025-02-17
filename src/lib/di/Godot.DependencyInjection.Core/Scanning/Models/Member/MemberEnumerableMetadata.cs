using System;
using System.Diagnostics;
using Godot.DependencyInjection.Scanning.Models.Shared;
using static Godot.DependencyInjection.Scanning.Models.Member.MemberMetadata;

namespace Godot.DependencyInjection.Scanning.Models.Member;

[DebuggerDisplay("{DebugDisplay(),nq}")]
internal readonly struct MemberEnumerableMetadata : IMemberMetadata
{
    private readonly Type _serviceType;
    private readonly MemberSetter _memberSetter;

    public MemberEnumerableMetadata(Type serviceType, MemberSetter memberSetter)
    {
        _serviceType = serviceType;
        _memberSetter = memberSetter;
    }

    /// <inheritdoc/>
    public void Inject(IServiceProvider serviceProvider, object instance)
    {
        var services = serviceProvider.GetServicesEnumerable(_serviceType);
        _memberSetter.Invoke(instance, services);
    }

    public override string ToString()
    {
        return $@"
            {{
                ""type"": ""System.Collections.Generic.IEnumerable<{_serviceType.FullName}>"",
            }}";
    }

    /// <inheritdoc/>
    public string DebugDisplay()
    {
        return $@"System.Collections.Generic.IEnumerable<{_serviceType.FullName}>";
    }

}
