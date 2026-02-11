namespace EternalJourney.Bullet.Strategies.Collision;

using System;
using System.Collections.Generic;

/// <summary>
/// 弾丸衝突戦略ファクトリ
/// </summary>
public static class BulletCollisionStrategyFactory
{
    private static readonly Dictionary<string, Func<IBulletCollisionStrategy>> _strategies = new()
    {
        { "normal", () => new NormalCollisionStrategy() },
        { "explosion", () => new ExplosionCollisionStrategy() },
    };

    /// <summary>
    /// 衝突戦略を生成
    /// </summary>
    /// <param name="type">衝突タイプ</param>
    /// <returns>衝突戦略</returns>
    public static IBulletCollisionStrategy Create(string type)
    {
        if (_strategies.TryGetValue(type, out var factory))
        {
            return factory();
        }
        return new NormalCollisionStrategy();
    }
}
