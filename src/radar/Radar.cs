namespace EternalJourney.Radar;

using System.Collections.Generic;
using System.Linq;
using Chickensoft.AutoInject;
using Chickensoft.LogicBlocks;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Cores.Consts;
using EternalJourney.Radar.State;
using Godot;

/// <summary>
/// レーダーインターフェース
/// </summary>
public interface IRadar : IArea2D
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

    /// <summary>
    /// 検知対象のコリジョンマスク
    /// </summary>
    public uint TargetMask { get; set; }

    /// <summary>
    /// 索敵の有効・無効を切り替える（無効の間は敵を検知しない）
    /// </summary>
    public void SetActive(bool active);
};

/// <summary>
/// レーダークラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class Radar : Area2D, IRadar
{
    public override void _Notification(int what) => this.Notify(what);

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
    /// 検知対象のコリジョンマスク（デフォルト：Enemy）
    /// </summary>
    [Export]
    public uint TargetMask { get; set; } = CollisionEntity.Enemy;

    /// <summary>
    /// レーダーロジック
    /// </summary>
    public IRadarLogic RadarLogic { get; set; } = default!;

    /// <summary>
    /// レーダーロジックバインド
    /// </summary>
    public LogicBlock.Binding RadarLogicBinding { get; set; } = default!;

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
        CollisionLayer = CollisionEntity.Radar;
        // サーチエリアコリジョンマスク
        CollisionMask = TargetMask;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void OnResolved()
    {

        RadarLogicBinding.OnOutput((in RadarLogic.Output.StatusChanged output) =>
        {
        });
        RadarLogicBinding.OnState<RadarState.Idle>(_ =>
        {
            AreaEntered += OnAreaEntered;
            EmitSignal(SignalName.NotSearched);
            SetPhysicsProcess(false);
        });
        // Monitor a specific type of state:
        RadarLogicBinding.OnState<RadarState.EnemySearched>(_ =>
        {
            AreaEntered -= OnAreaEntered;
            EmitSignal(SignalName.Searched);
            SetPhysicsProcess(true);
        });
        RadarLogic.Start<RadarState.Idle>();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="delta"></param>
    public void OnPhysicsProcess(double delta)
    {
        // 無効化されている間は何も検知していない扱いにする（監視オフ中は重なりを取得できない）
        if (!Monitoring)
        {
            OnAreaEnemies = new List<Area2D>();
            RadarLogic.Input(new RadarLogic.Input.PhysicProcess(new List<Node2D>()));
            return;
        }

        OnAreaEnemies = GetOverlappingAreas()
            .Where(e => (e.CollisionLayer & TargetMask) != 0)
            .OrderBy(enemy => GlobalPosition.DistanceTo(enemy.GlobalPosition))
            .ToList();
        // ロジックブロックにオーバーラップしている敵のArea2Dノードリストを入力
        RadarLogic.Input(new RadarLogic.Input.PhysicProcess(GetOverlappingAreas().ToList<Node2D>()));
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void SetActive(bool active)
    {
        // 物理クエリ中に呼ばれる可能性があるため遅延設定する
        SetDeferred(Area2D.PropertyName.Monitoring, active);
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
