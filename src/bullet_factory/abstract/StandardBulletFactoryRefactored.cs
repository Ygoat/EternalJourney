namespace EternalJourney.Bullet.Abstract;

using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using EternalJourney.Bullet.Abstract.Base;
using EternalJourney.BulletFactory.State;
using EternalJourney.Cores.Pooling;
using Godot;

/// <summary>
/// スタンダード弾丸ファクトリインターフェース
/// </summary>
public interface IStandardBulletFactoryRefactored : IBaseFactory<Node2D>
{
    /// <summary>
    /// 弾丸生成
    /// </summary>
    void GenerateBullet();
}

/// <summary>
/// スタンダード弾丸ファクトリクラス（BaseFactoryを使用したリファクタリング版）
/// 重複コードを削減し、Godotエコシステムを活用した実装
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class StandardBulletFactoryRefactored : BaseFactory<Node2D>, IStandardBulletFactoryRefactored
{
    #region State
    /// <summary>
    /// 弾丸ファクトリロジック
    /// </summary>
    public StandardBulletFactoryLogic Logic { get; set; } = default!;

    /// <summary>
    /// 弾丸ファクトリバインド
    /// </summary>
    public StandardBulletFactoryLogic.IBinding Binding { get; set; } = default!;
    #endregion State

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
        base.SetupPoolableObject(obj);

        // Removedイベントをプールへの返却処理に接続
        if (obj is IBaseBullet bullet)
        {
            bullet.Removed += OnBulletRemoved;
        }
    }

    /// <summary>
    /// セットアップ処理のオーバーライド
    /// ロジックの初期化とプールの生成を行う
    /// </summary>
    public override void Setup()
    {
        // ロジックの初期化
        Logic = new StandardBulletFactoryLogic();
        Binding = Logic.Bind();

        // 基底クラスのセットアップ（プール生成）
        base.Setup();
    }

    /// <summary>
    /// 解決後処理のオーバーライド
    /// ロジックの出力ハンドラとタイマーの設定
    /// </summary>
    public override void OnResolved()
    {
        // ロジック出力ハンドラの設定
        Binding
            // Generated出力時
            .Handle((in StandardBulletFactoryLogic.Output.Generated _) =>
            {
                // 弾丸生成（遅延実行）
                CallDeferred(nameof(EmitBullet));
            })
            // Cooling出力時
            .Handle((in StandardBulletFactoryLogic.Output.Cooling _) =>
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
        Logic.Input(new StandardBulletFactoryLogic.Input.CoolDownComplete());
    }

    /// <summary>
    /// 弾丸生成（公開API）
    /// </summary>
    public void GenerateBullet()
    {
        // Fire入力
        Logic.Input(new StandardBulletFactoryLogic.Input.Fire());
    }

    /// <summary>
    /// 弾丸射出
    /// プールから弾丸を取得してシーンツリーに追加し、射出処理を実行
    /// </summary>
    private void EmitBullet()
    {
        // プールから弾丸を取得
        Node2D? bullet = AcquireFromPool();
        if (bullet == null) return;

        // シーンツリーに追加
        AddChild(bullet);

        // 弾丸射出
        if (bullet is IBaseBullet iBullet)
        {
            iBullet.Emit(GlobalPosition, GlobalRotation);
        }

        // クールダウン開始
        Logic.Input(new StandardBulletFactoryLogic.Input.StartCoolDonw());
    }

    /// <summary>
    /// 弾丸除去イベントハンドラ
    /// 弾丸がシーンから削除された際にプールに返却
    /// </summary>
    /// <param name="bullet">除去された弾丸</param>
    private void OnBulletRemoved(BaseBullet bullet)
    {
        // プールに返却
        ReleaseToPool(bullet);
    }
}
