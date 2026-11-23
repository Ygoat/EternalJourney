namespace EternalJourney.ResearchUI.State;

using Chickensoft.GoDotTest;
using EternalJourney.Cores.Models.Research;
using Godot;
using Shouldly;

public class ResearchUILogicCardViewTest : TestClass
{
    public ResearchUILogicCardViewTest(Node testScene) : base(testScene) { }

    [Test]
    public void ToViewsBuildsCardText()
    {
        var views = ResearchUILogic.ToViews(new[]
        {
            new ResearchCard(ResearchCardType.AtkUp, ResearchRarity.Rare),
            new ResearchCard(ResearchCardType.ShipDefUp, ResearchRarity.Legendary),
        });

        views.Count.ShouldBe(2);
        views[0].Text.ShouldBe("WEAPON\n\n攻撃力増強\n\n全ダメージ +20%\n\nRare");
        views[0].Rarity.ShouldBe(ResearchRarity.Rare);
        views[1].Text.ShouldStartWith("SHIP\n\n");
        views[1].Rarity.ShouldBe(ResearchRarity.Legendary);
    }
}
