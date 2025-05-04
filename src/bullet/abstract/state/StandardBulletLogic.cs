namespace EternalJourney.Bullet.Abstract.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Battle.Domain;
using EternalJourney.Bullet.Abstract.Base;
using EternalJourney.Enemy.Abstract.Base;
using Godot;




/// <summary>
/// 弾丸ロジックインターフェース
/// </summary>
public interface IStandardBulletLogic : ILogicBlock<StandardBulletLogic.State>;

/// <summary>
/// 弾丸ロジック
/// </summary>
[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class StandardBulletLogic : LogicBlock<StandardBulletLogic.State>, IStandardBulletLogic
{
    /// <summary>
    /// 初期状態
    /// </summary>
    /// <returns></returns>
    public override Transition GetInitialState() => To<State.EmitWait>();

    /// <summary>
    /// 入力定義
    /// </summary>
    public static class Input
    {
        /// <summary>
        /// 射撃
        /// </summary>
        public readonly record struct Emit(Vector2 ShotGlobalPosition, float ShotGlobalAngle);

        /// <summary>
        /// ヒット
        /// </summary>
        public readonly record struct EnemyHit(IBaseEnemy BaseEnemy);

        /// <summary>
        /// ミス
        /// </summary>
        public readonly record struct Miss;

        /// <summary>
        /// 物理処理
        /// </summary>
        /// <param name="Direction"></param>
        /// <param name="Speed"></param>
        public readonly record struct PhysicsProcess(Vector2 Direction, float Speed);
    }

    /// <summary>
    /// 出力定義
    /// </summary>
    public static class Output
    {
        /// <summary>
        /// 崩壊
        /// </summary>
        public readonly record struct Collapse;

        /// <summary>
        /// 移動
        /// </summary>
        public readonly record struct Move(Vector2 NextPositionDelta);
    }

    /// <summary>
    /// 状態定義
    /// </summary>
    public abstract record State : StateLogic<State>
    {
        /// <summary>
        /// ロード
        /// </summary>
        public record EmitWait : State, IGet<Input.Emit>
        {
            public EmitWait()
            {
            }

            public Transition On(in Input.Emit input)
            {
                Input.Emit ip = input;
                return To<InFlight>().With(
                    (state) =>
                    {
                        ((InFlight)state).ShotGlobalPosition = ip.ShotGlobalPosition;
                        ((InFlight)state).ShotGlobalAngle = ip.ShotGlobalAngle;
                    }
                );

            }
        }

        /// <summary>
        /// 飛翔
        /// </summary>
        public record InFlight : State, IGet<Input.PhysicsProcess>, IGet<Input.EnemyHit>, IGet<Input.Miss>
        {
            public Vector2 ShotGlobalPosition { get; set; }
            public float ShotGlobalAngle { get; set; }

            public InFlight()
            {
            }

            public Transition On(in Input.PhysicsProcess input)
            {
                // 位置を更新
                Vector2 nextPositionDelta = input.Direction.Normalized() * input.Speed;
                Output(new Output.Move(nextPositionDelta));
                return ToSelf();
            }

            public Transition On(in Input.EnemyHit input)
            {
                IBattleRepo battleRepo = Get<IBattleRepo>();
                IBaseBullet baseBullet = Get<IBaseBullet>();
                battleRepo.BulletDamagedByEnemy(baseBullet, input.BaseEnemy);
                // 耐久値が0以下の場合
                if (baseBullet.Status.CurrentDur <= 0)
                {
                    // 崩壊を出力する
                    Output(new Output.Collapse());
                    // 射出待機に遷移する
                    return To<EmitWait>();
                }
                return ToSelf();
            }

            public Transition On(in Input.Miss input)
            {
                // 崩壊を出力する
                Output(new Output.Collapse());
                // 射出待機に遷移する
                return To<EmitWait>();
            }
        }
    }
}
