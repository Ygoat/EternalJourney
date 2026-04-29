namespace EternalJourney.Battle.Domain;

using System;
using Chickensoft.Collections;
using EternalJourney.Bullet.Abstract.Base;
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

    /// <summary>
    /// ATK乗数（スキルバフ用）
    /// </summary>
    public float AtkMultiplier { get; set; }

    /// <summary>
    /// SPD乗数（スキルバフ用）
    /// </summary>
    public float SpdMultiplier { get; set; }

    /// <summary>
    /// スコア
    /// </summary>
    public IAutoProp<int> Score { get; }

    /// <summary>
    /// エネミー撃破数
    /// </summary>
    public IAutoProp<int> NumEnemyDestroyed { get; }

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
    /// スコアカウントアップ
    /// </summary>
    public void ScoreCountUp(int score);

    /// <summary>
    /// エネミー撃破数カウントアップ
    /// </summary>
    public void NumEnemyDestroyedCountUp();

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
    /// 敵耐久値減少処理
    /// </summary>
    /// <param name="curDurability">現在耐久値</param>
    /// <param name="damage">ダメージ</param>
    /// <returns></returns>
    public float ReduceEnemyDurability(float curDurability, float damage);

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

    /// <summary>
    /// ATK乗数（スキルバフ用）
    /// </summary>
    public float AtkMultiplier { get; set; } = 1.0f;

    /// <summary>
    /// SPD乗数（スキルバフ用）
    /// </summary>
    public float SpdMultiplier { get; set; } = 1.0f;

    /// <summary>
    /// スコア
    /// </summary>
    public IAutoProp<int> Score => _score;

    private readonly AutoProp<int> _score;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public IAutoProp<int> NumEnemyDestroyed => _numEnemyDestroyed;

    private readonly AutoProp<int> _numEnemyDestroyed;

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

    private bool _disposedValue;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public BattleRepo()
    {
        _numEnemyDestroyed = new AutoProp<int>(0);
        _score = new AutoProp<int>(0);
    }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    internal BattleRepo(
      AutoProp<int> numEnemyDestroyed,
      AutoProp<int> score
    )
    {
        _numEnemyDestroyed = numEnemyDestroyed;
        _score = score;
    }

    /// <summary>
    /// <inheritdoc>
    /// </summary>
    public void ScoreCountUp(int score)
    {
        _score.OnNext(_score.Value + score);
    }

    /// <summary>
    /// <inheritdoc>
    /// </summary>
    public void NumEnemyDestroyedCountUp()
    {
        _numEnemyDestroyed.OnNext(_numEnemyDestroyed.Value + 1);
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
        _numEnemyDestroyed.OnNext(numEnemyDestroyed);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void OnEnemyDestroyed(IBaseEnemy baseEnemy)
    {
        EnemyDestroyed?.Invoke(baseEnemy);
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

    public float ReduceEnemyDurability(float curDurability, float damage)
    {
        return curDurability - damage * AtkMultiplier;
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
                _numEnemyDestroyed.OnCompleted();
                _numEnemyDestroyed.Dispose();
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
