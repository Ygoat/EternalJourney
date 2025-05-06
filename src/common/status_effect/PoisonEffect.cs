namespace EternalJourney.Common.StatusEffect;

using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using EternalJourney.Battle.Domain;
using EternalJourney.Common.StatusEffect.State;
using Godot;

/// <summary>
/// 毒効果インターフェース
/// </summary>
public interface IPoisonEffect : IStatusEffect
{
    public event PoisonEffect.AppliedEventHandler Applied;
    public event PoisonEffect.DamagedEventHandler Damaged;
}

/// <summary>
/// 毒効果クラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class PoisonEffect : StatusEffect, IPoisonEffect
{
    public override void _Notification(int what) => this.Notify(what);

    [Signal]
    public delegate void AppliedEventHandler();

    [Signal]
    public delegate void DamagedEventHandler(float damage);


    /// <summary>
    /// 毒ロジック
    /// </summary>
    public PoisonEffectLogic PoisonEffectLogic { get; set; } = default!;

    /// <summary>
    /// 毒バインド
    /// </summary>
    public PoisonEffectLogic.IBinding PoisonEffectBinding { get; set; } = default!;

    /// <summary>
    /// ダメージタイマー
    /// </summary>
    public Timer DamageTimer { get; set; } = default!;

    /// <summary>
    /// 除去タイマー
    /// </summary>
    public Timer RemoveTimer { get; set; } = default!;

    public float DamageDuration { get; set; } = default!;

    public float PoisonDamage { get; set; } = default!;

    [Dependency] public IBattleRepo BattleRepo => this.DependOn<IBattleRepo>();

    public void Setup()
    {
        DamageTimer = new Timer();
        RemoveTimer = new Timer();
        RemoveTime = 10;
        DamageDuration = 1;

        PoisonEffectLogic = new PoisonEffectLogic();
        PoisonEffectBinding = PoisonEffectLogic.Bind();

        PoisonEffectLogic.Set(this as IPoisonEffect);
        PoisonEffectLogic.Set(BattleRepo);
    }

    public void OnResolved()
    {
        // タイマーをシーンツリーに追加して有効化
        AddChild(DamageTimer);
        AddChild(RemoveTimer);
        // ダメージタイマーの間隔設定
        DamageTimer.WaitTime = DamageDuration;
        // ダメージタイマーのタイムアウトイベント設定
        DamageTimer.Timeout += OnDamageTimerTimeout;
        // 除去タイマーの時間設定
        RemoveTimer.WaitTime = RemoveTime;
        // ワンショット設定
        RemoveTimer.OneShot = true;
        // 除去タイマーのタイムアウトイベント設定
        RemoveTimer.Timeout += OnRemoveTimerTimeout;

        // ステートロジック設定
        PoisonEffectBinding
            .When<PoisonEffectLogic.State.Active>(state =>
            {
                // タイマー開始
                DamageTimer.Start();
                RemoveTimer.Start();
                // 毒ダメージ設定
                PoisonDamage = state.PoisonDamage;
            })
            .When<PoisonEffectLogic.State.InActive>(state =>
            {
                // タイマーの停止と初期化
                DamageTimer.Stop();
                DamageTimer.WaitTime = DamageDuration;
                RemoveTimer.Stop();
                RemoveTimer.WaitTime = RemoveTime;
            })
            .Watch((in PoisonEffectLogic.Input.Apply input) =>
            {
                // 除去タイマーリセット
                RemoveTimer.Stop();
                RemoveTimer.WaitTime = RemoveTime;
                RemoveTimer.Start();
            });
        // 初期状態開始
        PoisonEffectLogic.Start();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="target"></param>
    public override void Apply()
    {
        PoisonEffectLogic.Input(new PoisonEffectLogic.Input.Apply());
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="target"></param>
    public override void Remove()
    {
        PoisonEffectLogic.Input(new PoisonEffectLogic.Input.Remove());
    }

    /// <summary>
    /// ダメージタイマーのタイムアウトイベントファンクション
    /// </summary>
    private void OnDamageTimerTimeout()
    {
        EmitSignal(SignalName.Damaged, PoisonDamage);
    }

    /// <summary>
    /// 除去タイマーのタイムアウトイベントファンクション
    /// </summary>
    private void OnRemoveTimerTimeout()
    {
        Remove();
    }
}
