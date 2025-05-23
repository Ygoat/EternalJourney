using System;
using System.Collections.Generic;
using System.Reflection;
using Godot.DependencyInjection.Attributes;
using Godot.DependencyInjection.Scanning;
using Godot.DependencyInjection.Scanning.Models;

namespace Godot.DependencyInjection.Injection;

/// <summary>
/// Represents a service that handles dependency injection for nodes.
/// </summary>
public class InjectionService
{
    private static readonly Dictionary<Type, InjectionMetadata> Metadata;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes static members of the <see cref="InjectionService"/> class.
    /// </summary>
    static InjectionService()
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        Metadata = InjectionScanner.CollectMetadata(assemblies);
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="InjectionService"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency injection.</param>
    public InjectionService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Injects dependencies into the specified node.
    /// </summary>
    /// <param name="node">The node to inject dependencies into.</param>
    public void InjectDependencies(object node)
    {
        if (node.GetType().GetCustomAttribute<IgnoreDependencyInjectionAttribute>() is not null)
        {
            return;
        }

        var type = node.GetType();
        var memberQueue = new Queue<(Type type, object? instance)>();
        memberQueue.Enqueue((type, instance: node));

        while (memberQueue.TryDequeue(out var element))
        {
            if (element.instance is null
                || !Metadata.TryGetValue(element.type, out var metadata))
            {
                continue;
            }
            metadata.Inject(_serviceProvider, element.instance);
            for (int i = 0; i < metadata.nestedInjections.Length; i++)
            {
                var nestedType = metadata.nestedInjections[i].nestedType;
                var nestedinstance = metadata.nestedInjections[i].memberGetter(element.instance);
                memberQueue.Enqueue((
                    type: nestedType,
                    instance: nestedinstance
                ));
            }
        }
    }

}
