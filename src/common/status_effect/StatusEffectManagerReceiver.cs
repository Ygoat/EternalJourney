namespace EternalJourney.Common.StatusEffect;

using System;
using System.Collections.Generic;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;

/// <summary>
/// 状態異常受信マネージャーインターフェース
/// </summary>
public interface IStatusEffectReceiverManager : INode
{
    /// <summary>
    /// 型で状態異常インスタンスを取得する
    /// </summary>
    StatusEffect? Get(Type type);

    /// <summary>
    /// 全ての状態異常を除去する
    /// </summary>
    void RemoveAll();
}

/// <summary>
/// 状態異常受信マネージャークラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class StatusEffectReceiverManager : Node, IStatusEffectReceiverManager
{
    public override void _Notification(int what) => this.Notify(what);

    private readonly Dictionary<Type, StatusEffect> _effects;

    public StatusEffectReceiverManager()
    {
        // new() で生成されるため Initialize() は呼ばれない → コンストラクタで登録
        // 状態異常を追加する場合はここに1行追加するだけ
        _effects = new Dictionary<Type, StatusEffect>();
        RegisterEffect(new PoisonEffect());
        RegisterEffect(new StunEffect());
    }

    public virtual void OnReady()
    {
        // コンストラクタで生成済みのエフェクトをシーンツリーに追加
        foreach (var effect in _effects.Values)
        {
            AddChild(effect);
        }
    }

    public virtual void Setup() { }

    public virtual void OnResolved() { }

    /// <summary>
    /// 型で状態異常インスタンスを取得する（インターフェース実装）
    /// </summary>
    public StatusEffect? Get(Type type) =>
        _effects.TryGetValue(type, out var e) ? e : null;

    /// <summary>
    /// 型で状態異常インスタンスを取得する（ジェネリック版）
    /// </summary>
    public T? Get<T>() where T : StatusEffect => Get(typeof(T)) as T;

    public virtual void RemoveAll()
    {
        foreach (var effect in _effects.Values)
        {
            effect.Remove();
        }
    }

    private void RegisterEffect(StatusEffect effect) =>
        _effects[effect.GetType()] = effect;
}
