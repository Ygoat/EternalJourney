namespace EternalJourney.Common.StatusEffect;

using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using EternalJourney.Common.StatusEffect.State;
using Godot;

/// <summary>
/// スタン効果インターフェース
/// </summary>
public interface IStunEffect : IStatusEffect
{
    public event StunEffect.StunnedEventHandler Stunned;
    public event StunEffect.StunEndedEventHandler StunEnded;
}

/// <summary>
/// スタン効果クラス（一定時間、敵の移動を停止させる）
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class StunEffect : StatusEffect, IStunEffect
{
    public override void _Notification(int what) => this.Notify(what);

    /// <summary>
    /// スタン開始シグナル
    /// </summary>
    [Signal]
    public delegate void StunnedEventHandler();

    /// <summary>
    /// スタン終了シグナル
    /// </summary>
    [Signal]
    public delegate void StunEndedEventHandler();

    /// <summary>
    /// スタンロジック
    /// </summary>
    public StunEffectLogic StunEffectLogic { get; set; } = default!;

    /// <summary>
    /// スタンバインド
    /// </summary>
    public StunEffectLogic.IBinding StunEffectBinding { get; set; } = default!;

    /// <summary>
    /// スタンタイマー
    /// </summary>
    public Timer StunTimer { get; set; } = default!;

    /// <summary>
    /// スタン持続時間（秒）
    /// </summary>
    public float StunDuration { get; set; }

    public void Initialize()
    {
        StunTimer = new Timer();
        StunDuration = 2.0f;
        RemoveTime = StunDuration;
    }

    public void OnReady()
    {
        StunTimer.WaitTime = StunDuration;
        StunTimer.OneShot = true;
        StunTimer.Timeout += OnStunTimerTimeout;
        AddChild(StunTimer);
    }

    public void Setup()
    {
        StunEffectLogic = new StunEffectLogic();
        StunEffectBinding = StunEffectLogic.Bind();
    }

    public void OnResolved()
    {
        StunEffectBinding
            .Handle((in StunEffectLogic.Output.Activated _) =>
            {
                StunTimer.Start();
                EmitSignal(SignalName.Stunned);
            })
            .Handle((in StunEffectLogic.Output.Deactivated _) =>
            {
                StunTimer.Stop();
                StunTimer.WaitTime = StunDuration;
                EmitSignal(SignalName.StunEnded);
            })
            .Watch((in StunEffectLogic.Input.Apply _) =>
            {
                // 重ねがけ：タイマーをリセットして延長
                StunTimer.Stop();
                StunTimer.WaitTime = StunDuration;
                StunTimer.Start();
            });
        StunEffectLogic.Start();
    }

    /// <summary>
    /// スタンを適用する
    /// </summary>
    public override void Apply()
    {
        StunEffectLogic?.Input(new StunEffectLogic.Input.Apply());
    }

    /// <summary>
    /// スタンを解除する
    /// </summary>
    public override void Remove()
    {
        StunEffectLogic?.Input(new StunEffectLogic.Input.Remove());
    }

    /// <summary>
    /// スタンタイマーのタイムアウト
    /// </summary>
    private void OnStunTimerTimeout()
    {
        StunEffectLogic.Input(new StunEffectLogic.Input.Remove());
    }
}
