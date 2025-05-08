namespace EternalJourney.Common.StatusEffect;

using System;
using System.Collections.Generic;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;

/// <summary>
/// 状態異常マネージャーインターフェース
/// </summary>
public interface IProvideStatusEffectManager : INode
{
}

/// <summary>
/// 状態異常マネージャークラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class ProvideStatusEffectManager : Node, IProvideStatusEffectManager
{
    public override void _Notification(int what) => this.Notify(what);

    // 状態異常の有効・無効を管理(状態異常が増えたら追加する)
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
    public void Apply(IStatusEffectManager manager)
    {
        foreach (var kvp in _effectEnabled)
        {
            var effectType = kvp.Key;
            var enabled = kvp.Value;

            if (!enabled)
            {
                continue;
            }

            // 状態異常インスタンスを取得して Apply()
            var effect = GetStatusEffectInstance(effectType, manager);
            effect?.Apply();
        }
    }

    private StatusEffect? GetStatusEffectInstance(Type type, IStatusEffectManager manager)
    {
        // 型に応じて manager からインスタンスを取り出す
        // ここは状態異常の種類が増えたら対応を増やします
        if (type == typeof(PoisonEffect))
        {
            return manager.PoisonEffect;
        }
        // else if (type == typeof(BurnEffect))
        //     return manager.BurnEffect;

        return null;
    }
}
