namespace EternalJourney.Common.DurabilityModule;

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
    /// 耐久値残存シグナル
    /// </summary>
    public event DurabilityModule.DurabilityLeftEventHandler DurabilityLeft;

    /// <summary>
    /// 耐久値ゼロシグナル
    /// </summary>
    public event DurabilityModule.ZeroDurabilityEventHandler ZeroDurability;

    /// <summary>
    /// 耐久値の設定
    /// </summary>
    /// <param name="maxValue">最大値</param>
    /// <param name="currentRatio">最大値からの現在体力比</param>
    public void SetDurability(float maxValue, float currentRatio = 1);

    /// <summary>
    /// 耐久値減少
    /// </summary>
    /// <param name="damageValue"></param>
    public void TakeDamage(float damageValue);

    /// <summary>
    /// 耐久値回復
    /// </summary>
    /// <param name="repairValue"></param>
    public void Repair(float repairValue);

    /// <summary>
    /// 耐久値全回復
    /// </summary>
    public void FullRepir();

    /// <summary>
    /// 現在耐久値を取得する
    /// </summary>
    /// <returns></returns>
    public float GetCurrentDurability();

    /// <summary>
    /// 最大値からの現在耐久比を取得する
    /// </summary>
    /// <returns></returns>
    public float GetCurrentRatio();

    /// <summary>
    /// 耐久値を最大値にする
    /// </summary>
    public void MaximizeDurability();
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
    /// 耐久値最大シグナル
    /// </summary>
    [Signal]
    public delegate void MaxDurabilityEventHandler();

    /// <summary>
    /// 耐久値ゼロシグナル
    /// </summary>
    [Signal]
    public delegate void ZeroDurabilityEventHandler();

    /// <summary>
    /// 耐久値残存シグナル
    /// </summary>
    [Signal]
    public delegate void DurabilityLeftEventHandler();
    #endregion Signals

    #region Exports

    /// <summary>
    /// 最大耐久値
    /// </summary>
    [ExportGroup("Durability Setting")]
    [Export(PropertyHint.Range, "0.1,10000,0.1")]
    private float _maxValue { get; set; } = 10;

    /// <summary>
    /// 最大値からの現在耐久比
    /// </summary>
    [ExportGroup("Durability Setting")]
    [Export(PropertyHint.Range, "0,1,0.01")]
    private float _currentRatio { get; set; } = 1;

    /// <summary>
    /// 耐久値のAutoProp
    /// </summary>
    private AutoProp<float> _durability { get; set; } = default!;
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
    public void SetDurability(float maxValue, float currentRatio = 1)
    {
        _maxValue = maxValue;
        _currentRatio = currentRatio;
        _durability = new AutoProp<float>(maxValue * currentRatio);
        _durability.Sync += OnDurabilityChanged;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="damageValue"></param>
    public void TakeDamage(float damageValue)
    {
        // 耐久値減少
        float nextDurability = _durability.Value - damageValue;
        // 減少後の耐久値が0以下の時
        if (nextDurability <= 0)
        {
            nextDurability = 0;
        }
        // 耐久値をセット
        _durability.OnNext(nextDurability);
        // 現在耐久値比を計算
        CalcCurrentRatio();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="repairValue"></param>
    public void Repair(float repairValue)
    {
        // 耐久値回復
        float nextDurability = _durability.Value + repairValue;
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
    public void FullRepir()
    {
        Repair(_maxValue);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    public float GetCurrentDurability()
    {
        return _durability.Value;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    public float GetCurrentRatio()
    {
        return _currentRatio;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    public void MaximizeDurability()
    {
        _durability.OnNext(_maxValue);
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
    private void OnDurabilityChanged(float value)
    {
        // 耐久値が0以下の時
        if (value <= 0)
        {
            OnDurabilityUnderZero();
        }
        // 耐久値が最大値以上の時
        else if (value >= _maxValue)
        {
            OnDurabilityFulled();
        }
        else
        {
            OnDurabilityLeft();
        }
    }

    /// <summary>
    /// 耐久値ゼロ以下イベントファンクション
    /// </summary>
    private void OnDurabilityUnderZero()
    {
        EmitSignal(SignalName.ZeroDurability);
    }

    /// <summary>
    /// 耐久値最大イベントファンクション
    /// </summary>
    private void OnDurabilityFulled()
    {
        EmitSignal(SignalName.MaxDurability);
    }

    /// <summary>
    /// 耐久値残存イベントファンクション
    /// </summary>
    private void OnDurabilityLeft()
    {
        EmitSignal(SignalName.DurabilityLeft);
    }
}
