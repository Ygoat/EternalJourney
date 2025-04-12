namespace EternalJourney.Common.DurabilityModule;

using System;
using System.ComponentModel.DataAnnotations;
using Chickensoft.AutoInject;
using Chickensoft.Collections;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;

/// <summary>
/// 耐久値モジュールインターフェース
/// </summary>
public interface IDurabilityModule : INode
{
    /// <summary>
    /// 耐久値最大シグナル
    /// </summary>
    public event DurabilityModule.MaxDurabilityEventHandler MaxDurability;

    /// <summary>
    /// 耐久値ゼロシグナル
    /// </summary>
    public event DurabilityModule.ZeroDurabilityEventHandler ZeroDurability;

    /// <summary>
    /// 耐久値の設定
    /// </summary>
    /// <param name="maxValue">最大値</param>
    /// <param name="currentRatio">最大値からの現在体力比</param>
    public void SetDurability(double maxValue, double currentRatio);

    /// <summary>
    /// 耐久値減少
    /// </summary>
    /// <param name="damageValue"></param>
    public void TakeDamage(double damageValue);

    /// <summary>
    /// 耐久値回復
    /// </summary>
    /// <param name="repairValue"></param>
    public void Repair(double repairValue);

    /// <summary>
    /// 現在耐久値を取得する
    /// </summary>
    /// <returns></returns>
    public double GetCurrentDurability();

    /// <summary>
    /// 最大値からの現在耐久比を取得する
    /// </summary>
    /// <returns></returns>
    public double GetCurrentRatio();
}

/// <summary>
/// 耐久値モジュールクラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class DurabilityModule : Node, IDurabilityModule
{
    public override void _Notification(int what) => this.Notify(what);

    #region Signals
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [Signal]
    public delegate void MaxDurabilityEventHandler();

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [Signal]
    public delegate void ZeroDurabilityEventHandler();
    #endregion Signals

    #region Exports

    /// <summary>
    /// 最大耐久値
    /// </summary>
    [ExportGroup("Durability Setting")]
    [Export(PropertyHint.Range, "-10000,10000,0.1")]
    [Range(-10000, 10000)]
    private double _maxValue { get; set; }

    /// <summary>
    /// 最大値からの現在耐久比
    /// </summary>
    [ExportGroup("Durability Setting")]
    [Export(PropertyHint.Range, "0,1,0.01")]
    [Range(0, 1)]
    private double _currentRatio { get; set; }

    /// <summary>
    /// 耐久値のAutoProp
    /// </summary>
    private AutoProp<double> _durability { get; set; } = default!;
    #endregion Exports

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Setup()
    {
        // 耐久値設定
        SetDurability(_maxValue, _currentRatio);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="maxValue"></param>
    /// <param name="currentRatio"></param>
    public void SetDurability(double maxValue, double currentRatio = 1)
    {
        _maxValue = maxValue;
        _currentRatio = currentRatio;
        _durability = new AutoProp<double>(maxValue * currentRatio);
        _durability.Sync += OnDurabilityChanged;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="damageValue"></param>
    public void TakeDamage(double damageValue)
    {
        // 耐久値減少
        double nextDurability = _durability.Value - damageValue;
        // 耐久値をセット
        _durability.OnNext(nextDurability);
        // 現在耐久値比を計算
        CalcCurrentRatio();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="repairValue"></param>
    public void Repair(double repairValue)
    {
        // 耐久値回復
        double nextDurability = _durability.Value + repairValue;
        // 回復後の耐久値が最大値以上の時
        if (nextDurability >= _maxValue)
        {
            nextDurability = _maxValue;
        }
        // 耐久値をセット
        _durability.OnNext(nextDurability);
        // 現在耐久値比を計算
        CalcCurrentRatio();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    public double GetCurrentDurability()
    {
        return _durability.Value;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    public double GetCurrentRatio()
    {
        return _currentRatio;
    }

    /// <summary>
    /// 現在耐久値比を計算
    /// </summary>
    private void CalcCurrentRatio()
    {
        _currentRatio = _durability.Value / _maxValue;
    }

    /// <summary>
    /// 耐久値変更イベントファンクション
    /// </summary>
    private void OnDurabilityChanged(double value)
    {
        // 耐久値が0以下の時
        if (value <= 0)
        {
            OnDurabilityUnderZero(value);
        }
        // 耐久値が最大値以上の時
        else if (value >= _maxValue)
        {
            OnDurabilityFulled(value);
        }
    }

    /// <summary>
    /// 耐久値ゼロ以下イベントファンクション
    /// </summary>
    private void OnDurabilityUnderZero(double value)
    {
        EmitSignal(SignalName.ZeroDurability);
    }

    /// <summary>
    /// 耐久値最大イベントファンクション
    /// </summary>
    private void OnDurabilityFulled(double value)
    {
        EmitSignal(SignalName.MaxDurability);
    }
}
