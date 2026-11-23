namespace EternalJourney.MagnetPulse.State;

using System;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

/// <summary>
/// 磁気パルスロジックインターフェース
/// </summary>
public interface IMagnetPulseLogic : ILogicBlock;

/// <summary>
/// 磁気パルスロジック（一定間隔で円状のパルスを広げる）
/// </summary>
[Meta]
public partial class MagnetPulseLogic : LogicBlock, IMagnetPulseLogic
{
    public MagnetPulseLogic()
    {
        // ステートを事前確保
        Set(new MagnetPulseState.Waiting());
        Set(new MagnetPulseState.Expanding());
    }

    /// <summary>
    /// パルス設定
    /// </summary>
    /// <param name="Interval">発射間隔（秒）</param>
    /// <param name="MaxRadius">最大半径（px）</param>
    /// <param name="ExpandSpeed">拡大速度（px/秒）</param>
    public record Settings(float Interval, float MaxRadius, float ExpandSpeed);

    /// <summary>
    /// 入力定義
    /// </summary>
    public static class Input
    {
        /// <summary>
        /// 物理処理
        /// </summary>
        public readonly record struct PhysicsProcess(double Delta);
    }

    /// <summary>
    /// 出力定義
    /// </summary>
    public static class Output
    {
        /// <summary>
        /// 半径更新（パルスが広がっている）
        /// </summary>
        public readonly record struct Expanded(float Radius);

        /// <summary>
        /// パルス終了（次の発射まで消える）
        /// </summary>
        public readonly record struct Ended;
    }
}

/// <summary>
/// 磁気パルス状態定義
/// </summary>
[StateDiagram]
public abstract partial record MagnetPulseState : LogicBlockState
{
    /// <summary>
    /// 次の発射までの待機
    /// </summary>
    public record Waiting : MagnetPulseState, IGet<MagnetPulseLogic.Input.PhysicsProcess>
    {
        /// <summary>
        /// 待機経過時間（OnEnterで初期化する）
        /// </summary>
        private float _elapsed;

        public Waiting()
        {
            this.OnEnter(() =>
            {
                _elapsed = 0f;
                Output(new MagnetPulseLogic.Output.Ended());
            });
        }

        public Type On(in MagnetPulseLogic.Input.PhysicsProcess input)
        {
            _elapsed += (float)input.Delta;
            return _elapsed >= Get<MagnetPulseLogic.Settings>().Interval
                ? To<Expanding>()
                : ToSelf();
        }
    }

    /// <summary>
    /// パルス拡大中
    /// </summary>
    public record Expanding : MagnetPulseState, IGet<MagnetPulseLogic.Input.PhysicsProcess>
    {
        /// <summary>
        /// 現在の半径（OnEnterで初期化する）
        /// </summary>
        private float _radius;

        public Expanding()
        {
            this.OnEnter(() => _radius = 0f);
        }

        public Type On(in MagnetPulseLogic.Input.PhysicsProcess input)
        {
            MagnetPulseLogic.Settings settings = Get<MagnetPulseLogic.Settings>();
            _radius += settings.ExpandSpeed * (float)input.Delta;

            if (_radius >= settings.MaxRadius)
            {
                return To<Waiting>();
            }

            Output(new MagnetPulseLogic.Output.Expanded(_radius));
            return ToSelf();
        }
    }
}
