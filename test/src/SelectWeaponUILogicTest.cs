namespace EternalJourney.SelectUI.State;

using System;
using System.Linq;
using Chickensoft.GoDotTest;
using EternalJourney.Cores.Models.Weapon;
using EternalJourney.Game.Domain;
using Godot;
using Shouldly;

public class SelectWeaponUILogicTest : TestClass
{
    public SelectWeaponUILogicTest(Node testScene) : base(testScene) { }

    private static SelectWeaponUILogic Create(GameRepo repo, out Func<SelectWeaponUILogic.Output.SelectionChanged> last)
    {
        var logic = new SelectWeaponUILogic();
        logic.Set<IGameRepo>(repo);
        logic.Set(new Random(11));
        SelectWeaponUILogic.Output.SelectionChanged latest = default;
        var binding = logic.Bind();
        binding.OnOutput((in SelectWeaponUILogic.Output.SelectionChanged o) => latest = o);
        logic.Start<SelectWeaponUIState.Idle>();
        last = () => latest;
        return logic;
    }

    [Test]
    public void StartsWithDefaultLoadoutAndSavesChoice()
    {
        using var repo = new GameRepo();
        int notified = 0;
        repo.WeaponSelected += () => notified++;
        var logic = Create(repo, out var last);

        last().Selected.ShouldBe(WeaponCatalog.DefaultLoadout.Select(WeaponCatalog.IndexOf).ToArray());

        int cold = WeaponCatalog.IndexOf(WeaponType.Cold);
        logic.Input(new SelectWeaponUILogic.Input.WeaponChosen(1, cold));
        logic.Input(new SelectWeaponUILogic.Input.WeaponChosen(2, cold));
        last().Selected[1].ShouldBe(cold);
        last().Description.ShouldContain("左下：冷気");

        logic.Input(new SelectWeaponUILogic.Input.ConfirmPressed());
        notified.ShouldBe(1);
        repo.SelectedWeapons.ShouldBe(new[] { WeaponType.Normal, WeaponType.Cold, WeaponType.Cold, WeaponType.Flame });
        logic.Dispose();
    }

    [Test]
    public void RandomFillsEverySlotWithValidWeapon()
    {
        using var repo = new GameRepo();
        var logic = Create(repo, out var last);

        logic.Input(new SelectWeaponUILogic.Input.RandomPressed());

        last().Selected.Count.ShouldBe(WeaponCatalog.SLOT_COUNT);
        last().Selected.All(i => i >= 0 && i < WeaponCatalog.All.Count).ShouldBeTrue();
        logic.Dispose();
    }
}
