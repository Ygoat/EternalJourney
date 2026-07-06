namespace EternalJourney.EnemyFactory;

using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using EternalJourney.Common.BaseFactory;
using EternalJourney.Enemy.Base;
using Godot;

/// <summary>
/// エネミーファクトリインターフェース
/// </summary>
public interface IEnemyFactory
{
    void SpawnEnemy();
}

/// <summary>
/// エネミーファクトリクラス（BaseFactoryを使用したリファクタリング版）
/// 重複コードを削減し、Godotエコシステムを活用した実装
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class EnemyFactory : BaseFactory<BaseEnemy>, IEnemyFactory
{
    public override void _Notification(int what) => this.Notify(what);

    /// <summary>
    /// エネミーシーンリソース
    /// </summary>
    [Export]
    public Resource EnemyScene { get; set; } = default!;

    /// <summary>
    /// シーンパスの取得（BaseFactory抽象メソッドの実装）
    /// </summary>
    /// <returns>エネミーシーンのパス</returns>
    protected override string GetScenePath()
    {
        return EnemyScene?.ResourcePath ?? string.Empty;
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
        obj.Removed += OnRemoved;
    }

    /// <summary>
    /// セットアップ処理のオーバーライド
    /// </summary>
    public override void Setup()
    {
        // プールサイズの設定（エネミーは200個）
        PoolSize = 200;
        WaitTime = 1.6f;
        // 基底クラスの初期化を呼び出す
        base.Initialize();

        // 基底クラスのセットアップ（プール生成）
        base.Setup();
    }

    /// <summary>
    /// エネミーをスポーン（公開API）
    /// </summary>
    public void SpawnEnemy()
    {
        RequestGenerate();
    }

    /// <summary>
    /// 生成完了時の処理（BaseFactoryのオーバーライド）
    /// </summary>
    protected override void OnGenerated()
    {
        // エネミー生成（遅延実行）
        CallDeferred(nameof(DequeueAndSpawn));
    }

    /// <summary>
    /// キューからエネミーを取り出してスポーン
    /// プールからエネミーを取得してシーンツリーに追加し、スポーン処理を実行
    /// </summary>
    private void DequeueAndSpawn()
    {
        // プールからエネミーを取得
        BaseEnemy? enemy = AcquireFromPool();
        if (enemy == null)
        {
            return;
        }

        // シーンツリーに追加（Setup()が呼ばれる）
        AddChild(enemy);

        // エネミースポーン
        if (enemy is IBaseEnemy iEnemy)
        {
            iEnemy.Spawn(GlobalPosition, GlobalRotation);
        }

        // クールダウン開始
        StartCoolDown();
    }

    /// <summary>
    /// エネミー除去イベントハンドラ
    /// エネミーがシーンから削除された際にプールに返却
    /// </summary>
    /// <param name="enemy">除去されたエネミー</param>
    private void OnRemoved(BaseEnemy enemy)
    {
        // プールに返却
        ReleaseToPool(enemy);
    }
}
