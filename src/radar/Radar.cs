namespace EternalJourney.Radar;

using System.Collections.Generic;
using System.Linq;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Cores.Consts;
using EternalJourney.Enemy;
using EternalJourney.Radar.State;
using Godot;

public interface IRadar : INode2D
{
    public List<Area2D> OnAreaEnemies { get; set; }
};

[Meta(typeof(IAutoNode))]
public partial class Radar : Node2D, IRadar
{
    public override void _Notification(int what) => this.Notify(what);

    public List<Area2D> OnAreaEnemies { get; set; } = new List<Area2D>();

    [Node("%SearchArea")]
    public IArea2D Area2D { get; set; } = default!;

    public IRadarLogic RadarLogic { get; set; } = default!;

    public RadarLogic.IBinding RadarLogicBinding { get; set; } = default!;

    public void Setup()
    {
        RadarLogic = new RadarLogic();
        RadarLogicBinding = RadarLogic.Bind();
        Area2D.CollisionLayer = CollisionEntity.Radar;
        Area2D.CollisionMask = CollisionEntity.Enemy;
    }

    public void OnResolved()
    {
        GD.Print("Ready!");
        // Monitor an output:
        RadarLogicBinding.Handle((in RadarLogic.Output.StatusChanged output) => GD.Print("Changed"));
        // Monitor an input:
        // RadarLogicBinding.Watch((in RadarLogic.Input.PhysicProcess input) => GD.Print("Entered"));

        RadarLogicBinding.When((RadarLogic.State.Idle _) =>
        {
            Area2D.AreaEntered += OnAreaEntered;
            // Idle状態の時はProcessが実行されない
            SetPhysicsProcess(false);
            GD.Print("Idle");
        });
        // Monitor a specific type of state:
        RadarLogicBinding.When((RadarLogic.State.EnemySearched _) =>
        {
            Area2D.AreaEntered -= OnAreaEntered;
            // Idle状態の時はProcessを実行する
            SetPhysicsProcess(true);
            GD.Print("Searched");
        });
        RadarLogic.Start();
    }

    /// <summary>
    /// 物理更新処理
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
        GD.Print("Entered!");
        // ロジックブロックに進入した敵のArea2Dノードを入力
        RadarLogic.Input(new RadarLogic.Input.WatchEnemy(area));
    }

    /// <summary>
    /// 衝突判定検知用テストコード
    /// TODO:削除する
    /// </summary>
    /// <param name="event"></param>
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            // 新しいArea2Dノードを作成
            Area2D area = new Area2D();

            // CollisionShape2Dを作成
            CollisionShape2D collisionShape = new CollisionShape2D();
            area.CollisionLayer = CollisionEntity.Enemy;
            area.CollisionMask = 0;
            area.AddChild(collisionShape);

            // CircleShape2Dを作成してCollisionShape2Dに設定
            CircleShape2D circleShape = new CircleShape2D();
            circleShape.Radius = 50;  // 半径50の円を作成
            collisionShape.Shape = circleShape;

            // サークルのテクスチャを円の画像に設定（円を描いたテクスチャが必要）
            // ここではColorRectを使って色付きの四角形を表示
            ColorRect colorRect = new ColorRect();
            colorRect.Color = new Color(1, 0, 0);  // 赤色に設定（Colorの値はR,G,Bで0から1の範囲）
            colorRect.Size = new Vector2(10, 10);  // サークルの大きさ（直径）

            area.Monitoring = true;

            area.AddChild(colorRect);

            // クリックした位置にArea2Dを配置
            area.Position = mouseEvent.Position;
            GD.Print("area position" + area.Position);
            GD.Print("mouse position" + mouseEvent.Position);
            GD.Print("click test");
            AddChild(area);  // 現在のノード（Node2D）に追加
        }
    }
}
