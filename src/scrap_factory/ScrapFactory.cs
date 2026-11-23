namespace EternalJourney.ScrapFactory;

using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using EternalJourney.Battle.Domain;
using EternalJourney.Common.BaseFactory;
using EternalJourney.Enemy.Base;
using EternalJourney.Scrap;
using Godot;

/// <summary>
/// スクラップファクトリクラス
/// 敵の撃破位置にスクラップをドロップする（プール運用）
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class ScrapFactory : BaseFactory<Scrap>
{
    public override void _Notification(int what) => this.Notify(what);

    #region Exports
    /// <summary>
    /// スクラップシーン
    /// </summary>
    [Export]
    public PackedScene ScrapScene { get; set; } = default!;

    /// <summary>
    /// 敵1体あたりのドロップ数
    /// </summary>
    [Export]
    public int DropCountPerEnemy { get; set; } = 1;
    #endregion Exports

    #region Dependencies
    [Dependency]
    public IBattleRepo BattleRepo => this.DependOn<IBattleRepo>();
    #endregion Dependencies

    /// <summary>
    /// シーンパスの取得（BaseFactory抽象メソッドの実装）
    /// </summary>
    protected override string GetScenePath() => ScrapScene?.ResourcePath ?? string.Empty;

    /// <summary>
    /// プール可能なオブジェクトのセットアップ
    /// </summary>
    protected override void SetupPoolableObject(Scrap obj)
    {
        base.SetupPoolableObject(obj);

        // Removedイベントをプールへの返却処理に接続
        obj.Removed += OnRemoved;
    }

    /// <summary>
    /// セットアップ処理（プール生成）
    /// </summary>
    public override void Setup()
    {
        base.Initialize();
        base.Setup();
    }

    /// <summary>
    /// 解決後処理（敵撃破イベントの購読）
    /// </summary>
    public override void OnResolved()
    {
        base.OnResolved();
        BattleRepo.EnemyDestroyed += OnEnemyDestroyed;
    }

    /// <summary>
    /// ツリー離脱時の後処理
    /// </summary>
    public void OnExitTree()
    {
        BattleRepo.EnemyDestroyed -= OnEnemyDestroyed;

        // プール内のスクラップはツリーに属さないため、ここで解放する
        foreach (Scrap scrap in ObjectQueue)
        {
            if (IsInstanceValid(scrap))
            {
                scrap.Free();
            }
        }
        ObjectQueue.Clear();
    }

    /// <summary>
    /// 敵撃破時：撃破位置にスクラップをドロップする
    /// </summary>
    private void OnEnemyDestroyed(IBaseEnemy enemy)
    {
        // 物理クエリのフラッシュ中にツリーを変更できないため遅延させる
        CallDeferred(nameof(DropScrap), enemy.GlobalPosition);
    }

    /// <summary>
    /// 指定位置にスクラップをドロップする
    /// </summary>
    private void DropScrap(Vector2 globalPosition)
    {
        for (int i = 0; i < DropCountPerEnemy; i++)
        {
            Scrap? scrap = AcquireFromPool();
            if (scrap == null)
            {
                return;
            }

            // シーンツリーに追加（初回のみSetup()/OnResolved()が呼ばれる）
            AddChild(scrap);
            scrap.Drop(globalPosition);
        }
    }

    /// <summary>
    /// スクラップ除去イベントハンドラ
    /// </summary>
    private void OnRemoved(Scrap scrap)
    {
        ReleaseToPool(scrap);
    }
}
