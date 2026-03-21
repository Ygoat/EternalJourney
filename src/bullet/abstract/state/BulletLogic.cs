namespace EternalJourney.Bullet.Abstract.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Battle.Domain;
using EternalJourney.Bullet.Abstract.Base;
using EternalJourney.Bullet.Strategies.Collision;
using EternalJourney.Bullet.Strategies.Movement;
using EternalJourney.Enemy.Base;
using Godot;

/// <summary>
/// 弾丸ロジックインターフェース
/// </summary>
public interface IBulletLogic : ILogicBlock<BulletLogic.State>;

/// <summary>
/// 統一弾丸ロジック（移動・衝突をストラテジーに委譲）
/// </summary>
[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class BulletLogic : LogicBlock<BulletLogic.State>, IBulletLogic
{
    /// <summary>
    /// 初期状態
    /// </summary>
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
        public readonly record struct PhysicsProcess(
            Vector2 Direction,
            float Speed,
            float ElapsedTime,
            IBulletMovementStrategy MovementStrategy,
            Vector2 CurrentPosition
        );

        /// <summary>
        /// 爆風タイムアウト
        /// </summary>
        public readonly record struct BlastTimerTimeout();
    }

    /// <summary>
    /// 出力定義
    /// </summary>
    public static class Output
    {
        /// <summary>
        /// 耐久値変化
        /// </summary>
        public readonly record struct CurrentDurChange(float CurrentDur);

        /// <summary>
        /// 崩壊
        /// </summary>
        public readonly record struct Collapse;

        /// <summary>
        /// 移動
        /// </summary>
        public readonly record struct Move(Vector2 NextPositionDelta);

        /// <summary>
        /// 自ノード除去
        /// </summary>
        public readonly record struct RemoveSelf();
    }

    /// <summary>
    /// 状態定義
    /// </summary>
    public abstract record State : StateLogic<State>
    {
        /// <summary>
        /// 射出待機
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
                // 移動ストラテジーに移動計算を委譲
                Vector2 nextPositionDelta = input.MovementStrategy.CalculateMovement(
                    input.CurrentPosition,
                    input.Direction,
                    input.Speed,
                    input.ElapsedTime
                );
                Output(new Output.Move(nextPositionDelta));
                // 耐久値チェック
                IBaseBullet baseBullet = Get<IBaseBullet>();
                return CheckUnderZeroDurability(baseBullet.Status.CurrentDur);
            }

            public Transition On(in Input.EnemyHit input)
            {
                // 衝突ストラテジーから耐久コストを取得
                IBattleRepo battleRepo = Get<IBattleRepo>();
                IBaseBullet baseBullet = Get<IBaseBullet>();
                IBulletCollisionStrategy collisionStrategy = baseBullet.CollisionStrategy;

                float durabilityCost = collisionStrategy.GetDurabilityCost();
                float currentDur = battleRepo.ReduceBulletDurability(baseBullet.Status.CurrentDur, durabilityCost);

                // 衝突ストラテジーに基づいてステータスエフェクト適用
                if (collisionStrategy.ShouldApplyStatusEffects())
                {
                    baseBullet.StatusEffectServerManager.Apply(input.BaseEnemy.StatusEffectReceiverManager);
                }

                // 耐久値変更を通知
                Output(new Output.CurrentDurChange(currentDur));
                // 耐久値チェック
                return CheckUnderZeroDurability(currentDur);
            }

            public Transition On(in Input.Miss input)
            {
                // 崩壊を出力して射出待機に遷移
                Output(new Output.RemoveSelf());
                return To<EmitWait>();
            }

            private Transition CheckUnderZeroDurability(float currentDur)
            {
                if (currentDur <= 0)
                {
                    // 衝突ストラテジーに基づいて分岐
                    IBulletCollisionStrategy collisionStrategy = Get<IBaseBullet>().CollisionStrategy;
                    OnDepletedAction action = collisionStrategy.GetOnDepletedAction();

                    Output(new Output.Collapse());
                    switch (action)
                    {
                        case OnDepletedAction.Blast:
                            return To<Blast>();
                        default:
                            Output(new Output.RemoveSelf());
                            return To<EmitWait>();
                    }
                }
                return ToSelf();
            }
        }

        /// <summary>
        /// 爆風（衝突ストラテジーがBlastを返した場合のみ遷移）
        /// </summary>
        public record Blast : State, IGet<Input.BlastTimerTimeout>, IGet<Input.EnemyHit>
        {
            public Blast()
            {
            }

            public Transition On(in Input.BlastTimerTimeout input)
            {
                // 自ノード除去を出力
                Output(new Output.RemoveSelf());
                // 射出待機に遷移
                return To<EmitWait>();
            }

            public Transition On(in Input.EnemyHit input)
            {
                return ToSelf();
            }
        }
    }
}
