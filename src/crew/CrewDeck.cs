namespace EternalJourney.Crew;

using System;
using System.Collections.Generic;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Battle.Domain;
using EternalJourney.Cores.Models.Crews;
using EternalJourney.Crew.Domain;
using EternalJourney.Crew.State;
using EternalJourney.Game.Domain;
using Godot;

/// <summary>
/// クルー配置インターフェース
/// </summary>
public interface ICrewDeck : INode2D
{
}

/// <summary>
/// クルー配置クラス（船内の配置位置を持ち、クルーの駒を生成して指揮官の指示を伝える）
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class CrewDeck : Node2D, ICrewDeck
{
    public override void _Notification(int what) => this.Notify(what);

    #region State
    public ICrewDeckLogic Logic { get; set; } = default!;
    public LogicBlock.Binding Binding { get; set; } = default!;
    #endregion State

    #region Exports
    /// <summary>
    /// 配置を決め直す間隔（秒）
    /// </summary>
    [Export]
    public float CommandInterval { get; set; } = 1f;

    /// <summary>
    /// 臆病なクルーの色
    /// </summary>
    [Export]
    public Color CowardColor { get; set; } = new(0.6f, 0.85f, 1f);

    /// <summary>
    /// 普通のクルーの色
    /// </summary>
    [Export]
    public Color SteadyColor { get; set; } = new(0.85f, 0.85f, 0.85f);

    /// <summary>
    /// 勇敢なクルーの色
    /// </summary>
    [Export]
    public Color BraveColor { get; set; } = new(1f, 0.6f, 0.45f);

    /// <summary>
    /// 指揮官の駒の半径
    /// </summary>
    [Export]
    public float CommanderRadius { get; set; } = 14f;
    #endregion Exports

    #region Nodes
    [Node]
    public IMarker2D Station0 { get; set; } = default!;

    [Node]
    public IMarker2D Station1 { get; set; } = default!;

    [Node]
    public IMarker2D Station2 { get; set; } = default!;

    [Node]
    public IMarker2D Station3 { get; set; } = default!;

    [Node]
    public IMarker2D RepairPoint { get; set; } = default!;

    [Node]
    public IMarker2D Bridge { get; set; } = default!;
    #endregion Nodes

    #region Dependencies
    [Dependency]
    public IBattleRepo BattleRepo => this.DependOn<IBattleRepo>();

    [Dependency]
    public ICrewRepo CrewRepo => this.DependOn<ICrewRepo>();

    [Dependency]
    public IGameRepo GameRepo => this.DependOn<IGameRepo>();
    #endregion Dependencies

    private readonly List<ICrewMember> _members = new();
    private CrewDefinition? _commander;

    public void Setup()
    {
        Logic = new CrewDeckLogic();
        Binding = Logic.Bind();
        SetProcess(false);
    }

    public void OnResolved()
    {
        Logic.Set(BattleRepo);
        Logic.Set(CrewRepo);
        Logic.Set(GameRepo);
        Logic.Set(new CrewDeckLogic.Settings(CommandInterval));

        Binding
            .OnOutput((in CrewDeckLogic.Output.Embark output) => Embark(output.Commander, output.Workers, output.Stations))
            .OnOutput((in CrewDeckLogic.Output.Assign output) =>
            {
                if (output.WorkerIndex >= 0 && output.WorkerIndex < _members.Count)
                {
                    _members[output.WorkerIndex].Assign(output.Station);
                }
            });

        Logic.Start<CrewDeckState.Waiting>();
    }

    public void OnProcess(double delta) => Logic.Input(new CrewDeckLogic.Input.Tick(delta));

    /// <summary>
    /// クルーの駒を生成して配置する
    /// </summary>
    private void Embark(CrewDefinition commander, IReadOnlyList<CrewDefinition> workers, IReadOnlyList<int> stations)
    {
        _commander = commander;
        for (int i = 0; i < workers.Count; i++)
        {
            var member = new CrewMember
            {
                Name = $"Crew{i}",
                Definition = workers[i],
                StationPosition = StationPosition,
                BodyColor = ColorOf(workers[i].Personality),
                InitialStation = stations[i],
            };
            AddChild(member);
            _members.Add(member);
        }
        SetProcess(true);
        QueueRedraw();
    }

    /// <summary>
    /// 配置先の位置（このノードのローカル座標）
    /// </summary>
    private Vector2 StationPosition(int station) => station switch
    {
        0 => Station0.Position,
        1 => Station1.Position,
        2 => Station2.Position,
        3 => Station3.Position,
        _ => RepairPoint.Position,
    };

    private Color ColorOf(CrewPersonality personality) => personality switch
    {
        CrewPersonality.Coward => CowardColor,
        CrewPersonality.Brave => BraveColor,
        _ => SteadyColor,
    };

    public override void _Draw()
    {
        if (_commander == null)
        {
            return;
        }
        // 指揮官はブリッジに固定表示する
        Vector2 center = Bridge.Position;
        DrawCircle(center, CommanderRadius, ColorOf(_commander.Personality));
        DrawArc(center, CommanderRadius, 0f, Mathf.Tau, 24, Colors.Gold, 3f);
        Font font = ThemeDB.FallbackFont;
        DrawString(font, center + new Vector2(-CommanderRadius, 6f), _commander.Initial,
            HorizontalAlignment.Center, CommanderRadius * 2f, 16, Colors.Black);
    }

    public void OnExitTree()
    {
        Binding.Dispose();
        ((IDisposable)Logic).Dispose();
    }
}
