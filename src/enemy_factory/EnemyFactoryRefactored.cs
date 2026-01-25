namespace EternalJourney.EnemyFactory;

using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using EternalJourney.Cores.Consts;
using EternalJourney.Cores.Pooling;
using EternalJourney.Enemy.Abstract.Base;
using EternalJourney.EnemyFactory.State;
using Godot;

/// <summary>
/// エネミーファクトリインターフェース（リファクタリング版）
/// </summary>
public interface IEnemyFactoryRefactored : IBaseFactory<BaseEnemy>
{
    /// <summary>
    /// エネミーをスポーン
    /// </summary>
    void SpawnEnemy();
}

/// <summary>
/// エネミーファクトリクラス（BaseFactoryを使用したリファクタリング版）
/// 重複コードを削減し、Godotエコシステムを活用した実装
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class EnemyFactoryRefactored : BaseFactory<BaseEnemy>, IEnemyFactoryRefactored
{
    #region State
    /// <summary>
    /// エネミーファクトリロジック
    /// </summary>
    public EnemyFactoryLogic Logic { get; set; } = default!;

    /// <summary>
    /// エネミーファクトリバインド
    /// </summary>
    public EnemyFactoryLogic.IBinding Binding { get; set; } = default!;
    #endregion State

    /// <summary>
    /// シーンパスの取得（BaseFactory抽象メソッドの実装）
    /// </summary>
    /// <returns>エネミーシーンのパス</returns>
    protected override string GetScenePath()
    {
        return Const.EnemyNodePath;
    }

    /// <summary>
    /// プール可能なオブジェクトのセットアップ
    /// エネミーがプールに返却される際のイベントを設定
    /// </summary>
    /// <param name="obj">セットアップするエネミーオブジェクト</param>
    protected override void SetupPoolableObject(BaseEnemy obj)
    {
        base.SetupPoolableObject(obj);

        // Removedイベントをプールへの返却処理に接続
        obj.Removed += OnEnemyRemoved;
    }

    /// <summary>
    /// セットアップ処理のオーバーライド
    /// ロジックの初期化とプールの生成を行う
    /// </summary>
    public override void Setup()
    {
        // プールサイズの設定（エネミーは200個）
        PoolSize = 200;

        // ロジックの初期化
        Logic = new EnemyFactoryLogic();
        Binding = Logic.Bind();

        // 基底クラスのセットアップ（プール生成）
        base.Setup();
    }

    /// <summary>
    /// 解決後処理のオーバーライド
    /// ロジックの出力ハンドラとタイマーの設定
    /// </summary>
    public void OnReady()
    {
        // ロジック出力ハンドラの設定
        Binding
            // ReadyComplete出力時
            .Handle((in EnemyFactoryLogic.Output.ReadyComplete _) =>
            {
                // エネミー生成（遅延実行）
                CallDeferred(nameof(DequeueAndSpawn));
            })
            // StartCoolDown出力時
            .Handle((in EnemyFactoryLogic.Output.StartCoolDown _) =>
            {
                // タイマー開始
                StartTimer();
            });

        // 基底クラスの解決後処理（タイマー設定）
        base.OnResolved();

        // ロジックの開始
        Logic.Start();
    }

    /// <summary>
    /// タイムアウトイベント（BaseFactoryのオーバーライド）
    /// </summary>
    protected override void OnTimeout()
    {
        // クールダウン完了を入力
        Logic.Input(new EnemyFactoryLogic.Input.CoolDownComplete());
    }

    /// <summary>
    /// エネミーをスポーン（公開API）
    /// </summary>
    public void SpawnEnemy()
    {
        // Spawn入力
        Logic.Input(new EnemyFactoryLogic.Input.Spawn());
    }

    /// <summary>
    /// キューからエネミーを取り出してスポーン
    /// プールからエネミーを取得してシーンツリーに追加し、スポーン処理を実行
    /// </summary>
    private void DequeueAndSpawn()
    {
        // Spawn入力
        Logic.Input(new EnemyFactoryLogic.Input.Spawn());

        // プールからエネミーを取得
        BaseEnemy? enemy = AcquireFromPool();
        if (enemy == null) return;

        // シーンツリーに追加
        AddChild(enemy);

        // エネミースポーン
        if (enemy is IBaseEnemy iEnemy)
        {
            iEnemy.Spawn(GlobalPosition, GlobalRotation);
        }
    }

    /// <summary>
    /// エネミー除去イベントハンドラ
    /// エネミーがシーンから削除された際にプールに返却
    /// </summary>
    /// <param name="enemy">除去されたエネミー</param>
    private void OnEnemyRemoved(BaseEnemy enemy)
    {
        // プールに返却
        ReleaseToPool(enemy);
    }
}
