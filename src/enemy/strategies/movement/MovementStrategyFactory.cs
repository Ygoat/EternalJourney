namespace EternalJourney.Enemy.Strategies.Movement;

using System;
using System.Collections.Generic;

/// <summary>
/// 移動戦略ファクトリ
/// </summary>
public static class MovementStrategyFactory
{
    private static readonly Dictionary<string, Func<IMovementStrategy>> _strategies = new()
    {
        { "linear", () => new LinearMovementStrategy() },
        { "sine_wave", () => new SineWaveMovementStrategy() },
        { "homing", () => new HomingMovementStrategy() },
        { "stop_and_go", () => new StopAndGoMovementStrategy() }
    };

    /// <summary>
    /// 移動戦略を生成
    /// </summary>
    /// <param name="type">移動タイプ</param>
    /// <returns>移動戦略</returns>
    public static IMovementStrategy Create(string type)
    {
        if (_strategies.TryGetValue(type, out var factory))
        {
            return factory();
        }
        return new LinearMovementStrategy();
    }
}
