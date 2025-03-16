namespace EternalJourney.Enemy;

using Chickensoft.AutoInject;
using Chickensoft.Collections;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Cores.Consts;
using EternalJourney.EnemyFactory;
using EternalJourney.EnemySpawner;
using EternalJourney.Ship;
using Godot;

/// <summary>
/// エネミーインターフェース
/// </summary>
public interface IEnemy : INode2D { }

/// <summary>
/// エネミークラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class Enemy : Node2D, IEnemy
{
    public override void _Notification(int what) => this.Notify(what);

    #region State
    /// <summary>
    /// エネミーロジック
    /// </summary>
    public EnemyLogic EnemyLogic { get; set; } = default!;

    /// <summary>
    /// エネミーロジックバインド
    /// </summary>
    public EnemyLogic.IBinding EnemyBinding { get; set; } = default!;
    #endregion State

    #region Exports
    /// <summary>
    /// 移動速度
    /// </summary>
    public int Speed { get; set; } = 3;

    /// <summary>
    /// 標的対象位置
    /// </summary>
    public Vector2 TargetPosition { get; set; } = default!;

    /// <summary>
    /// 進行方向
    /// </summary>
    public Vector2 Direction { get; set; } = default!;
    #endregion Exports

    #region Nodes
    /// <summary>
    /// 衝突判定用
    /// </summary>
    [Node]
    public IArea2D Area2D { get; set; } = default!;

    /// <summary>
    /// 画面外検知用
    /// </summary>
    [Node]
    public IVisibleOnScreenNotifier2D VisibleOnScreenNotifier2D { get; set; } = default!;
    #endregion Nodes

    #region Dependencies
    /// <summary>
    /// エネミーファクトリ
    /// </summary>
    [Dependency]
    public IEnemyFactory EnemyFactory => this.DependOn<IEnemyFactory>();

    /// <summary>
    /// エネミースポナー
    /// </summary>
    [Dependency]
    public IEnemySpawner EnemySpawner => this.DependOn<IEnemySpawner>();

    /// <summary>
    /// エンティティ―テーブル
    /// </summary>
    [Dependency]
    public EntityTable<int> EntityTable => this.DependOn<EntityTable<int>>();
    #endregion Dependencies

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void OnReady()
    {
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Setup()
    {
        // エネミーロジック
        EnemyLogic = new EnemyLogic();
        // エネミーロジックバインド
        EnemyBinding = EnemyLogic.Bind();
        // コリジョンレイヤをエネミーに設定
        Area2D.CollisionLayer = CollisionEntity.Enemy;
        // コリジョンマスクを船と弾丸に設定
        Area2D.CollisionMask = CollisionEntity.Ship | CollisionEntity.Bullet;
        // ターゲットを船のTargetMarker位置に設定
        TargetPosition = EntityTable.Get<IShip>(0)!.EnemyTargetMarker.GlobalPosition;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void OnResolved()
    {
        EnemyBinding
            // StartClose出力時
            .Handle((in EnemyLogic.Output.StartClose _) =>
            {
                // 物理処理有効化
                SetPhysicsProcess(true);
            })
            // Damaged出力時
            .Handle((in EnemyLogic.Output.Damaged _) =>
            {
                // 自ノード除去
                CallDeferred("RemoveSelf");
                // 物理処理無効化
                SetPhysicsProcess(false);
                // エネミーキュー追加
                EnemyFactory.EnemiesQueue.Enqueue(this);
                // Removed入力
                EnemyLogic.Input(new EnemyLogic.Input.Removed());
            })
            // SpawnEnable出力時
            .Handle((in EnemyLogic.Output.SpawnEnable _) =>
            {
            });
        // エリアエンターイベント設定
        Area2D.AreaEntered += OnAreaEntered;
        // スクリーン外イベント設定
        VisibleOnScreenNotifier2D.ScreenExited += OnScreenExited;
        // エネミーロジック初期状態開始
        EnemyLogic.Start();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="delta"></param>
    public void OnPhysicsProcess(double delta)
    {
        // 進行方向設定
        Direction = TargetPosition - GlobalPosition;
        // グローバル位置更新
        GlobalPosition += Speed * Direction.Normalized();
    }

    /// <summary>
    /// エリアエンターイベントファンクション
    /// </summary>
    /// <param name="area"></param>
    public void OnAreaEntered(Area2D area)
    {
        // TakeDamage入力
        EnemyLogic.Input(new EnemyLogic.Input.TakeDamage());
    }

    /// <summary>
    /// 敵スポーン
    /// </summary>
    public void EnemySpawn()
    {
        // TargetDiscover入力
        EnemyLogic.Input(new EnemyLogic.Input.TargetDiscover());
        // グローバル位置をスポナーのPathFollow2Dの位置から取得
        GlobalPosition = EnemySpawner.PathFollow2D.GlobalPosition;
    }

    /// <summary>
    /// エリア外に出た時消す
    /// TODO:それ用のstateを作成したい
    /// </summary>
    public void OnScreenExited()
    {
        // TakeDamage入力
        EnemyLogic.Input(new EnemyLogic.Input.TakeDamage());
    }

    /// <summary>
    /// 自ノード除去
    /// </summary>
    public void RemoveSelf()
    {
        // 自ノード除去
        GetParent().RemoveChild(this);
    }
}
