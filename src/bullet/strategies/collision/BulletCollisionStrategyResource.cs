namespace EternalJourney.Bullet.Strategies.Collision;

using Godot;

/// <summary>
/// 弾丸衝突戦略リソース基底（パラメータ保持＋生成のみ。実行時オブジェクトは毎回新規生成する）
/// </summary>
[GlobalClass]
public abstract partial class BulletCollisionStrategyResource : Resource
{
    public abstract IBulletCollisionStrategy CreateStrategy();
}
