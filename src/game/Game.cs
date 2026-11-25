namespace EternalJourney.Game;

using Chickensoft.AutoInject;
using Chickensoft.LogicBlocks;
using Chickensoft.Collections;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Battle;
using EternalJourney.Battle.Domain;
using EternalJourney.Cores.Repositories;
using EternalJourney.Game.Domain;
using EternalJourney.Game.State;
using EternalJourney.Result;
using EternalJourney.SelectSkill;
using EternalJourney.SelectUI;
using Godot;


/// <summary>
/// ゲームインターフェース
/// </summary>
public interface IGame : INode2D, IProvide<EntityTable<int>>, IProvide<IBattleRepo>, IProvide<IGameRepo>
{
}

/// <summary>
/// ゲームクラス
/// </summary>
[Meta(typeof(IAutoNode))] // 子Nodeへ親Nodeの値をDIするために必要なミックスイン
public partial class Game : Node2D, IGame
{
    // 子Nodeへ親Nodeの値をDIするために必要
    public override void _Notification(int what) => this.Notify(what);

    public EntityTable<int> EntityTable { get; set; } = new EntityTable<int>();
    EntityTable<int> IProvide<EntityTable<int>>.Value() => EntityTable;

    /// <summary>
    /// IBattleRepo を子ノード配下へ再提供
    /// </summary>
    IBattleRepo IProvide<IBattleRepo>.Value() => Battle.BattleRepo;

    /// <summary>
    /// ゲームリポジトリ
    /// </summary>
    public IGameRepo GameRepo { get; set; } = default!;

    /// <summary>
    /// IGameRepo を子ノード配下へ提供
    /// </summary>
    IGameRepo IProvide<IGameRepo>.Value() => GameRepo;

    /// <summary>
    /// スキル選択
    /// </summary>
    [Node]
    public ISelectSkill SelectSkill { get; set; } = default!;

    /// <summary>
    /// SPスキル選択
    /// </summary>
    [Node]
    public ISelectSpSkillUI SelectSpSkill { get; set; } = default!;

    /// <summary>
    /// 武器選択
    /// </summary>
    [Node]
    public ISelectWeaponUI SelectWeapon { get; set; } = default!;

    /// <summary>
    /// クルー選択
    /// </summary>
    [Node]
    public ISelectCrewUI SelectCrew { get; set; } = default!;

    /// <summary>
    /// バトル
    /// </summary>
    [Node]
    public IBattle Battle { get; set; } = default!;

    /// <summary>
    /// リザルト
    /// </summary>
    [Node]
    public IResult Result { get; set; } = default!;

    /// <summary>
    /// ゲームロジック
    /// </summary>
    public IGameLogic GameLogic { get; set; } = default!;

    /// <summary>
    /// ゲームロジックバインド
    /// </summary>
    public LogicBlock.Binding GameBinding { get; set; } = default!;

    [Dependency]
    private ICrewCsvReader crewCsvReader => this.DependOn<ICrewCsvReader>(() => new CrewCsvReader());

    public void Setup()
    {
        GameRepo = new GameRepo();
        GameLogic = new GameLogic();
        GameLogic.Set(GameRepo);
        GameBinding = GameLogic.Bind();
    }

    public void OnResolved()
    {
        // Provide()を呼び出して依存関係を提供
        this.Provide();

        GameBinding
            .OnOutput((in GameLogic.Output.ShowSelectSkill _) =>
            {
                Battle.ProcessMode = ProcessModeEnum.Disabled;
                SelectSkill.Show();
                SelectSpSkill.Hide();
                SelectWeapon.Hide();
                SelectCrew.Hide();
                Battle.Hide();
            })
            .OnOutput((in GameLogic.Output.ShowSelectSpSkill _) =>
            {
                SelectSkill.Hide();
                SelectSpSkill.Show();
            })
            .OnOutput((in GameLogic.Output.ShowSelectWeapon _) =>
            {
                SelectSpSkill.Hide();
                SelectWeapon.Show();
            })
            .OnOutput((in GameLogic.Output.ShowSelectCrew _) =>
            {
                SelectWeapon.Hide();
                SelectCrew.Show();
            })
            .OnOutput((in GameLogic.Output.InitializeBattle _) =>
            {
                Battle.Initialize();
                GameRepo.NotifyBattleInitialized();
            })
            .OnOutput((in GameLogic.Output.StartBattle _) =>
            {
                Battle.ProcessMode = ProcessModeEnum.Inherit;
                SelectSkill.Hide();
                SelectSpSkill.Hide();
                SelectWeapon.Hide();
                SelectCrew.Hide();
                Battle.Show();
                Battle.StartBattle();
            })
            .OnOutput((in GameLogic.Output.EndBattle _) =>
            {
                Battle.Hide();
                GameRepo.NotifyBattleEnded();
            })
            .OnOutput((in GameLogic.Output.ShowResult _) =>
            {
                Result.Show();
            });

        GameLogic.Start<GameState.SelectSkillPhase>();
    }

    public void OnExitTree()
    {
        GameBinding.Dispose();
        ((System.IDisposable)GameLogic).Dispose();
        GameRepo.Dispose();
    }
}
