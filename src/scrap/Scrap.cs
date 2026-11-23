namespace EternalJourney.Scrap;

using Chickensoft.AutoInject;
using Chickensoft.Collections;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Battle.Domain;
using EternalJourney.Cores.Consts;
using EternalJourney.Cores.Pooling;
using EternalJourney.MagnetPulse;
using EternalJourney.Scrap.State;
using EternalJourney.Ship;
using Godot;

/// <summary>
/// スクラップインターフェース
/// </summary>
public interface IScrap : IArea2D
{
    /// <summary>
    /// 回収時に加算される個数
    /// </summary>
    int Value { get; }

    /// <summary>
    /// 落下速度（px/秒）
    /// </summary>
    float FallSpeed { get; }

    /// <summary>
    /// 吸引開始時の速度（px/秒）
    /// </summary>
    float AttractSpeed { get; }

    /// <summary>
    /// 吸引中の加速度（px/秒²）
    /// </summary>
    float AttractAcceleration { get; }

    /// <summary>
    /// これより下に流れたら消えるY座標
    /// </summary>
    float DespawnY { get; }

    /// <summary>
    /// 指定位置にドロップする
    /// </summary>
    void Drop(Vector2 globalPosition);
}

/// <summary>
/// スクラップクラス（敵の撃破位置から下に流れるドロップアイテム）
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class Scrap : Area2D, IScrap, IPoolable
{
    public override void _Notification(int what) => this.Notify(what);

    #region Signals
    /// <summary>
    /// 除去シグナル（プールへの返却通知）
    /// </summary>
    [Signal]
    public delegate void RemovedEventHandler(Scrap scrap);
    #endregion Signals

    #region State
    /// <summary>
    /// スクラップロジック
    /// </summary>
    public IScrapLogic ScrapLogic { get; set; } = default!;

    /// <summary>
    /// スクラップロジックバインド
    /// </summary>
    public LogicBlock.Binding ScrapBinding { get; set; } = default!;
    #endregion State

    #region Exports
    /// <summary>
    /// 回収時に加算される個数
    /// </summary>
    [Export]
    public int Value { get; set; } = 1;

    /// <summary>
    /// 落下速度（px/秒）
    /// </summary>
    [Export]
    public float FallSpeed { get; set; } = 120f;

    /// <summary>
    /// 吸引開始時の速度（px/秒）
    /// </summary>
    [Export]
    public float AttractSpeed { get; set; } = 300f;

    /// <summary>
    /// 吸引中の加速度（px/秒²）
    /// </summary>
    [Export]
    public float AttractAcceleration { get; set; } = 900f;

    /// <summary>
    /// これより下に流れたら消えるY座標
    /// </summary>
    [Export]
    public float DespawnY { get; set; } = 1400f;
    #endregion Exports

    #region Dependencies
    [Dependency]
    public IBattleRepo BattleRepo => this.DependOn<IBattleRepo>();

    [Dependency]
    public EntityTable<int> EntityTable => this.DependOn<EntityTable<int>>();
    #endregion Dependencies

    public void Setup()
    {
        ScrapLogic = new ScrapLogic();
        ScrapBinding = ScrapLogic.Bind();

        // スクラップ層に置き、Shipと磁気パルスだけを検知する
        CollisionLayer = CollisionEntity.Scrap;
        CollisionMask = CollisionEntity.Ship | CollisionEntity.MagnetPulse;
        // 親ノードの座標に影響されないようにする
        TopLevel = true;
    }

    public void OnResolved()
    {
        ScrapLogic.Set(this as IScrap);
        ScrapLogic.Set(BattleRepo);
        ScrapLogic.Set(EntityTable.Get<IShip>(0)!);

        ScrapBinding
            .OnOutput((in ScrapLogic.Output.Place output) =>
            {
                GlobalPosition = output.GlobalPosition;
                SetPhysicsProcess(true);
            })
            .OnOutput((in ScrapLogic.Output.Move output) =>
            {
                GlobalPosition += output.PositionDelta;
            })
            .OnOutput((in ScrapLogic.Output.RemoveSelf _) =>
            {
                // 物理クエリのフラッシュ中にツリーを変更できないため遅延させる
                CallDeferred(nameof(RemoveSelf));
            });

        AreaEntered += OnAreaEntered;
        ScrapLogic.Start<ScrapState.Idle>();
    }

    public void OnPhysicsProcess(double delta)
    {
        ScrapLogic.Input(new ScrapLogic.Input.PhysicsProcess(delta));
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Drop(Vector2 globalPosition)
    {
        ScrapLogic.Input(new ScrapLogic.Input.Drop(globalPosition));
    }

    /// <summary>
    /// プールから取得された時のコールバック
    /// </summary>
    public void OnAcquired()
    {
    }

    /// <summary>
    /// プールに返却された時のコールバック
    /// </summary>
    public void OnReleased()
    {
        SetPhysicsProcess(false);
    }

    /// <summary>
    /// 自インスタンスをツリーから一時的に取り除く（プールへ返却される）
    /// </summary>
    public void RemoveSelf()
    {
        GetParent().RemoveChild(this);
        SetPhysicsProcess(false);
        EmitSignal(SignalName.Removed, this);
    }

    // 注意: プールに返却するたびにツリーから外れるため、OnExitTree では Logic / Binding を Dispose しない。
    // 破棄はファクトリがプールごと解放する際に行われる。

    private void OnAreaEntered(Area2D area)
    {
        if (area is IMagnetPulse)
        {
            ScrapLogic.Input(new ScrapLogic.Input.PulseTouched());
        }
        else if (area is IShip)
        {
            ScrapLogic.Input(new ScrapLogic.Input.ShipTouched());
        }
    }
}
