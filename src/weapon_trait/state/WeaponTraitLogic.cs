namespace EternalJourney.WeaponTrait.State;

using System;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Cores.Models.Weapon;

/// <summary>
/// 武器特性ロジックインターフェース
/// </summary>
public interface IWeaponTraitLogic : ILogicBlock;

/// <summary>
/// 武器特性ロジック
/// リロード・オーバーヒート・チャージ・バッテリーのゲージを管理し、撃てる／撃てないを状態で表す
/// </summary>
[Meta]
public partial class WeaponTraitLogic : LogicBlock, IWeaponTraitLogic
{
    public WeaponTraitLogic()
    {
        // ステートを事前確保
        Set(new WeaponTraitState.Disabled());
        Set(new WeaponTraitState.Ready());
        Set(new WeaponTraitState.Locked());
        Set(new Gauge());
    }

    /// <summary>
    /// ゲージの値（全状態で共有する）
    /// </summary>
    public sealed class Gauge
    {
        /// <summary>
        /// 現在の設定
        /// </summary>
        public WeaponTraitSettings Settings { get; set; } = WeaponTraitSettings.None;

        /// <summary>
        /// ゲージ量（残弾・熱・チャージ量・エネルギー）
        /// </summary>
        public float Value { get; set; }

        /// <summary>
        /// 最後に撃ってからの経過秒数
        /// </summary>
        public float SinceShot { get; set; }

        /// <summary>
        /// Locked に入ってからの経過秒数
        /// </summary>
        public float LockElapsed { get; set; }

        /// <summary>
        /// 表示用のゲージ比率（0〜1）
        /// </summary>
        public float Ratio(bool locked)
        {
            WeaponTraitSettings s = Settings;
            // リロード中は進捗を表示する
            if (locked && s.Type == WeaponTraitType.Reload)
            {
                return s.LockTime > 0f ? Math.Clamp(LockElapsed / s.LockTime, 0f, 1f) : 1f;
            }
            return s.Capacity > 0f ? Math.Clamp(Value / s.Capacity, 0f, 1f) : 0f;
        }
    }

    /// <summary>
    /// 入力定義
    /// </summary>
    public static class Input
    {
        /// <summary>
        /// 特性を設定し直す（ゲージは初期値に戻る）
        /// </summary>
        public readonly record struct Configure(WeaponTraitSettings Settings);

        /// <summary>
        /// 同じ特性のまま設定値だけを差し替える（ゲージは維持する）
        /// </summary>
        public readonly record struct UpdateSettings(WeaponTraitSettings Settings);

        /// <summary>
        /// 時間経過
        /// </summary>
        /// <param name="Delta">経過秒数</param>
        /// <param name="Engaged">敵を捕捉しているか（チャージが溜まる条件）</param>
        public readonly record struct Tick(double Delta, bool Engaged);

        /// <summary>
        /// 1発撃った
        /// </summary>
        public readonly record struct Fired;
    }

    /// <summary>
    /// 出力定義
    /// </summary>
    public static class Output
    {
        /// <summary>
        /// ゲージの変化
        /// </summary>
        /// <param name="Type">特性の種類（表示色の切り替え用）</param>
        /// <param name="Ratio">ゲージ比率（0〜1）</param>
        /// <param name="Locked">撃てない状態か</param>
        public readonly record struct GaugeChanged(WeaponTraitType Type, float Ratio, bool Locked);
    }
}

/// <summary>
/// 武器特性状態定義
/// </summary>
[StateDiagram]
public abstract partial record WeaponTraitState : LogicBlockState,
    IGet<WeaponTraitLogic.Input.Configure>,
    IGet<WeaponTraitLogic.Input.UpdateSettings>,
    IGet<WeaponTraitLogic.Input.Tick>,
    IGet<WeaponTraitLogic.Input.Fired>
{
    public Type On(in WeaponTraitLogic.Input.Configure input)
    {
        WeaponTraitLogic.Gauge gauge = Get<WeaponTraitLogic.Gauge>();
        WeaponTraitSettings s = input.Settings;
        gauge.Settings = s;
        gauge.SinceShot = 0f;
        gauge.LockElapsed = 0f;
        // 残弾・エネルギーは満タン、熱・チャージは空から始める
        gauge.Value = s.Type is WeaponTraitType.Reload or WeaponTraitType.Battery ? s.Capacity : 0f;

        Type next = s.Type switch
        {
            WeaponTraitType.None => To<Disabled>(),
            WeaponTraitType.Charge => To<Locked>(),
            _ => To<Ready>(),
        };
        Output(new WeaponTraitLogic.Output.GaugeChanged(s.Type, gauge.Ratio(s.Type == WeaponTraitType.Charge), s.Type == WeaponTraitType.Charge));
        return next;
    }

    public Type On(in WeaponTraitLogic.Input.UpdateSettings input)
    {
        WeaponTraitLogic.Gauge gauge = Get<WeaponTraitLogic.Gauge>();
        if (input.Settings.Type != gauge.Settings.Type)
        {
            // 特性の種類が変わるときは設定し直す
            return On(new WeaponTraitLogic.Input.Configure(input.Settings));
        }
        gauge.Settings = input.Settings;
        gauge.Value = Math.Min(gauge.Value, input.Settings.Capacity);
        return ToSelf();
    }

    public virtual Type On(in WeaponTraitLogic.Input.Tick input) => ToSelf();

    public virtual Type On(in WeaponTraitLogic.Input.Fired input) => ToSelf();

    /// <summary>
    /// ゲージの現在値を通知する
    /// </summary>
    protected void NotifyGauge(bool locked)
    {
        WeaponTraitLogic.Gauge gauge = Get<WeaponTraitLogic.Gauge>();
        Output(new WeaponTraitLogic.Output.GaugeChanged(gauge.Settings.Type, gauge.Ratio(locked), locked));
    }

    /// <summary>
    /// 撃っていない間の回復（オーバーヒートは冷却、バッテリーは充電）
    /// </summary>
    protected static void Recover(WeaponTraitLogic.Gauge gauge, float delta)
    {
        WeaponTraitSettings s = gauge.Settings;
        gauge.SinceShot += delta;
        if (gauge.SinceShot < s.RecoverDelay)
        {
            return;
        }
        if (s.Type == WeaponTraitType.Overheat)
        {
            gauge.Value = Math.Max(0f, gauge.Value - s.RecoverRate * delta);
        }
        else if (s.Type == WeaponTraitType.Battery)
        {
            gauge.Value = Math.Min(s.Capacity, gauge.Value + s.RecoverRate * delta);
        }
    }

    /// <summary>
    /// 特性なし（常に撃てる）
    /// </summary>
    public record Disabled : WeaponTraitState;

    /// <summary>
    /// 撃てる
    /// </summary>
    public record Ready : WeaponTraitState
    {
        public override Type On(in WeaponTraitLogic.Input.Tick input)
        {
            Recover(Get<WeaponTraitLogic.Gauge>(), (float)input.Delta);
            NotifyGauge(false);
            return ToSelf();
        }

        public override Type On(in WeaponTraitLogic.Input.Fired input)
        {
            WeaponTraitLogic.Gauge gauge = Get<WeaponTraitLogic.Gauge>();
            WeaponTraitSettings s = gauge.Settings;
            gauge.SinceShot = 0f;
            bool lockNow;
            if (s.Type == WeaponTraitType.Overheat)
            {
                gauge.Value += s.CostPerShot;
                lockNow = gauge.Value >= s.Capacity;
            }
            else
            {
                // リロード・チャージ・バッテリーは消費し、次の1発分が足りなければ止まる
                gauge.Value = Math.Max(0f, gauge.Value - s.CostPerShot);
                lockNow = gauge.Value < s.CostPerShot;
            }

            if (!lockNow)
            {
                NotifyGauge(false);
                return ToSelf();
            }
            if (s.Type == WeaponTraitType.Overheat)
            {
                gauge.Value = s.Capacity;
            }
            gauge.LockElapsed = 0f;
            NotifyGauge(true);
            return To<Locked>();
        }
    }

    /// <summary>
    /// 撃てない（リロード中・オーバーヒート中・チャージ中・充電中）
    /// </summary>
    public record Locked : WeaponTraitState
    {
        public override Type On(in WeaponTraitLogic.Input.Tick input)
        {
            WeaponTraitLogic.Gauge gauge = Get<WeaponTraitLogic.Gauge>();
            WeaponTraitSettings s = gauge.Settings;
            float delta = (float)input.Delta;
            gauge.LockElapsed += delta;

            bool release = false;
            switch (s.Type)
            {
                case WeaponTraitType.Reload:
                    if (gauge.LockElapsed >= s.LockTime)
                    {
                        gauge.Value = s.Capacity;
                        release = true;
                    }
                    break;
                case WeaponTraitType.Overheat:
                    // 停止時間の間に熱を 0 まで下げる
                    float progress = s.LockTime > 0f ? Math.Min(1f, gauge.LockElapsed / s.LockTime) : 1f;
                    gauge.Value = s.Capacity * (1f - progress);
                    release = progress >= 1f;
                    break;
                case WeaponTraitType.Charge:
                    // 敵を捕捉している間だけ溜まる
                    if (input.Engaged)
                    {
                        float rate = s.LockTime > 0f ? s.Capacity / s.LockTime : s.Capacity;
                        gauge.Value = Math.Min(s.Capacity, gauge.Value + rate * delta);
                    }
                    release = gauge.Value >= s.Capacity;
                    break;
                case WeaponTraitType.Battery:
                    Recover(gauge, delta);
                    release = gauge.Value >= s.Capacity * s.ResumeRatio && gauge.Value >= s.CostPerShot;
                    break;
                default:
                    release = true;
                    break;
            }

            NotifyGauge(!release);
            return release ? To<Ready>() : ToSelf();
        }
    }
}
