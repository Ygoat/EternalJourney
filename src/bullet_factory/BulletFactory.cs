namespace EternalJourney.BulletFactory;

using System.Collections.Generic;
using System.Linq;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Bullet;
using EternalJourney.BulletFactory.State;
using EternalJourney.Cores.Consts;
using EternalJourney.Cores.Utils;
using Godot;

/// <summary>
/// 弾丸ファクトリインターフェース
/// </summary>
public interface IBulletFactory : INode2D, IProvide<IBulletFactory>
{
    /// <summary>
    /// 弾丸キュー
    /// </summary>
    public Queue<Bullet> BulletsQueue { get; set; }

    /// <summary>
    /// 射撃
    /// </summary>
    public void Shoot();
};

/// <summary>
/// 弾丸ファクトリ
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class BulletFactory : Node2D, IBulletFactory
{
    public override void _Notification(int what) => this.Notify(what);

    #region State
    /// <summary>
    /// 弾丸ファクトリロジック
    /// </summary>
    public BulletFactoryLogic BulletFactoryLogic { get; set; } = default!;

    /// <summary>
    /// 弾丸ファクトリバインド
    /// </summary>
    public BulletFactoryLogic.IBinding BulletFactoryBinding { get; set; } = default!;
    #endregion State

    #region Exports
    /// <summary>
    /// 待機時間
    /// </summary>
    public double WaitTime { get; set; } = 0.1;

    /// <summary>
    ///　弾丸配列
    /// </summary>
    public Bullet[] Bullets { get; set; } = new Bullet[100];

    /// <summary>
    /// 弾丸キュー
    /// </summary>
    public Queue<Bullet> BulletsQueue { get; set; } = new Queue<Bullet>();
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
    IBulletFactory IProvide<IBulletFactory>.Value() => this;
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
        // 依存性提供
        this.Provide();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Setup()
    {
        // 弾丸ロジックインスタンス化
        BulletFactoryLogic = new BulletFactoryLogic();
        // 弾丸ロジックバインド
        BulletFactoryBinding = BulletFactoryLogic.Bind();

        // 弾丸配列生成
        Bullets = Bullets.Select(e =>
        {
            // 弾丸ノードインスタンス化&ロード
            e = Instantiator.LoadAndInstantiate<Bullet>(Const.BulletNodePath);
            // イベントファンクション付与
            e.Collapsed += OnCollapsed;
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
            .Handle((in BulletFactoryLogic.Output.Generated _) =>
            {
                // 弾丸生成
                CallDeferred("GenerateBullet");
            })
            // Cooling出力時
            .Handle((in BulletFactoryLogic.Output.Cooling _) =>
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
        BulletFactoryLogic.Start();
    }

    /// <summary>
    /// タイムアウトイベント
    /// </summary>
    public void OnTimeout()
    {
        // CoolDownCompleteを入力
        BulletFactoryLogic.Input(new BulletFactoryLogic.Input.CoolDownComplete());
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
    /// 射撃
    /// </summary>
    public void Shoot()
    {
        // Fire入力
        BulletFactoryLogic.Input(new BulletFactoryLogic.Input.Fire());
    }

    /// <summary>
    /// 弾丸生成
    /// </summary>
    public void GenerateBullet()
    {
        // 弾丸キュー取り出し
        Bullet bullet = BulletsQueue.Dequeue();
        // 弾丸ノードをノードツリーに追加
        AddChild(bullet);
        // 弾丸射出
        bullet.Emit(GlobalPosition, GlobalRotation);
        // StartCoolDown入力
        BulletFactoryLogic.Input(new BulletFactoryLogic.Input.StartCoolDonw());
    }

    /// <summary>
    /// Collapsedイベントファンクション
    /// </summary>
    /// <param name="bullet"></param>
    public void OnCollapsed(Bullet bullet)
    {
        // キューに追加
        BulletsQueue.Enqueue(bullet);
    }

}
