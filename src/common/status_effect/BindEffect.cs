namespace EternalJourney.Common.StatusEffect;

using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using EternalJourney.Battle.Domain;
using EternalJourney.Common.StatusEffect.State;
using Godot;

/// <summary>
/// バインド効果インターフェース
/// </summary>
public interface IBindEffect : IStatusEffect
{
    public event BindEffect.AppliedEventHandler Applied;
    public event BindEffect.BindedEventHandler Binded;
    public event BindEffect.ReleasedEventHandler Released;
}

/// <summary>
/// バインド効果クラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class BindEffect : StatusEffect, IBindEffect
{
    public override void _Notification(int what) => this.Notify(what);

    [Signal]
    public delegate void AppliedEventHandler();

    [Signal]
    public delegate void BindedEventHandler(float duration);

    [Signal]
    public delegate void ReleasedEventHandler();

    /// <summary>
    /// バインドロジック
    /// </summary>
    public BindEffectLogic BindEffectLogic { get; set; } = default!;

    /// <summary>
    /// バインドバインディング
    /// </summary>
    public BindEffectLogic.IBinding BindEffectBinding { get; set; } = default!;

    /// <summary>
    /// バインドタイマー
    /// </summary>
    public Timer BindTimer { get; set; } = default!;

    public float BindDuration { get; set; } = 3.0f; // デフォルトバインド時間

    [Dependency] public IBattleRepo BattleRepo => this.DependOn<IBattleRepo>();

    public void Setup()
    {
        BindTimer = new Timer();
        BindTimer.OneShot = true;

        BindEffectLogic = new BindEffectLogic();
        BindEffectBinding = BindEffectLogic.Bind();

        BindEffectLogic.Set(this as IBindEffect);
        BindEffectLogic.Set(BattleRepo);
    }

    public void OnResolved()
    {
        AddChild(BindTimer);
        BindTimer.WaitTime = BindDuration;
        BindTimer.Timeout += OnBindTimerTimeout;

        BindEffectBinding
            .When<BindEffectLogic.State.Active>(state =>
            {
                // バインド状態になったらタイマー開始
                BindTimer.WaitTime = state.BindDuration;
                BindTimer.Start();
                EmitSignal(SignalName.Binded, state.BindDuration);
            })
            .When<BindEffectLogic.State.InActive>(state =>
            {
                // バインド解除時タイマー停止
                BindTimer.Stop();
            })
            .Watch((in BindEffectLogic.Input.Apply input) =>
            {
                // アクティブ中に再度バインド→タイマーリセット
                BindTimer.Stop();
                BindTimer.WaitTime = input.Duration;
                BindTimer.Start();
                EmitSignal(SignalName.Applied);
            });
        BindEffectLogic.Start();
    }

    public override void Apply()
    {
        BindEffectLogic.Input(new BindEffectLogic.Input.Apply(BindDuration));
    }

    public override void Remove()
    {
        BindEffectLogic.Input(new BindEffectLogic.Input.Remove());
        EmitSignal(SignalName.Released);
    }

    private void OnBindTimerTimeout()
    {
        Remove();
    }
}
