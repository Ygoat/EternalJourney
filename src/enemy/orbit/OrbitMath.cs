namespace EternalJourney.Enemy.Orbit;

using Godot;

/// <summary>
/// 円形移動する敵の軌道計算（純粋関数）
/// </summary>
public static class OrbitMath
{
    /// <summary>
    /// 中心へ向かって一直線に近づく1フレーム分の移動量（中心を通り過ぎない）
    /// </summary>
    /// <param name="position">現在位置</param>
    /// <param name="center">中心位置</param>
    /// <param name="speed">速さ（px/秒）</param>
    /// <param name="delta">経過時間（秒）</param>
    public static Vector2 ApproachStep(Vector2 position, Vector2 center, float speed, float delta)
    {
        Vector2 toCenter = center - position;
        float step = speed * delta;
        return toCenter.Length() <= step ? toCenter : toCenter.Normalized() * step;
    }

    /// <summary>
    /// 中心から見た位置の角度（ラジアン）
    /// </summary>
    public static float AngleOf(Vector2 center, Vector2 position)
    {
        return (position - center).Angle();
    }

    /// <summary>
    /// 中心からの角度と半径で決まる円周上の位置
    /// </summary>
    public static Vector2 OrbitPosition(Vector2 center, float radius, float angle)
    {
        return center + (new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
    }

    /// <summary>
    /// 中心から遠ざかる1フレーム分の移動量。中心と重なっている場合は下方向へ逃げる
    /// </summary>
    public static Vector2 FleeStep(Vector2 position, Vector2 center, float speed, float delta)
    {
        Vector2 away = position - center;
        Vector2 direction = away.LengthSquared() < Mathf.Epsilon ? Vector2.Down : away.Normalized();
        return direction * (speed * delta);
    }
}
