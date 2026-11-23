namespace EternalJourney.Weapon.Continuous;

using Chickensoft.GoDotTest;
using Godot;
using Shouldly;

public class ConeGeometryTest : TestClass
{
    public ConeGeometryTest(Node testScene) : base(testScene) { }

    [Test]
    public void BuildReturnsOriginAndArcPoints()
    {
        Vector2[] points = ConeGeometry.Build(100f, 60f, 4);

        // 原点 + 円弧（分割数 + 1 点）
        points.Length.ShouldBe(6);
        points[0].ShouldBe(Vector2.Zero);
    }

    [Test]
    public void BuildPlacesArcPointsOnRadius()
    {
        Vector2[] points = ConeGeometry.Build(100f, 60f, 4);

        for (int i = 1; i < points.Length; i++)
        {
            points[i].Length().ShouldBe(100f, 0.001f);
        }
    }

    [Test]
    public void BuildOpensSymmetricallyAroundXAxis()
    {
        Vector2[] points = ConeGeometry.Build(100f, 60f, 4);

        // 両端は ±(角度/2)
        Mathf.RadToDeg(points[1].Angle()).ShouldBe(-30f, 0.01f);
        Mathf.RadToDeg(points[^1].Angle()).ShouldBe(30f, 0.01f);
    }

    [Test]
    public void BuildTreatsNonPositiveSegmentsAsOne()
    {
        Vector2[] points = ConeGeometry.Build(100f, 60f, 0);

        points.Length.ShouldBe(3);
    }
}
