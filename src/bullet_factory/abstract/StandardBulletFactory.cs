namespace EternalJourney.Bullet.Abstract;

using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using EternalJourney.Bullet.Abstract.Base;
using EternalJourney.Common.BaseFactory;
using Godot;

/// <summary>
/// スタンダード弾丸ファクトリインターフェース
/// </summary>
public interface IStandardBulletFactory
{
    /// <summary>
    /// 弾丸生成
    /// </summary>
    public void GenerateBullet();
}

/// <summary>
/// スタンダード弾丸ファクトリクラス（BaseFactoryを使用したリファクタリング版）
/// 重複コードを削減し、Godotエコシステムを活用した実装
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class StandardBulletFactory : BaseFactory<Node2D>, IStandardBulletFactory
{
    public override void _Notification(int what) => this.Notify(what);

    #region Exports
    /// <summary>
    /// 弾丸シーンリソース
    /// </summary>
    [Export]
    public Resource BulletScene { get; set; } = default!;
    #endregion Exports

    /// <summary>
    /// シーンパスの取得（BaseFactory抽象メソッドの実装）
    /// </summary>
    /// <returns>弾丸シーンのパス</returns>
    protected override string GetScenePath()
    {
        return BulletScene?.ResourcePath ?? string.Empty;
    }

    /// <summary>
    /// プール可能なオブジェクトのセットアップ
    /// 弾丸がプールに返却される際のイベントを設定
    /// </summary>
    /// <param name="obj">セットアップする弾丸オブジェクト</param>
    protected override void SetupPoolableObject(Node2D obj)
    {
        // Removedイベントをプールへの返却処理に接続
        if (obj is IBaseBullet bullet)
        {
            bullet.Removed += OnRemoved;
        }
    }

    /// <summary>
    /// セットアップ処理のオーバーライド
    /// </summary>
    public override void Setup()
    {
        // 基底クラスの初期化を呼び出す
        base.Initialize();

        // 基底クラスのセットアップ（プール生成）
        base.Setup();
    }

    /// <summary>
    /// 弾丸生成（公開API）
    /// </summary>
    public void GenerateBullet()
    {
        RequestGenerate();
    }

    /// <summary>
    /// 生成完了時の処理（BaseFactoryのオーバーライド）
    /// </summary>
    protected override void OnGenerated()
    {
        // 弾丸生成（遅延実行）
        CallDeferred(nameof(BulletEmit));
    }

    /// <summary>
    /// 弾丸射出
    /// プールから弾丸を取得してシーンツリーに追加し、射出処理を実行
    /// </summary>
    private void BulletEmit()
    {
        // プールから弾丸を取得
        Node2D? bullet = AcquireFromPool();
        if (bullet == null)
            return;

        // シーンツリーに追加
        AddChild(bullet);

        // 弾丸射出
        if (bullet is IBaseBullet iBullet)
        {
            iBullet.Emit(GlobalPosition, GlobalRotation);
        }

        // クールダウン開始
        StartCoolDown();
    }

    /// <summary>
    /// 弾丸除去イベントハンドラ
    /// 弾丸がシーンから削除された際にプールに返却
    /// </summary>
    /// <param name="bullet">除去された弾丸</param>
    private void OnRemoved(BaseBullet bullet)
    {
        // プールに返却
        ReleaseToPool(bullet);
    }
}
