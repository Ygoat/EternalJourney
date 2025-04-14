using System;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Common.BaseEntity;
using Godot;

public interface IBaseBullet : INode2D
{
}

[Meta(typeof(IAutoNode))]
public partial class BaseBullet : BaseEntity, IBaseBullet
{
}
