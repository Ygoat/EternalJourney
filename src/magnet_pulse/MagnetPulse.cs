namespace EternalJourney.MagnetPulse;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Cores.Consts;
using EternalJourney.MagnetPulse.State;
using Godot;

/// <summary>
/// 磁気パルスインターフェース
/// </summary>
public interface IMagnetPulse : IArea2D;

/// <summary>
/// 磁気パルスクラス（Ship中央から一定間隔で円状に広がり、スクラップを引き寄せる）
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class MagnetPulse : Area2D, IMagnetPulse
{
    public override void _Notification(int what) => this.Notify(what);

    #region State
    /// <summary>
    /// 磁気パルスロジック
    /// </summary>
    public IMagnetPulseLogic MagnetPulseLogic { get; set; } = default!;

    /// <summary>
    /// 磁気パルスロジックバインド
    /// </summary>
    public LogicBlock.Binding MagnetPulseBinding { get; set; } = default!;

    /// <summary>
    /// 描画中の半径（0のときは描画しない）
    /// </summary>
    private float _drawRadius;
    #endregion State

    #region Exports
    /// <summary>
    /// 発射間隔（秒）
    /// </summary>
    [Export]
    public float Interval { get; set; } = 3f;

    /// <summary>
    /// 最大半径（px）
    /// </summary>
    [Export]
    public float MaxRadius { get; set; } = 420f;

    /// <summary>
    /// 拡大速度（px/秒）
    /// </summary>
    [Export]
    public float ExpandSpeed { get; set; } = 420f;

    /// <summary>
    /// リングの色
    /// </summary>
    [Export]
    public Color RingColor { get; set; } = new Color(0.4f, 0.8f, 1f, 0.8f);

    /// <summary>
    /// リングの太さ（px）
    /// </summary>
    [Export]
    public float RingWidth { get; set; } = 3f;
    #endregion Exports

    #region Nodes
    [Node]
    public CollisionShape2D CollisionShape2D { get; set; } = default!;
    #endregion Nodes

    public void Setup()
    {
        MagnetPulseLogic = new MagnetPulseLogic();
        MagnetPulseBinding = MagnetPulseLogic.Bind();

        // パルス層に置く。スクラップ側が検知するため、自身は何も検知しない
        CollisionLayer = CollisionEntity.MagnetPulse;
        CollisionMask = 0;
        Monitoring = false;
    }

    public void OnResolved()
    {
        MagnetPulseLogic.Set(new MagnetPulseLogic.Settings(Interval, MaxRadius, ExpandSpeed));

        MagnetPulseBinding
            .OnOutput((in MagnetPulseLogic.Output.Expanded output) =>
            {
                ((CircleShape2D)CollisionShape2D.Shape).Radius = output.Radius;
                CollisionShape2D.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
                _drawRadius = output.Radius;
                QueueRedraw();
            })
            .OnOutput((in MagnetPulseLogic.Output.Ended _) =>
            {
                CollisionShape2D.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
                _drawRadius = 0f;
                QueueRedraw();
            });

        MagnetPulseLogic.Start<MagnetPulseState.Waiting>();
        SetPhysicsProcess(true);
    }

    public void OnPhysicsProcess(double delta)
    {
        MagnetPulseLogic.Input(new MagnetPulseLogic.Input.PhysicsProcess(delta));
    }

    public override void _Draw()
    {
        if (_drawRadius > 0f)
        {
            DrawArc(Vector2.Zero, _drawRadius, 0f, Mathf.Tau, 64, RingColor, RingWidth);
        }
    }

    public void OnExitTree()
    {
        MagnetPulseBinding.Dispose();
        ((System.IDisposable)MagnetPulseLogic).Dispose();
    }
}
