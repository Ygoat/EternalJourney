namespace EternalJourney.Bullet.Strategies.Movement;

using Godot;

/// <summary>
/// 弾丸移動戦略リソース基底（パラメータ保持＋生成のみ。実行時オブジェクトは毎回新規生成する）
/// </summary>
[GlobalClass]
public abstract partial class BulletMovementStrategyResource : Resource
{
    public abstract IBulletMovementStrategy CreateStrategy();
}
