namespace EternalJourney.EnemyFactory;

using System.Collections.Generic;
using System.Linq;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Cores.Consts;
using EternalJourney.Cores.Utils;
using EternalJourney.Enemy;
using EternalJourney.EnemyFactory.State;
using Godot;

/// <summary>
/// エネミーファクトリインターフェース
/// </summary>
public interface IEnemyFactory : INode, IProvide<IEnemyFactory>
{
    public Queue<Enemy> EnemiesQueue { get; set; }
};

/// <summary>
/// エネミーファクトリークラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class EnemyFactory : Node, IEnemyFactory
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
    ///　弾丸配列
    /// </summary>
    public Enemy[] Enemies { get; set; } = new Enemy[100];

    /// <summary>
    /// 弾丸キュー
    /// </summary>
    public Queue<Enemy> EnemiesQueue { get; set; } = new Queue<Enemy>();
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
            e = Instantiator.LoadAndInstantiate<Enemy>(Const.EnemyNodePath);
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
                SetPhysicsProcess(true);
            })
            // StartCoolDown出力時
            .Handle((in EnemyFactoryLogic.Output.StartCoolDown _) =>
            {
                // 物理処理無効化
                SetPhysicsProcess(false);
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
        CallDeferred(nameof(GenerateEnemy));
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
    /// 射撃
    /// </summary>
    public void GenerateEnemy()
    {
        // Spawn入力
        EnemyFactoryLogic.Input(new EnemyFactoryLogic.Input.Spawn());
        // エネミーキュー取り出し
        Enemy enemy = EnemiesQueue.Dequeue();
        // エネミー追加
        AddChild(enemy);
        // エネミースポーン
        enemy.EnemySpawn();
    }
}
