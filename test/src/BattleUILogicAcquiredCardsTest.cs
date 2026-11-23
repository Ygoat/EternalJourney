namespace EternalJourney.BattleUI.State;

using System.Collections.Generic;
using Chickensoft.GoDotTest;
using EternalJourney.Battle.Domain;
using EternalJourney.Cores.Models.Research;
using Godot;
using Shouldly;

public class BattleUILogicAcquiredCardsTest : TestClass
{
    public BattleUILogicAcquiredCardsTest(Node testScene) : base(testScene) { }

    [Test]
    public void ToRowsFormatsNameRarityAndCount()
    {
        using var repo = new BattleRepo();
        var card = new ResearchCard(ResearchCardType.AtkUp, ResearchRarity.Rare);
        repo.ApplyResearchCard(card);
        repo.ApplyResearchCard(card);

        IReadOnlyList<BattleUILogic.AcquiredCardRow> rows = BattleUILogic.ToRows(repo.AcquiredCards.Value);

        rows.Count.ShouldBe(1);
        rows[0].Text.ShouldBe("[R] 攻撃力増強 ×2");
        rows[0].Rarity.ShouldBe(ResearchRarity.Rare);
    }

    [Test]
    public void ToRowsReturnsEmptyForNoCards()
    {
        BattleUILogic.ToRows(System.Array.Empty<AcquiredCard>()).Count.ShouldBe(0);
    }
}
