namespace EternalJourney.Bullet.Abstract;

using System.Collections.Generic;
using System.Linq;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Bullet.Abstract.Base;
using EternalJourney.BulletFactory.State;
using EternalJourney.Cores.Utils;
using Godot;

/// <summary>
/// スタンダード弾丸ファクトリインターフェース
/// </summary>
public interface IStandardBulletFactory : IBaseBulletFactory
{

}

/// <summary>
/// スタンダード弾丸ファクトリクラス
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class StandardBulletFactory : BaseBulletFactory, IStandardBulletFactory
{
    public override void _Notification(int what) => this.Notify(what);

    #region State
    /// <summary>
    /// 弾丸ファクトリロジック
    /// </summary>
    public StandardBulletFactoryLogic StandardBulletFactoryLogic { get; set; } = default!;

    /// <summary>
    /// 弾丸ファクトリバインド
    /// </summary>
    public StandardBulletFactoryLogic.IBinding BulletFactoryBinding { get; set; } = default!;
    #endregion State

    #region Exports
    /// <summary>
    /// 弾丸シーンリソース
    /// </summary>
    [Export]
    public Resource BulletScene { get; set; } = default!;

    /// <summary>
    /// 待機時間
    /// </summary>
    public double WaitTime { get; set; } = 0.1;

    /// <summary>
    ///　弾丸配列
    /// </summary>
    public Node2D[] Bullets { get; set; } = new Node2D[100];

    /// <summary>
    /// 弾丸キュー
    /// </summary>
    public Queue<Node2D> BulletsQueue { get; set; } = new Queue<Node2D>();
    #endregion Exports

    #region Nodes
    /// <summary>
    /// タイマーノード
    /// </summary>
    [Node]
    public ITimer Timer { get; set; } = default!;
    #endregion Nodes

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
        // 依存性提供
        this.Provide();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Setup()
    {
        // 弾丸ロジックインスタンス化
        StandardBulletFactoryLogic = new StandardBulletFactoryLogic();
        // 弾丸ロジックバインド
        BulletFactoryBinding = StandardBulletFactoryLogic.Bind();

        // 弾丸配列生成
        Bullets = Bullets.Select(e =>
        {
            // 弾丸ノードインスタンス化&ロード
            e = Instantiator.LoadAndInstantiate<Node2D>(BulletScene.ResourcePath);
            // イベントファンクション付与
            if (e is IStandardBullet iBullet)
            {
                iBullet.Removed += OnRemoved;
            }
            // キュー追加
            BulletsQueue.Enqueue(e);
            return e;
        }).ToArray();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void OnResolved()
    {
        BulletFactoryBinding
            // Generated出力時
            .Handle((in StandardBulletFactoryLogic.Output.Generated _) =>
            {
                // 弾丸生成
                CallDeferred("BulletEmit");
            })
            // Cooling出力時
            .Handle((in StandardBulletFactoryLogic.Output.Cooling _) =>
            {
                // タイマーセット
                SetTimer();
            });
        // タイマーワンショット設定
        Timer.OneShot = true;
        // 待機時間設定
        Timer.SetWaitTime(WaitTime);
        // タイムアウトイベント設定
        Timer.Timeout += OnTimeout;
        // 弾丸ロジック初期状態セット
        StandardBulletFactoryLogic.Start();
    }

    /// <summary>
    /// タイムアウトイベント
    /// </summary>
    public void OnTimeout()
    {
        // CoolDownCompleteを入力
        StandardBulletFactoryLogic.Input(new StandardBulletFactoryLogic.Input.CoolDownComplete());
    }

    /// <summary>
    /// タイマーセット
    /// </summary>
    public void SetTimer()
    {
        // クールダウンタイマー開始
        Timer.Start();
    }

    /// <summary>
    /// 弾丸生成
    /// </summary>
    public override void GenerateBullet()
    {
        // Fire入力
        StandardBulletFactoryLogic.Input(new StandardBulletFactoryLogic.Input.Fire());
    }

    /// <summary>
    /// 弾丸射出
    /// </summary>
    public void BulletEmit()
    {
        // 弾丸キュー取り出し
        Node2D bullet = BulletsQueue.Dequeue();
        // 弾丸ノードをノードツリーに追加
        AddChild(bullet);
        // 弾丸射出
        if (bullet is IStandardBullet iBullet)
        {
            iBullet.Emit(GlobalPosition, GlobalRotation);
        }
        // StartCoolDown入力
        StandardBulletFactoryLogic.Input(new StandardBulletFactoryLogic.Input.StartCoolDonw());
    }

    /// <summary>
    /// Collapsedイベントファンクション
    /// </summary>
    /// <param name="bullet"></param>
    public void OnRemoved(StandardBullet bullet)
    {
        // キューに追加
        BulletsQueue.Enqueue(bullet);
    }
}
