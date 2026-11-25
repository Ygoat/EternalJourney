namespace EternalJourney.Battle.Domain;

using System;
using System.Collections.Generic;
using System.Linq;
using Chickensoft.Sync.Primitives;
using EternalJourney.Bullet.Abstract.Base;
using EternalJourney.Cores.Models.Research;
using EternalJourney.Enemy.Base;

/// <summary>
/// バトルレポジトリクラス
/// </summary>
public interface IBattleRepo : IDisposable
{
    /// <summary>
    /// 毒ダメージ
    /// </summary>
    public float PoisonDamage { get; set; }

    public float ShipAtkBonus { get; set; }
    public float ShipSpdMultiplier { get; set; }
    public float ShipDefBonus { get; set; }

    public float WeaponAtkMultiplier { get; set; }
    public float WeaponSpdMultiplier { get; set; }

    /// <summary>
    /// 武器速度乗数変更イベント
    /// </summary>
    public event Action? WeaponSpdMultiplierChanged;

    public float BulletAtkMultiplier { get; set; }
    public float BulletSpdMultiplier { get; set; }
    public float BulletDefBonus { get; set; }

    /// <summary>
    /// スコア
    /// </summary>
    public IAutoValue<int> Score { get; }

    /// <summary>
    /// エネミー撃破数
    /// </summary>
    public IAutoValue<int> NumEnemyDestroyed { get; }

    /// <summary>
    /// スクラップ所持数（バトル中のみ保持）
    /// </summary>
    public IAutoValue<int> Scrap { get; }

    /// <summary>
    /// 次回の研究コスト（スクラップ）
    /// </summary>
    public IAutoValue<int> ResearchCost { get; }

    /// <summary>
    /// 研究による累計強化値
    /// </summary>
    public IAutoValue<ResearchBonus> Research { get; }

    /// <summary>
    /// 取得済みの研究カード一覧
    /// </summary>
    public IAutoValue<IReadOnlyList<AcquiredCard>> AcquiredCards { get; }

    /// <summary>
    /// 研究開始イベント
    /// </summary>
    public event Action? ResearchStarted;

    /// <summary>
    /// 研究による強化値の変化イベント（AutoValue を購読しないプール運用ノード向け）
    /// </summary>
    public event Action? ResearchChanged;

    /// <summary>
    /// スクラップが足りれば消費して研究を開始する
    /// </summary>
    /// <returns>開始できたか</returns>
    public bool TryStartResearch();

    /// <summary>
    /// 選択した研究カードの効果を適用する
    /// </summary>
    public void ApplyResearchCard(ResearchCard card);

    /// <summary>
    /// SPスキルのダメージ（研究効果込み）を計算する
    /// </summary>
    public float CalcSpSkillDamage(float baseDamage);

    /// <summary>
    /// 敵の攻撃力倍率（SPスキルのデバフ）
    /// </summary>
    public float EnemyAtkMultiplier { get; }

    /// <summary>
    /// 敵の発射間隔倍率（SPスキルのデバフ）
    /// </summary>
    public float EnemyFireIntervalMultiplier { get; }

    /// <summary>
    /// 敵へのデバフを設定する（解除時は 1, 1 を渡す）
    /// </summary>
    public void SetEnemyDebuff(float atkMultiplier, float fireIntervalMultiplier);

    /// <summary>
    /// 敵へのデバフの変化イベント（プール運用される敵武器向け）
    /// </summary>
    public event Action? EnemyDebuffChanged;

    /// <summary>
    /// 弾丸がヒットした際に呼び出されるイベント
    /// </summary>
    public event Action<IBaseBullet>? BulletHittingStarted;

    /// <summary>
    /// 弾丸がヒットし終わった際に呼び出されるイベント
    /// </summary>
    public event Action<IBaseBullet>? BulletHittingCompleted;

    /// <summary>
    /// 敵が破壊された際に呼び出されるイベント
    /// </summary>
    public event Action<IBaseEnemy> EnemyDestroyed;

    /// <summary>
    /// 弾丸が破壊された際に呼び出されるイベント
    /// </summary>
    public event Action<IBaseBullet> BulletDestroyed;

    /// <summary>
    /// 船HP変化イベント
    /// </summary>
    public event Action<float, float>? ShipHpChanged;

    /// <summary>
    /// ゲームオーバーイベント
    /// </summary>
    public event Action? GameOverOccurred;

    /// <summary>
    /// 船回復要求イベント
    /// </summary>
    public event Action<float>? ShipHealRequested;

    /// <summary>
    /// SPパーセント（0〜100）
    /// </summary>
    public float SpPercent { get; }

    /// <summary>
    /// SPパーセント変化イベント
    /// </summary>
    public event Action<float>? SpPercentChanged;

    /// <summary>
    /// SP発動イベント
    /// </summary>
    public event Action? SPActivated;

    /// <summary>
    /// SPを発動する（SP100%時のみ有効）
    /// </summary>
    public void ActivateSP();

    /// <summary>
    /// バトル開始イベント
    /// </summary>
    public event Action? BattleStarted;

    /// <summary>
    /// バトル開始を通知する
    /// </summary>
    public void NotifyBattleStarted();

    /// <summary>
    /// スコアカウントアップ
    /// </summary>
    public void ScoreCountUp(int score);

    /// <summary>
    /// エネミー撃破数カウントアップ
    /// </summary>
    public void NumEnemyDestroyedCountUp();

    /// <summary>
    /// スクラップを加算する
    /// </summary>
    public void AddScrap(int amount);

    /// <summary>
    /// 敵が倒されたことをバトルに通知する
    /// </summary>
    public void OnEnemyDestroyed(IBaseEnemy baseEnemy);

    /// <summary>
    /// 弾丸が当たったことをバトルに通知する
    /// </summary>
    /// <param name="baseBullet"></param>
    public void StartBulletHitting(IBaseBullet baseBullet);

    /// <summary>
    /// 弾丸が破壊されたことをバトルに通知する
    /// </summary>
    public void OnBulletDestroyed(IBaseBullet baseBullet);

    /// <summary>
    /// 倒された敵の数をバトルに通知する
    /// </summary>
    /// <param name="numEnemyDestroyed"></param>
    public void SetNumEnemyDestroyed(int numEnemyDestroyed);

    /// <summary>
    /// 船HP変化をバトルUIに通知する
    /// </summary>
    public void NotifyShipHpChanged(float currentHp, float maxHp);

    /// <summary>
    /// ゲームオーバーを通知する
    /// </summary>
    public void NotifyGameOver();

    /// <summary>
    /// 船回復を要求する
    /// </summary>
    public void RequestShipHeal(float amount);

    /// <summary>
    /// クルーによる船体の修理を要求する（研究の回復強化は乗らない）
    /// </summary>
    public void RequestShipRepair(float amount);

    /// <summary>
    /// 敵耐久値減少処理
    /// </summary>
    public float ReduceEnemyDurability(float curDurability, float bulletAtk, float bulletDef, float enemyDef);

    /// <summary>
    /// 弾丸耐久値減少処理
    /// </summary>
    /// <param name="curDurability">現在耐久値</param>
    /// <param name="damage">ダメージ</param>
    /// <returns></returns>
    public float ReduceBulletDurability(float curDurability, float damage);
}

/// <summary>
/// バトルレポジトリクラス
/// </summary>
public class BattleRepo : IBattleRepo
{
    /// <summary>
    /// 毒ダメージ
    /// </summary>
    public float PoisonDamage { get; set; } = 2.5f;

    public float ShipAtkBonus { get; set; } = 0.0f;
    public float ShipSpdMultiplier { get; set; } = 1.0f;
    public float ShipDefBonus { get; set; } = 0.0f;

    public float WeaponAtkMultiplier { get; set; } = 1.0f;

    private float _weaponSpdMultiplier = 1.0f;
    public float WeaponSpdMultiplier
    {
        get => _weaponSpdMultiplier;
        set { _weaponSpdMultiplier = value; WeaponSpdMultiplierChanged?.Invoke(); }
    }
    public event Action? WeaponSpdMultiplierChanged;

    public float BulletAtkMultiplier { get; set; } = 1.0f;
    public float BulletSpdMultiplier { get; set; } = 1.0f;
    public float BulletDefBonus { get; set; } = 0.0f;

    private float _spPercent = 0f;
    public float SpPercent => _spPercent;
    public event Action<float>? SpPercentChanged;
    public event Action? SPActivated;

    /// <summary>
    /// スコア
    /// </summary>
    public IAutoValue<int> Score => _score;

    private readonly AutoValue<int> _score;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public IAutoValue<int> NumEnemyDestroyed => _numEnemyDestroyed;

    private readonly AutoValue<int> _numEnemyDestroyed;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public IAutoValue<int> Scrap => _scrap;

    private readonly AutoValue<int> _scrap;

    /// <summary>
    /// 初回の研究コスト
    /// </summary>
    public const int INITIAL_RESEARCH_COST = 10;

    /// <summary>
    /// 研究1回ごとのコスト増加量
    /// </summary>
    public const int RESEARCH_COST_STEP = 5;

    /// <summary>
    /// 撃破1体あたりの基本SP獲得量（%）
    /// </summary>
    public const float BASE_SP_GAIN = 5f;

    /// <summary>
    /// 敵の弾が船に与えるダメージの基本倍率（バランス調整用。SPスキルのデバフとは別に掛かる）
    /// </summary>
    public const float ENEMY_BASE_ATK_MULTIPLIER = 0.5f;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public IAutoValue<int> ResearchCost => _researchCost;

    private readonly AutoValue<int> _researchCost = new(INITIAL_RESEARCH_COST);

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public IAutoValue<ResearchBonus> Research => _research;

    private readonly AutoValue<ResearchBonus> _research = new(new ResearchBonus());

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public IAutoValue<IReadOnlyList<AcquiredCard>> AcquiredCards => _acquiredCards;

    private readonly AutoValue<IReadOnlyList<AcquiredCard>> _acquiredCards = new(Array.Empty<AcquiredCard>());

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event Action? ResearchStarted;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event Action? ResearchChanged;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event Action<IBaseBullet>? BulletHittingStarted;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event Action<IBaseBullet>? BulletHittingCompleted;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event Action<IBaseEnemy>? EnemyDestroyed;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event Action<IBaseBullet>? BulletDestroyed;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event Action<float, float>? ShipHpChanged;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event Action? GameOverOccurred;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event Action<float>? ShipHealRequested;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event Action? BattleStarted;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void NotifyBattleStarted() => BattleStarted?.Invoke();

    private bool _disposedValue;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public BattleRepo()
    {
        _numEnemyDestroyed = new AutoValue<int>(0);
        _score = new AutoValue<int>(0);
        _scrap = new AutoValue<int>(0);
    }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    internal BattleRepo(
      AutoValue<int> numEnemyDestroyed,
      AutoValue<int> score
    )
    {
        _numEnemyDestroyed = numEnemyDestroyed;
        _score = score;
        _scrap = new AutoValue<int>(0);
    }

    /// <summary>
    /// <inheritdoc>
    /// </summary>
    public void ScoreCountUp(int score)
    {
        _score.Value = _score.Value + score;
    }

    /// <summary>
    /// <inheritdoc>
    /// </summary>
    public void NumEnemyDestroyedCountUp()
    {
        _numEnemyDestroyed.Value = _numEnemyDestroyed.Value + 1;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void AddScrap(int amount)
    {
        _scrap.Value = _scrap.Value + amount;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public bool TryStartResearch()
    {
        int cost = _researchCost.Value;
        if (_scrap.Value < cost)
        {
            return false;
        }
        _scrap.Value = _scrap.Value - cost;
        _researchCost.Value = cost + RESEARCH_COST_STEP;
        ResearchStarted?.Invoke();
        return true;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void ApplyResearchCard(ResearchCard card)
    {
        // 同じ種類・レアリティは回数を加算し、新しいリストとして公開する（AutoValue の変化検知のため）
        List<AcquiredCard> list = _acquiredCards.Value.ToList();
        int index = list.FindIndex(c => c.Type == card.Type && c.Rarity == card.Rarity);
        if (index >= 0)
        {
            list[index] = list[index] with { Count = list[index].Count + 1 };
        }
        else
        {
            list.Add(new AcquiredCard(card.Type, card.Rarity, 1));
        }
        _acquiredCards.Value = list;
        _research.Value = _research.Value.Apply(card);
        ResearchChanged?.Invoke();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public float CalcSpSkillDamage(float baseDamage) =>
        baseDamage * (1f + _research.Value.SpDamageRate);

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public float EnemyAtkMultiplier { get; private set; } = 1f;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public float EnemyFireIntervalMultiplier { get; private set; } = 1f;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event Action? EnemyDebuffChanged;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void SetEnemyDebuff(float atkMultiplier, float fireIntervalMultiplier)
    {
        EnemyAtkMultiplier = atkMultiplier;
        EnemyFireIntervalMultiplier = fireIntervalMultiplier;
        EnemyDebuffChanged?.Invoke();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void StartBulletHitting(IBaseBullet baseBullet)
    {
        BulletHittingStarted?.Invoke(baseBullet);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void SetNumEnemyDestroyed(int numEnemyDestroyed)
    {
        _numEnemyDestroyed.Value = numEnemyDestroyed;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void OnEnemyDestroyed(IBaseEnemy baseEnemy)
    {
        _spPercent = Math.Min(100f, _spPercent + BASE_SP_GAIN * (1f + _research.Value.SpGainRate));
        SpPercentChanged?.Invoke(_spPercent);
        EnemyDestroyed?.Invoke(baseEnemy);
    }

    public void ActivateSP()
    {
        if (_spPercent < 100f) return;
        SPActivated?.Invoke();
        _spPercent = 0f;
        SpPercentChanged?.Invoke(_spPercent);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void OnBulletDestroyed(IBaseBullet baseBullet)
    {
        BulletDestroyed?.Invoke(baseBullet);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void NotifyShipHpChanged(float currentHp, float maxHp)
    {
        ShipHpChanged?.Invoke(currentHp, maxHp);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void NotifyGameOver()
    {
        GameOverOccurred?.Invoke();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void RequestShipRepair(float amount) => ShipHealRequested?.Invoke(amount);

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void RequestShipHeal(float amount)
    {
        // スキルによる回復だけがここを通るため、研究の回復強化を掛ける
        ShipHealRequested?.Invoke(amount * (1f + _research.Value.HealRate));
    }

    public float ReduceEnemyDurability(float curDurability, float bulletAtk, float bulletDef, float enemyDef)
    {
        float baseDamage = (bulletAtk * BulletAtkMultiplier) * WeaponAtkMultiplier;
        float shipCorrected = baseDamage * (1f + ShipAtkBonus) * (1f + _research.Value.AtkRate);
        float effectiveDef = Math.Max(0f, enemyDef - (bulletDef + BulletDefBonus));
        float finalDamage = shipCorrected * (100f / (100f + effectiveDef));
        return curDurability - finalDamage;
    }

    public float ReduceBulletDurability(float curDurability, float damage)
    {
        return curDurability -= damage;
    }

    #region Internals

    protected void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _score.Dispose();
                _numEnemyDestroyed.Dispose();
                _scrap.Dispose();
                _researchCost.Dispose();
                _research.Dispose();
                _acquiredCards.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion Internals
}
