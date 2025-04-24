namespace EternalJourney.Battle.Domain;

using System;
using Chickensoft.Collections;
using EternalJourney.Bullet.Abstract.Base;
using EternalJourney.Enemy.Abstract.Base;

/// <summary>
/// バトルレポジトリクラス
/// </summary>
public interface IBattleRepo : IDisposable
{
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
    /// 敵が倒されたことをバトルに通知する
    /// </summary>
    public void OnEnemyDestroyed(IBaseEnemy baseEnemy);

    /// <summary>
    /// 敵が弾丸によるダメージを受けた時の処理
    /// </summary>
    /// <param name="baseEnemy"></param>
    /// <param name="baseBullet"></param>
    public void EnemyDamagedByBullet(IBaseEnemy baseEnemy, IBaseBullet baseBullet);

    /// <summary>
    /// 弾丸が敵によるダメージを受けた時の処理
    /// </summary>
    /// <param name="baseBullet"></param>
    /// <param name="baseEnemy"></param>
    public void BulletDamagedByEnemy(IBaseBullet baseBullet, IBaseEnemy baseEnemy);

    /// <summary>
    /// 弾丸が当たったことをバトルに通知する
    /// </summary>
    /// <param name="baseBullet"></param>
    public void StartBulletHitting(IBaseBullet baseBullet);

    /// <summary>
    /// 弾丸が破壊されたことをバトルに通知する
    /// </summary>
    public void OnBulletDestoryed(IBaseBullet baseBullet);

    /// <summary>
    /// 倒された敵の数をバトルに通知する
    /// </summary>
    /// <param name="numEnemyDestoryed"></param>
    public void SetNumEnemyDestoryed(int numEnemyDestoryed);
}

/// <summary>
/// バトルレポジトリクラス
/// </summary>
public class BattleRepo : IBattleRepo
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public IAutoProp<int> NumEnemyDestroyed => _numEnemyDestroyed;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
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

    private bool _disposedValue;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public BattleRepo()
    {
        _numEnemyDestroyed = new AutoProp<int>(0);
    }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="numEnemyDestroyed"></param>
    internal BattleRepo(
      AutoProp<int> numEnemyDestroyed
    )
    {
        _numEnemyDestroyed = numEnemyDestroyed;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="baseBullet"></param>
    public void StartBulletHitting(IBaseBullet baseBullet)
    {
        BulletHittingStarted?.Invoke(baseBullet);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="numEnemyDestoryed"></param>
    public void SetNumEnemyDestoryed(int numEnemyDestoryed)
    {
        _numEnemyDestroyed.OnNext(numEnemyDestoryed);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="baseEnemy"></param>
    public void OnEnemyDestroyed(IBaseEnemy baseEnemy)
    {
        EnemyDestroyed?.Invoke(baseEnemy);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="baseBullet"></param>
    public void OnBulletDestoryed(IBaseBullet baseBullet)
    {
        BulletDestroyed?.Invoke(baseBullet);
    }

    public void EnemyDamagedByBullet(IBaseEnemy baseEnemy, IBaseBullet baseBullet)
    {
        baseEnemy.Status.CurrentDur -= 1.0f;
    }
    public void BulletDamagedByEnemy(IBaseBullet baseBullet, IBaseEnemy baseEnemy)
    {
        baseBullet.Status.CurrentDur -= 1.0f;
    }

    #region Internals

    protected void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // Dispose managed objects.
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
