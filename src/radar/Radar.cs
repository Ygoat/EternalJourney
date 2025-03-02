namespace EternalJourney.Radar;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Radar.State;
using Godot;

[Meta(typeof(IAutoNode))]
public partial class Radar : Node2D
{
    public override void _Notification(int what) => this.Notify(what);

    [Node("%SearchArea")]
    public IArea2D Area2D { get; set; } = default!;

    public IRadarLogic RadarLogic { get; set; } = default!;

    public RadarLogic.IBinding RadarLogicBinding { get; set; } = default!;

    public void Setup()
    {
        RadarLogic = new RadarLogic();
    }

    public void OnResolved()
    {
        Area2D.AreaEntered += OnAreaEntered;
        RadarLogicBinding = RadarLogic.Bind();
        GD.Print("Ready!");

        RadarLogicBinding.Handle((in RadarLogic.Output.StatusChanged output) => GD.Print("Changed"));
    }

    public void OnAreaEntered(Area2D area)
    {
        GD.Print("Entered!");
        RadarLogic.Input(new RadarLogic.Input.EnemyEntered(area));
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
            area.CollisionLayer = 1;
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
