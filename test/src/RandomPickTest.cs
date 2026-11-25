namespace EternalJourney.Cores.Utils;

using System;
using System.Linq;
using Chickensoft.GoDotTest;
using Godot;
using Shouldly;

public class RandomPickTest : TestClass
{
    public RandomPickTest(Node testScene) : base(testScene) { }

    [Test]
    public void DistinctReturnsUniqueIndicesInRange()
    {
        var rng = new Random(5);
        for (int n = 0; n < 200; n++)
        {
            var picked = RandomPick.Distinct(rng, 8, 4);
            picked.Count.ShouldBe(4);
            picked.Distinct().Count().ShouldBe(4);
            picked.All(i => i >= 0 && i < 8).ShouldBeTrue();
        }
    }

    [Test]
    public void DistinctRejectsTooMany()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => RandomPick.Distinct(new Random(1), 3, 4));
    }
}
