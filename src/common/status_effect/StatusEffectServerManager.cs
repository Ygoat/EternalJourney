namespace EternalJourney.Common.StatusEffect;

using System;
using System.Collections.Generic;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;

/// <summary>
/// 状態異常付与マネージャーインターフェース
/// </summary>
public interface IStatusEffectServerManager : INode
{
}

/// <summary>
/// 状態異常付与マネージャークラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class StatusEffectServerManager : Node, IStatusEffectServerManager
{
    public override void _Notification(int what) => this.Notify(what);

    // 状態異常の有効・無効を管理（状態異常が増えても変更不要）
    private readonly Dictionary<Type, bool> _effectEnabled = new()
    {
        { typeof(PoisonEffect), false },
    };

    // 状態異常の登録
    public void Configure<T>(bool enabled) where T : StatusEffect
    {
        _effectEnabled[typeof(T)] = enabled;
    }

    // 適用
    public void Apply(IStatusEffectReceiverManager manager)
    {
        foreach (var kvp in _effectEnabled)
        {
            if (!kvp.Value) continue;

            var effect = manager.Get(kvp.Key);
            effect?.Apply();
        }
    }
}
