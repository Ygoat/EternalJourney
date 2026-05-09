namespace EternalJourney.Bullet.Abstract;

using System.Data;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using EternalJourney.Battle.Domain;
using EternalJourney.Bullet.Abstract.Base;
using EternalJourney.Bullet.Abstract.State;
using EternalJourney.Common.BaseFactory;
using EternalJourney.Common.Traits;
using EternalJourney.Cores.Consts;
using EternalJourney.Cores.Models.Bullet;
using EternalJourney.Cores.Repositories;
using Godot;

/// <summary>
/// スタンダード弾丸ファクトリインターフェース
/// </summary>
public interface IStandardBulletFactory
{
    /// <summary>
    /// 弾丸のコリジョンマスク（武器所有者に応じて設定される）
    /// </summary>
    uint BulletCollisionMask { get; set; }

    /// <summary>
    /// プレイヤー所有の弾丸を生成するか（デフォルト: true）
    /// </summary>
    bool IsPlayerBullet { get; set; }

    /// <summary>
    /// 発射間隔（秒）
    /// </summary>
    public float WaitTime { get; set; }

    /// <summary>
    /// 弾丸生成（デフォルトのBulletIdを使用）
    /// </summary>
    public void GenerateBullet();

    /// <summary>
    /// 弾丸生成（弾丸IDを指定）
    /// </summary>
    /// <param name="bulletId">弾丸設定ID（BulletConfig.jsonのidと対応）</param>
    public void GenerateBullet(string bulletId);
}

/// <summary>
/// スタンダード弾丸ファクトリクラス（BaseFactoryを使用したリファクタリング版）
/// 重複コードを削減し、Godotエコシステムを活用した実装
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class StandardBulletFactory : BaseFactory<StandardBullet>, IStandardBulletFactory
{
    public override void _Notification(int what) => this.Notify(what);

    #region Exports
    /// <summary>
    /// 弾丸シーンリソース
    /// </summary>
    [Export]
    public Resource BulletScene { get; set; } = default!;

    /// <summary>
    /// 弾丸名称（弾丸の種類を指定）
    /// </summary>
    [Export]
    public string BulletName { get; set; } = default!;

    #endregion Exports

    #region Dependencies
    [Dependency]
    public IBattleRepo BattleRepo => this.DependOn<IBattleRepo>();
    #endregion Dependencies

    #region State
    public StandardBulletFactoryLogic BulletFactoryLogic { get; set; } = default!;
    public StandardBulletFactoryLogic.IBinding BulletFactoryBind { get; set; } = default!;
    #endregion State

    private readonly BulletConfigReader _bulletConfigReader = new();
    private BulletConfig? _bulletConfig;

    /// <summary>
    /// 弾丸のコリジョンマスク（武器所有者に応じて設定される）
    /// </summary>
    public uint BulletCollisionMask { get; set; } = CollisionEntity.Enemy;

    /// <summary>
    /// プレイヤー所有の弾丸を生成するか（デフォルト: true）
    /// </summary>
    public bool IsPlayerBullet { get; set; } = true;

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
    protected override void SetupPoolableObject(StandardBullet obj)
    {
        // Removedイベントをプールへの返却処理に接続
        obj.Removed += OnRemoved;
    }

    /// <summary>
    /// セットアップ処理のオーバーライド
    /// </summary>
    public override void Setup()
    {
        base.Setup();
        BulletFactoryLogic = new StandardBulletFactoryLogic();
        BulletFactoryBind = BulletFactoryLogic.Bind();
    }

    /// <summary>
    /// 解決後処理（タイマー設定とロジックバインド）
    /// </summary>
    public override void OnResolved()
    {
        Timer.OneShot = true;
        Timer.WaitTime = WaitTime;
        Timer.Timeout += OnTimeout;

        BulletFactoryBind
            .Handle((in StandardBulletFactoryLogic.Output.Fire _) =>
            {
                CallDeferred(nameof(BulletEmit));
            })
            .Handle((in StandardBulletFactoryLogic.Output.StartCoolDown _) =>
            {
                StartTimer();
            });

        BulletFactoryLogic.Start();
    }

    /// <summary>
    /// タイマー開始
    /// </summary>
    protected override void StartTimer()
    {
        Timer.WaitTime = WaitTime;
        base.StartTimer();
    }

    /// <summary>
    /// 弾丸生成（デフォルトのBulletIdを使用）
    /// </summary>
    public void GenerateBullet()
    {
        BulletFactoryLogic.Input(new StandardBulletFactoryLogic.Input.FireRequested());
    }

    /// <summary>
    /// 弾丸生成（弾丸IDを指定）
    /// </summary>
    /// <param name="bulletId">弾丸設定ID（BulletConfig.jsonのidと対応）</param>
    public void GenerateBullet(string bulletId)
    {
        _bulletConfig = _bulletConfigReader.GetById(bulletId);
        BulletFactoryLogic.Input(new StandardBulletFactoryLogic.Input.FireRequested());
    }

    /// <summary>
    /// タイムアウト時の処理（クールダウン完了をロジックに通知）
    /// </summary>
    protected override void OnTimeout()
    {
        BulletFactoryLogic.Input(new StandardBulletFactoryLogic.Input.CoolDownComplete());
    }

    /// <summary>
    /// 弾丸射出
    /// プールから弾丸を取得してシーンツリーに追加し、射出処理を実行
    /// </summary>
    private void BulletEmit()
    {
        // プールから弾丸を取得
        StandardBullet? bullet = AcquireFromPool();
        if (bullet == null)
            return;

        // 弾丸設定を適用（Setup()の後に呼び出すことでJSONの値が反映される）
        if (_bulletConfig != null)
        {
            bullet.Configure(_bulletConfig);
        }

        // 武器所有者に応じたコリジョンマスクを適用
        bullet.CollisionMask = BulletCollisionMask;
        // プレイヤー所有フラグを適用
        bullet.IsPlayerOwned = IsPlayerBullet;

        // シーンツリーに追加（エネミー撃破後も弾丸が残るよう、IBattleRepoを提供する祖先ノードに追加）
        GetBulletContainer().AddChild(bullet);

        // 弾丸射出
        bullet.Emit(GlobalPosition, GlobalRotation);
    }

    /// <summary>
    /// IBattleRepoを提供する最も近い祖先ノードを取得する
    /// 弾丸をエネミーより長生きさせるために、エネミー外のノードに追加する
    /// </summary>
    private Node GetBulletContainer()
    {
        Node? current = GetParent();
        while (current != null)
        {
            if (current is IProvide<IBattleRepo>)
                return current;
            current = current.GetParent();
        }
        // フォールバック：自身を返す
        return this;
    }

    /// <summary>
    /// 弾丸除去イベントハンドラ
    /// 弾丸がシーンから削除された際にプールに返却
    /// </summary>
    /// <param name="bullet">除去された弾丸</param>
    private void OnRemoved(BaseBullet bullet)
    {
        // プールに返却
        if (bullet is StandardBullet standardBullet)
        {
            ReleaseToPool(standardBullet);
        }
    }
}
