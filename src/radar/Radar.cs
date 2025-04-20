namespace EternalJourney.Radar;

using System.Collections.Generic;
using System.Linq;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Cores.Consts;
using EternalJourney.Radar.State;
using Godot;

/// <summary>
/// レーダーインターフェース
/// </summary>
public interface IRadar : INode2D
{
    /// <summary>
    /// エリア内エネミー
    /// </summary>
    public List<Area2D> OnAreaEnemies { get; set; }

    /// <summary>
    /// 敵発見イベント
    /// </summary>
    public event Radar.SearchedEventHandler Searched;

    /// <summary>
    /// 敵未発見イベント
    /// </summary>
    public event Radar.NotSearchedEventHandler NotSearched;
};

/// <summary>
/// レーダークラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class Radar : Node2D, IRadar
{
    public override void _Notification(int what) => this.Notify(what);

    #region Nodes
    /// <summary>
    /// サーチエリア
    /// </summary>
    [Node("%SearchArea")]
    public IArea2D Area2D { get; set; } = default!;
    #endregion Nodes

    #region Signals
    /// <summary>
    /// 敵発見シグナル
    /// </summary>
    [Signal]
    public delegate void SearchedEventHandler();

    /// <summary>
    /// 敵未発見シグナル
    /// </summary>て
    [Signal]
    public delegate void NotSearchedEventHandler();
    #endregion Signals

    /// <summary>
    /// レーダー内エネミー
    /// </summary>
    public List<Area2D> OnAreaEnemies { get; set; } = new List<Area2D>();

    /// <summary>
    /// レーダーロジック
    /// </summary>
    public IRadarLogic RadarLogic { get; set; } = default!;

    /// <summary>
    /// レーダーロジックバインド
    /// </summary>
    public RadarLogic.IBinding RadarLogicBinding { get; set; } = default!;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Setup()
    {
        // レーダーロジックインスタンス化
        RadarLogic = new RadarLogic();
        // レーダーロジックバインド
        RadarLogicBinding = RadarLogic.Bind();
        // サーチエリアコリジョンレイヤ
        Area2D.CollisionLayer = CollisionEntity.Radar;
        // サーチエリアコリジョンマスク
        Area2D.CollisionMask = CollisionEntity.Enemy;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void OnResolved()
    {

        RadarLogicBinding.Handle((in RadarLogic.Output.StatusChanged output) =>
        {
        });
        RadarLogicBinding.When((RadarLogic.State.Idle _) =>
        {
            Area2D.AreaEntered += OnAreaEntered;
            EmitSignal(SignalName.NotSearched);
            SetPhysicsProcess(false);
        });
        // Monitor a specific type of state:
        RadarLogicBinding.When((RadarLogic.State.EnemySearched _) =>
        {
            Area2D.AreaEntered -= OnAreaEntered;
            EmitSignal(SignalName.Searched);
            SetPhysicsProcess(true);
        });
        RadarLogic.Start();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="delta"></param>
    public void OnPhysicsProcess(double delta)
    {
        OnAreaEnemies = Area2D.GetOverlappingAreas()
            .Where(enemy => enemy.CollisionLayer == CollisionEntity.Enemy)
            .OrderBy(enemy => GlobalPosition.DistanceTo(enemy.GlobalPosition))
            .ToList();
        // ロジックブロックにオーバーラップしている敵のArea2Dノードリストを入力
        RadarLogic.Input(new RadarLogic.Input.PhysicProcess(Area2D.GetOverlappingAreas().ToList<Node2D>()));
    }

    /// <summary>
    /// サーチエリア進入時のイベント
    /// </summary>
    /// <param name="area"></param>
    public void OnAreaEntered(Area2D area)
    {
        // ロジックブロックに進入した敵のArea2Dノードを入力
        RadarLogic.Input(new RadarLogic.Input.WatchEnemy(area));
    }
}
