namespace EternalJourney.Cores.Models.Weapon;

/// <summary>
/// 武器特性の種類
/// </summary>
public enum WeaponTraitType
{
    /// <summary>
    /// 特性なし（常に撃てる）
    /// </summary>
    None,

    /// <summary>
    /// リロード: 弾数を撃ち切るとリロード時間の間撃てない
    /// </summary>
    Reload,

    /// <summary>
    /// オーバーヒート: 熱が上限に達すると一定時間撃てない。撃たない間は冷える
    /// </summary>
    Overheat,

    /// <summary>
    /// チャージ: 敵を捕捉している間に溜まり、満タンで威力倍率付きで撃つ
    /// </summary>
    Charge,

    /// <summary>
    /// バッテリー: エネルギーがある限り撃てる。撃たない間は回復する
    /// </summary>
    Battery,
}

/// <summary>
/// 武器特性の設定値
/// </summary>
/// <param name="Type">特性の種類</param>
/// <param name="Capacity">ゲージ容量（装弾数・最大熱量・最大エネルギー。チャージは満タン量）</param>
/// <param name="CostPerShot">1発あたりの消費量（熱は増加量）</param>
/// <param name="RecoverRate">毎秒の回復量（冷却量・エネルギー回復量）</param>
/// <param name="RecoverDelay">撃ち止めてから回復が始まるまでの秒数</param>
/// <param name="LockTime">リロード時間・オーバーヒート時間・チャージ時間（秒）</param>
/// <param name="ResumeRatio">バッテリーが撃てるようになるゲージ比率</param>
/// <param name="DamageMultiplier">チャージ時の威力倍率</param>
public sealed record WeaponTraitSettings(
    WeaponTraitType Type,
    float Capacity = 1f,
    float CostPerShot = 1f,
    float RecoverRate = 0f,
    float RecoverDelay = 0f,
    float LockTime = 1f,
    float ResumeRatio = 0.3f,
    float DamageMultiplier = 1f
)
{
    /// <summary>
    /// 特性なしの設定
    /// </summary>
    public static readonly WeaponTraitSettings None = new(WeaponTraitType.None);

    /// <summary>
    /// 1発あたりの威力倍率（チャージのみ倍率が掛かる）
    /// </summary>
    public float ShotDamageMultiplier => Type == WeaponTraitType.Charge ? DamageMultiplier : 1f;

    /// <summary>
    /// ゲージ容量を倍率で変えた設定を返す（敵の武器用など）
    /// チャージは満タンで1発撃つ仕組みのため、容量を変えると撃てなくなるので対象外
    /// </summary>
    public WeaponTraitSettings WithCapacityScale(float scale) =>
        Type is WeaponTraitType.None or WeaponTraitType.Charge
            ? this
            : this with { Capacity = Capacity * scale };

    /// <summary>
    /// リロード・オーバーヒート・チャージの時間を倍率で変えた設定を返す（クルーのパッシブ用）
    /// </summary>
    public WeaponTraitSettings WithLockTimeScale(float scale) =>
        Type == WeaponTraitType.None ? this : this with { LockTime = LockTime * scale };
}
