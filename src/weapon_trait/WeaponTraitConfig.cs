namespace EternalJourney.WeaponTrait;

using EternalJourney.Cores.Models.Weapon;
using Godot;

/// <summary>
/// 武器特性の設定リソース（武器シーンの Export で設定する）
/// </summary>
[GlobalClass]
public partial class WeaponTraitConfig : Resource
{
    /// <summary>
    /// 特性の種類
    /// </summary>
    [Export]
    public WeaponTraitType Type { get; set; } = WeaponTraitType.None;

    /// <summary>
    /// ゲージ容量（装弾数・最大熱量・最大エネルギー。チャージは満タン量）
    /// </summary>
    [Export]
    public float Capacity { get; set; } = 1f;

    /// <summary>
    /// 1発あたりの消費量（熱は増加量）
    /// </summary>
    [Export]
    public float CostPerShot { get; set; } = 1f;

    /// <summary>
    /// 毎秒の回復量（冷却量・エネルギー回復量）
    /// </summary>
    [Export]
    public float RecoverRate { get; set; }

    /// <summary>
    /// 撃ち止めてから回復が始まるまでの秒数
    /// </summary>
    [Export]
    public float RecoverDelay { get; set; }

    /// <summary>
    /// リロード時間・オーバーヒート時間・チャージ時間（秒）
    /// </summary>
    [Export]
    public float LockTime { get; set; } = 1f;

    /// <summary>
    /// バッテリーが撃てるようになるゲージ比率
    /// </summary>
    [Export(PropertyHint.Range, "0,1,0.05")]
    public float ResumeRatio { get; set; } = 0.3f;

    /// <summary>
    /// チャージ時の威力倍率
    /// </summary>
    [Export]
    public float DamageMultiplier { get; set; } = 1f;

    /// <summary>
    /// ロジック用の設定値に変換する
    /// </summary>
    public WeaponTraitSettings ToSettings() =>
        new(Type, Capacity, CostPerShot, RecoverRate, RecoverDelay, LockTime, ResumeRatio, DamageMultiplier);
}
