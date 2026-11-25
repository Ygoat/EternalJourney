namespace EternalJourney.WeaponTrait;

using System;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Cores.Models.Weapon;
using EternalJourney.WeaponTrait.State;
using Godot;

/// <summary>
/// 武器特性インターフェース
/// </summary>
public interface IWeaponTrait : INode2D
{
    /// <summary>
    /// 今撃てるか（Locked 状態でなければ撃てる）
    /// </summary>
    bool CanFire { get; }

    /// <summary>
    /// 1発あたりの威力倍率
    /// </summary>
    float DamageMultiplier { get; }

    /// <summary>
    /// 敵を捕捉しているかを返す関数（チャージが溜まる条件）
    /// </summary>
    Func<bool> EngagedProvider { get; set; }

    /// <summary>
    /// 撃てない状態から撃てる状態に戻ったときのイベント
    /// </summary>
    event Action? Released;

    /// <summary>
    /// 特性を設定し直す（null は特性なし）
    /// </summary>
    /// <param name="config">特性の設定</param>
    /// <param name="capacityScale">ゲージ容量の倍率（敵の武器は小さくする）</param>
    void Configure(WeaponTraitConfig? config, float capacityScale = 1f);

    /// <summary>
    /// 特性を無効にする
    /// </summary>
    void Disable();

    /// <summary>
    /// 1発撃ったことを通知する
    /// </summary>
    void NotifyFired();
}

/// <summary>
/// 武器特性クラス（ゲージの表示と、武器からの通知を Logic へ中継する）
/// 武器の回転・拡大に影響されないよう TopLevel で描画する
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class WeaponTrait : Node2D, IWeaponTrait
{
    public override void _Notification(int what)
    {
        this.Notify(what);
        // プールから再出現した敵の武器は、ゲージを初期状態に戻す
        if (what == NotificationEnterTree && _configured)
        {
            Configure(_config, _capacityScale);
        }
        // プール運用される敵の武器の子にもなるため、ツリー離脱ではなく破棄時に解放する
        if (what == NotificationPredelete)
        {
            _binding?.Dispose();
            (_logic as IDisposable)?.Dispose();
        }
    }

    #region Exports
    /// <summary>
    /// 武器の中心からの表示位置
    /// </summary>
    [Export]
    public Vector2 Offset { get; set; } = new(-20f, 28f);

    /// <summary>
    /// ゲージの大きさ
    /// </summary>
    [Export]
    public Vector2 GaugeSize { get; set; } = new(40f, 5f);

    /// <summary>
    /// リロードの色
    /// </summary>
    [Export]
    public Color ReloadColor { get; set; } = new(1f, 0.9f, 0.3f);

    /// <summary>
    /// オーバーヒートの色
    /// </summary>
    [Export]
    public Color OverheatColor { get; set; } = new(1f, 0.35f, 0.2f);

    /// <summary>
    /// チャージの色
    /// </summary>
    [Export]
    public Color ChargeColor { get; set; } = new(0.4f, 0.8f, 1f);

    /// <summary>
    /// バッテリーの色
    /// </summary>
    [Export]
    public Color BatteryColor { get; set; } = new(0.4f, 1f, 0.5f);

    /// <summary>
    /// 撃てない間の点滅速度（回/秒）
    /// </summary>
    [Export]
    public float BlinkSpeed { get; set; } = 6f;
    #endregion Exports

    private IWeaponTraitLogic? _logic;
    private LogicBlock.Binding? _binding;
    private Node2D? _anchor;

    // 再出現時に設定し直すための直近の設定
    private WeaponTraitConfig? _config;
    private float _capacityScale = 1f;
    private bool _configured;

    // 描画用の値（Output で更新する）
    private float _ratio;
    private bool _locked;
    private Color _color = Colors.White;
    private float _blinkTime;

    public bool CanFire => _logic?.State is not WeaponTraitState.Locked;

    public float DamageMultiplier { get; private set; } = 1f;

    public Func<bool> EngagedProvider { get; set; } = () => false;

    public event Action? Released;

    public void Setup()
    {
        TopLevel = true;
        Visible = false;
        _logic = new WeaponTraitLogic();
        _binding = _logic.Bind();
        _binding.OnOutput((in WeaponTraitLogic.Output.GaugeChanged output) =>
        {
            bool wasLocked = _locked;
            _ratio = output.Ratio;
            _locked = output.Locked;
            if (wasLocked && !_locked)
            {
                Released?.Invoke();
            }
            _color = ColorOf(output.Type);
            Visible = output.Type != WeaponTraitType.None;
            QueueRedraw();
        });
        _logic.Start<WeaponTraitState.Disabled>();
        SetProcess(true);
    }

    public void OnReady()
    {
        _anchor = GetParent() as Node2D;
    }

    public void Configure(WeaponTraitConfig? config, float capacityScale = 1f)
    {
        _config = config;
        _capacityScale = capacityScale;
        _configured = true;
        WeaponTraitSettings settings = (config?.ToSettings() ?? WeaponTraitSettings.None).WithCapacityScale(capacityScale);
        DamageMultiplier = settings.ShotDamageMultiplier;
        _logic?.Input(new WeaponTraitLogic.Input.Configure(settings));
    }

    public void Disable() => Configure(null);

    public void NotifyFired() => _logic?.Input(new WeaponTraitLogic.Input.Fired());

    public void OnProcess(double delta)
    {
        if (_anchor != null)
        {
            GlobalPosition = _anchor.GlobalPosition + Offset;
        }
        if (_logic?.State is WeaponTraitState.Disabled)
        {
            return;
        }
        _logic?.Input(new WeaponTraitLogic.Input.Tick(delta, EngagedProvider()));
        if (_locked)
        {
            _blinkTime += (float)delta;
            QueueRedraw();
        }
    }

    public override void _Draw()
    {
        DrawRect(new Rect2(Vector2.Zero, GaugeSize), new Color(0f, 0f, 0f, 0.6f));
        Color color = _color;
        if (_locked)
        {
            // 撃てない間は点滅させる
            color.A = 0.35f + 0.65f * Mathf.Abs(Mathf.Sin(_blinkTime * BlinkSpeed * Mathf.Pi));
        }
        DrawRect(new Rect2(Vector2.Zero, new Vector2(GaugeSize.X * _ratio, GaugeSize.Y)), color);
    }

    private Color ColorOf(WeaponTraitType type) => type switch
    {
        WeaponTraitType.Reload => ReloadColor,
        WeaponTraitType.Overheat => OverheatColor,
        WeaponTraitType.Charge => ChargeColor,
        WeaponTraitType.Battery => BatteryColor,
        _ => Colors.White,
    };
}
