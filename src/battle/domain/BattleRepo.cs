namespace EternalJourney.Battle.Domain;

using System;
using Chickensoft.Collections;
using EternalJourney.Bullet.Abstract.Base;
using EternalJourney.Enemy.Abstract.Base;
using Godot;

/// <summary>
/// バトルレポジトリクラス
/// </summary>
public interface IBattleRepo : IDisposable
{
    /// <summary>
    /// 弾丸がヒットした際に呼び出されるイベント
    /// </summary>
    event Action<IBaseBullet>? BulletHittingStarted;

    /// <summary>
    /// 弾丸がヒットし終わった際に呼び出されるイベント
    /// </summary>
    event Action<IBaseBullet>? BulletHittingCompleted;

    /// <summary>
    /// 敵が破壊された際に呼び出されるイベント
    /// </summary>
    event Action<IBaseEnemy> EnemyDestroyed;

    /// <summary>
    /// 弾丸が破壊された際に呼び出されるイベント
    /// </summary>
    event Action<IBaseBullet> BulletDestroyed;

    /// <summary>
    /// 敵が倒されたことをバトルに通知する
    /// </summary>
    void OnEnemyDestroyed(IBaseEnemy BaseEnemy);

    /// <summary>
    /// 弾丸が当たったことをバトルに通知する
    /// </summary>
    /// <param name="BaseBullet"></param>
    void StartBulletHitting(IBaseBullet BaseBullet);

    /// <summary>
    /// 弾丸が破壊されたことをバトルに通知する
    /// </summary>
    void OnBulletDestoryed(IBaseBullet BaseBullet);

    /// <summary>
    /// 倒された敵の数をバトルに通知する
    /// </summary>
    /// <param name="numEnemyDestoryed"></param>
    void SetNumEnemyDestoryed(int numEnemyDestoryed);
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
    /// <param name="BaseBullet"></param>
    public void StartBulletHitting(IBaseBullet BaseBullet)
    {
        BulletHittingStarted?.Invoke(BaseBullet);
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
    /// <param name="BaseEnemy"></param>
    public void OnEnemyDestroyed(IBaseEnemy BaseEnemy)
    {
        EnemyDestroyed?.Invoke(BaseEnemy);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="BaseBullet"></param>
    public void OnBulletDestoryed(IBaseBullet BaseBullet)
    {
        BulletDestroyed?.Invoke(BaseBullet);
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
