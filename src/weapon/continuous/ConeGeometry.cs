namespace EternalJourney.Weapon.Continuous;

using System;
using Godot;

/// <summary>
/// 扇形の頂点計算（表示と当たり判定で共有する純粋関数）
/// </summary>
public static class ConeGeometry
{
    /// <summary>
    /// 原点を要とし、+X方向に開く扇形の頂点列を作る
    /// </summary>
    /// <param name="range">半径</param>
    /// <param name="angleDegrees">扇の開き角度（度）</param>
    /// <param name="segments">円弧の分割数（1以上）</param>
    /// <returns>頂点列（先頭は原点、以降は円弧を時計回りに並べる）</returns>
    public static Vector2[] Build(float range, float angleDegrees, int segments)
    {
        int count = Math.Max(1, segments);
        float half = Mathf.DegToRad(angleDegrees) / 2f;
        float step = Mathf.DegToRad(angleDegrees) / count;

        Vector2[] points = new Vector2[count + 2];
        points[0] = Vector2.Zero;
        for (int i = 0; i <= count; i++)
        {
            float angle = -half + (step * i);
            points[i + 1] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * range;
        }
        return points;
    }
}
