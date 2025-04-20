namespace EternalJourney.Enemy.Abstract;

using Chickensoft.AutoInject;
using Chickensoft.Collections;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Common.Traits;
using EternalJourney.Cores.Consts;
using EternalJourney.Enemy.Abstract.Base;
using EternalJourney.Enemy.Abstract.State;
using EternalJourney.Ship;
using Godot;

/// <summary>
/// スタンダードエネミーインタフェース
/// </summary>
public interface IStandardEnemy : IBaseEnemy, IDetectable, IMovable
{
}

/// <summary>
/// スタンダードエネミークラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class StandardEnemy : BaseEnemy, IStandardEnemy
{
    public override void _Notification(int what) => this.Notify(what);

    #region State
    /// <summary>
    /// エネミーロジック
    /// </summary>
    public StandardEnemyLogic StandardEnemyLogic { get; set; } = default!;

    /// <summary>
    /// エネミーロジックバインド
    /// </summary>
    public StandardEnemyLogic.IBinding StandardEnemyBinding { get; set; } = default!;
    #endregion State

    #region Exports
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public float DetectRange { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public bool CanDetect { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public bool CanMove { get; set; }

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
        StandardEnemyLogic = new StandardEnemyLogic();
        // エネミーロジックバインド
        StandardEnemyBinding = StandardEnemyLogic.Bind();
        // コリジョンレイヤをエネミーに設定
        Area2D.CollisionLayer = CollisionEntity.Enemy;
        // コリジョンマスクを船と弾丸に設定
        Area2D.CollisionMask = CollisionEntity.Ship | CollisionEntity.Bullet;
        // ステータスセット
        SetStatus(new Status { Spd = 5.0f, MaxDur = 0.1f });
        // 耐久残存イベント設定
        DurabilityModule.DurabilityLeft += OnDurabilityLeft;
        // 耐久ゼロイベント設定
        DurabilityModule.ZeroDurability += OnZeroDurability;
        TargetPosition = Detect<IShip>().EnemyTargetMarker.GlobalPosition;
        TopLevel = true;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void OnResolved()
    {
        StandardEnemyBinding
            // StartClose出力時
            .Handle((in StandardEnemyLogic.Output.StartClose _) =>
            {
                // 物理処理有効化
                SetPhysicsProcess(true);
            })
            // Decay出力時
            .Handle((in StandardEnemyLogic.Output.Decay _) =>
            {
                DurabilityModule.TakeDamage(1);
            })
            // Damaged出力時
            .Handle((in StandardEnemyLogic.Output.Disappear _) =>
            {
                // 自ノード除去
                CallDeferred(nameof(RemoveSelf));
            })
            // SpawnEnable出力時
            .Handle((in StandardEnemyLogic.Output.SpawnEnable _) =>
            {
            });
        // エリアエンターイベント設定
        Area2D.AreaEntered += OnAreaEntered;
        // スクリーン外イベント設定
        VisibleOnScreenNotifier2D.ScreenExited += OnScreenExited;
        // エネミーロジック初期状態開始
        StandardEnemyLogic.Start();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="delta"></param>
    public void OnPhysicsProcess(double delta)
    {
        Move();
    }

    /// <summary>
    /// エリアエンターイベントファンクション
    /// </summary>
    /// <param name="area"></param>
    public void OnAreaEntered(Area2D area)
    {
        // TakeDamage入力
        StandardEnemyLogic.Input(new StandardEnemyLogic.Input.TakeDamage());
    }

    /// <summary>
    /// 敵スポーン
    /// </summary>
    public override void Spawn(Vector2 spawnGlobalPosition, float spawnGlobalAngle)
    {
        // TargetDiscover入力
        StandardEnemyLogic.Input(new StandardEnemyLogic.Input.TargetDiscover());
        // グローバル位置をスポナーのPathFollow2Dの位置から取得
        GlobalPosition = spawnGlobalPosition;
    }

    /// <summary>
    /// エリア外に出た時消す
    /// TODO:それ用のstateを作成したい
    /// </summary>
    public void OnScreenExited()
    {
        // TakeDamage入力
        StandardEnemyLogic.Input(new StandardEnemyLogic.Input.TakeDamage());
    }

    /// <summary>
    /// 自インスタンスをツリーから一時的に取り除く
    /// ※インスタンスは完全には削除されない
    /// </summary>
    public override void RemoveSelf()
    {
        // 親ノードを取得してから、子である自ノードを削除する
        GetParent().RemoveChild(this);
        // エネミーの初期化
        InitializeEnemy();
        // 物理処理無効化
        SetPhysicsProcess(false);
        // シグナル出力
        EmitSignal(BaseEnemy.SignalName.Removed, this);
        // Removed入力
        StandardEnemyLogic.Input(new StandardEnemyLogic.Input.Removed());
    }

    /// <summary>
    /// エネミー初期化
    /// </summary>
    public void InitializeEnemy()
    {
        // グローバル座標の初期化
        GlobalPosition = new Vector2(0, 0);
        // 方向を初期化
        Direction = new Vector2(0, 0);
        // 耐久値を回復
        DurabilityModule.FullRepir();
    }

    /// <summary>
    /// 耐久値ゼロイベントファンクション
    /// </summary>
    private void OnZeroDurability()
    {
        StandardEnemyLogic.Input(new StandardEnemyLogic.Input.Collapse());
    }

    /// <summary>
    /// 耐久値残存イベントファンクション
    /// </summary>
    private void OnDurabilityLeft()
    {
        StandardEnemyLogic.Input(new StandardEnemyLogic.Input.Alive());
    }

    /// <summary>
    /// 探知
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T Detect<T>() where T : class
    {
        return EntityTable.Get<T>(0)!;
    }

    /// <summary>
    /// 移動
    /// </summary>
    public void Move()
    {
        // 進行方向設定
        Direction = TargetPosition - GlobalPosition;
        // グローバル位置更新
        GlobalPosition += Speed * Direction.Normalized();
    }

}
