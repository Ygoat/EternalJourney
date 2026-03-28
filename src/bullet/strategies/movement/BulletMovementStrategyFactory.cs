namespace EternalJourney.Bullet.Strategies.Movement;

using System;
using System.Collections.Generic;

/// <summary>
/// 弾丸移動戦略ファクトリ
/// </summary>
public static class BulletMovementStrategyFactory
{
    private static readonly Dictionary<string, Func<IBulletMovementStrategy>> _strategies = new()
    {
        { "linear", () => new LinearBulletMovement() },
    };

    /// <summary>
    /// 移動戦略を生成
    /// </summary>
    /// <param name="type">移動タイプ</param>
    /// <returns>移動戦略</returns>
    public static IBulletMovementStrategy Create(string type)
    {
        if (_strategies.TryGetValue(type, out var factory))
        {
            return factory();
        }
        return new LinearBulletMovement();
    }
}
