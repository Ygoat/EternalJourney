namespace EternalJourney.Crew;

using System;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Battle.Domain;
using EternalJourney.Cores.Models.Crews;
using EternalJourney.Crew.Domain;
using EternalJourney.Crew.State;
using Godot;

/// <summary>
/// 作業クルーインターフェース
/// </summary>
public interface ICrewMember : INode2D
{
    /// <summary>
    /// 配置先を指示する（移動してから着任する）
    /// </summary>
    void Assign(int station);

    /// <summary>
    /// 配置先へ直接置く
    /// </summary>
    void Place(int station);
}

/// <summary>
/// 作業クルークラス（駒の描画と移動のみ。行動は CrewMemberLogic が決める）
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class CrewMember : Node2D, ICrewMember
{
    public override void _Notification(int what) => this.Notify(what);

    #region State
    public ICrewMemberLogic Logic { get; set; } = default!;
    public LogicBlock.Binding Binding { get; set; } = default!;
    #endregion State

    #region Exports
    /// <summary>
    /// 移動速度（親ノードのローカル座標/秒）
    /// </summary>
    [Export]
    public float MoveSpeed { get; set; } = 150f;

    /// <summary>
    /// 駒の半径
    /// </summary>
    [Export]
    public float Radius { get; set; } = 11f;

    /// <summary>
    /// 頭文字の文字サイズ
    /// </summary>
    [Export]
    public int FontSize { get; set; } = 14;
    #endregion Exports

    #region Dependencies
    [Dependency]
    public IBattleRepo BattleRepo => this.DependOn<IBattleRepo>();

    [Dependency]
    public ICrewRepo CrewRepo => this.DependOn<ICrewRepo>();
    #endregion Dependencies

    /// <summary>
    /// このクルーの定義（生成時に設定する）
    /// </summary>
    public CrewDefinition Definition { get; set; } = CrewCatalog.DefaultWorkers[0];

    /// <summary>
    /// 配置先の位置（親ノードのローカル座標）を返す関数（生成時に設定する）
    /// </summary>
    public Func<int, Vector2> StationPosition { get; set; } = _ => Vector2.Zero;

    /// <summary>
    /// 駒の色（個性ごと）
    /// </summary>
    public Color BodyColor { get; set; } = Colors.White;

    /// <summary>
    /// 最初に置く配置先（生成時に設定する）
    /// </summary>
    public int InitialStation { get; set; } = CrewCommander.REPAIR;

    private Vector2 _destination;

    public void Setup()
    {
        Logic = new CrewMemberLogic();
        Binding = Logic.Bind();
        SetProcess(false);
    }

    public void OnResolved()
    {
        Logic.Set(BattleRepo);
        Logic.Set(CrewRepo);
        Logic.Set(Definition);

        Binding
            .OnOutput((in CrewMemberLogic.Output.MoveTo output) =>
            {
                _destination = StationPosition(output.Station);
                SetProcess(true);
            })
            .OnOutput((in CrewMemberLogic.Output.PlaceAt output) =>
            {
                Position = StationPosition(output.Station);
            })
            .OnOutput((in CrewMemberLogic.Output.Working _) =>
            {
                // 着任後も修理の時間経過を送るため処理は続ける
                SetProcess(true);
            });

        Logic.Start<CrewMemberState.Standby>();
        Place(InitialStation);
        QueueRedraw();
    }

    public void Assign(int station) => Logic.Input(new CrewMemberLogic.Input.Assign(station));

    public void Place(int station) => Logic.Input(new CrewMemberLogic.Input.Place(station));

    public void OnProcess(double delta)
    {
        if (Logic.State is CrewMemberState.Moving)
        {
            Position = Position.MoveToward(_destination, MoveSpeed * (float)delta);
            if (Position.IsEqualApprox(_destination))
            {
                Logic.Input(new CrewMemberLogic.Input.Arrived());
            }
            return;
        }
        Logic.Input(new CrewMemberLogic.Input.Tick(delta));
    }

    public override void _Draw()
    {
        DrawCircle(Vector2.Zero, Radius, BodyColor);
        DrawArc(Vector2.Zero, Radius, 0f, Mathf.Tau, 24, Colors.Black, 2f);
        Font font = ThemeDB.FallbackFont;
        DrawString(font, new Vector2(-Radius, FontSize / 2f - 1f), Definition.Initial,
            HorizontalAlignment.Center, Radius * 2f, FontSize, Colors.Black);
    }

    public void OnExitTree()
    {
        Binding.Dispose();
        ((IDisposable)Logic).Dispose();
    }
}
