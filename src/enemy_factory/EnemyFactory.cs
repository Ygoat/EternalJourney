namespace EternalJourney.EnemyFactory;

using System.Collections.Generic;
using System.Linq;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Cores.Consts;
using EternalJourney.Cores.Utils;
using EternalJourney.Enemy.Abstract.Base;
using EternalJourney.EnemyFactory.State;
using Godot;

/// <summary>
/// エネミーファクトリインターフェース
/// </summary>
public interface IEnemyFactory : INode2D, IProvide<IEnemyFactory>
{
    public Queue<Node2D> EnemiesQueue { get; set; }
    public void SpawnEnemy();
};

/// <summary>
/// エネミーファクトリークラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class EnemyFactory : Node2D, IEnemyFactory
{
    public override void _Notification(int what) => this.Notify(what);

    #region State
    /// <summary>
    /// エネミーファクトリロジック
    /// </summary>
    public EnemyFactoryLogic EnemyFactoryLogic { get; set; } = default!;

    /// <summary>
    /// エネミーファクトリバインド
    /// </summary>
    public EnemyFactoryLogic.IBinding EnemyFactoryBinding { get; set; } = default!;
    #endregion State

    #region Exports
    /// <summary>
    /// 待機時間
    /// </summary>
    public double WaitTime { get; set; } = 0.1;

    /// <summary>
    ///　エネミー配列
    /// </summary>
    public Node2D[] Enemies { get; set; } = new Node2D[200];

    /// <summary>
    /// エネミーキュー
    /// </summary>
    public Queue<Node2D> EnemiesQueue { get; set; } = new Queue<Node2D>();
    #endregion Exports

    #region Nodes
    /// <summary>
    /// タイマーノード
    /// </summary>
    [Node]
    public ITimer Timer { get; set; } = default!;
    #endregion Nodes

    #region Provisions
    /// <summary>
    /// 弾丸ファクトリプロバイダー
    /// </summary>
    /// <returns></returns>
    IEnemyFactory IProvide<IEnemyFactory>.Value() => this;
    #endregion Provisions

    #region Dependencies
    /// <summary>
    /// インスタンス化部品
    /// </summary>
    [Dependency]
    public IInstantiator Instantiator => this.DependOn<IInstantiator>(() => new Instantiator(GetTree()));
    #endregion Dependencies

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Initialize()
    {
        this.Provide();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Setup()
    {
        // エネミーファクトリロジックインスタンス化
        EnemyFactoryLogic = new EnemyFactoryLogic();
        // エネミーファクトリロジックバインド
        EnemyFactoryBinding = EnemyFactoryLogic.Bind();

        // エネミー配列
        Enemies = Enemies.Select(e =>
        {
            // エネミーインスタンス化
            e = Instantiator.LoadAndInstantiate<Node2D>(Const.EnemyNodePath);
            if (e is IBaseEnemy iBaseEnemy)
            {
                iBaseEnemy.Removed += OnRemoved;
            }
            // エネミーキュー追加
            EnemiesQueue.Enqueue(e);
            return e;
        }).ToArray();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void OnReady()
    {
        EnemyFactoryBinding
            // ReadyComplete出力時
            .Handle((in EnemyFactoryLogic.Output.ReadyComplete _) =>
            {
                // 物理処理有効化
                // SetPhysicsProcess(true);
                CallDeferred(nameof(DequeueAndSpawn));
            })
            // StartCoolDown出力時
            .Handle((in EnemyFactoryLogic.Output.StartCoolDown _) =>
            {
                // 物理処理無効化
                // SetPhysicsProcess(false);
                // タイマーセット
                SetTimer();
            });
        // タイマーワンショット設定
        Timer.OneShot = true;
        // タイマー待機時間設定
        Timer.SetWaitTime(WaitTime);
        // タイマーイベント設定
        Timer.Timeout += OnTimeout;
        // エネミーファクトリーロジック初期状態開始
        EnemyFactoryLogic.Start();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="delta"></param>
    public void OnPhysicsProcess(double delta)
    {
        // エネミー生成
        // CallDeferred(nameof(GenerateEnemy));
    }

    /// <summary>
    /// タイムアウトイベント
    /// </summary>
    public void OnTimeout()
    {
        // CoolDownComplete入力
        EnemyFactoryLogic.Input(new EnemyFactoryLogic.Input.CoolDownComplete());
    }

    /// <summary>
    /// タイマーセット
    /// </summary>
    public void SetTimer()
    {
        // タイマースタート
        Timer.Start();
    }

    /// <summary>
    /// キューからエネミーを取り出してスポーン
    /// </summary>
    public void DequeueAndSpawn()
    {
        // Spawn入力
        EnemyFactoryLogic.Input(new EnemyFactoryLogic.Input.Spawn());
        // エネミーキュー取り出し
        Node2D enemy = EnemiesQueue.Dequeue();

        AddChild(enemy);
        if (enemy is IBaseEnemy iEnemy)
        {
            iEnemy.Spawn(GlobalPosition, GlobalRotation);
        }
    }

    /// <summary>
    /// エネミーをスポーン
    /// </summary>
    /// <param name="spawnGlobalPosition"></param>
    /// <param name="spawnGlobalAngle"></param>
    public void SpawnEnemy()
    {
        // Spawn入力
        EnemyFactoryLogic.Input(new EnemyFactoryLogic.Input.Spawn());
    }

    /// <summary>
    /// Collapsedイベントファンクション
    /// </summary>
    /// <param name="bullet"></param>
    public void OnRemoved(BaseEnemy enemy)
    {
        // キューに追加
        EnemiesQueue.Enqueue(enemy);
    }
}
