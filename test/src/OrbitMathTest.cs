namespace EternalJourney.Enemy.Orbit;

using Chickensoft.GoDotTest;
using Godot;
using Shouldly;

public class OrbitMathTest : TestClass
{
    public OrbitMathTest(Node testScene) : base(testScene) { }

    [Test]
    public void ApproachStepMovesTowardCenterBySpeedTimesDelta()
    {
        Vector2 step = OrbitMath.ApproachStep(new Vector2(0f, 0f), new Vector2(100f, 0f), 60f, 0.5f);

        step.X.ShouldBe(30f, 0.001f);
        step.Y.ShouldBe(0f, 0.001f);
    }

    [Test]
    public void ApproachStepDoesNotOvershootCenter()
    {
        Vector2 step = OrbitMath.ApproachStep(new Vector2(0f, 0f), new Vector2(10f, 0f), 1000f, 1f);

        step.X.ShouldBe(10f, 0.001f);
    }

    [Test]
    public void OrbitPositionLiesOnCircle()
    {
        Vector2 center = new(357f, 637f);
        Vector2 position = OrbitMath.OrbitPosition(center, 300f, Mathf.Pi / 3f);

        position.DistanceTo(center).ShouldBe(300f, 0.01f);
    }

    [Test]
    public void AngleOfIsInverseOfOrbitPosition()
    {
        Vector2 center = new(357f, 637f);
        Vector2 position = OrbitMath.OrbitPosition(center, 300f, 1.2f);

        OrbitMath.AngleOf(center, position).ShouldBe(1.2f, 0.0001f);
    }

    [Test]
    public void FleeStepMovesAwayFromCenter()
    {
        Vector2 step = OrbitMath.FleeStep(new Vector2(0f, -100f), Vector2.Zero, 200f, 0.5f);

        step.X.ShouldBe(0f, 0.001f);
        step.Y.ShouldBe(-100f, 0.001f);
    }

    [Test]
    public void FleeStepGoesDownWhenOverlappingCenter()
    {
        Vector2 step = OrbitMath.FleeStep(Vector2.Zero, Vector2.Zero, 100f, 1f);

        step.Y.ShouldBe(100f, 0.001f);
    }
}
