namespace EternalJourney.Bullet.Abstract.State;

using System;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Battle.Domain;
using EternalJourney.Bullet.Abstract.Base;
using EternalJourney.Bullet.Strategies.Collision;
using EternalJourney.Bullet.Strategies.Movement;
using EternalJourney.Cores.Models.Skill;
using EternalJourney.Enemy.Base;
using Godot;

/// <summary>
/// 弾丸ロジックインターフェース
/// </summary>
public interface IBulletLogic : ILogicBlock;

/// <summary>
/// 統一弾丸ロジック（移動・衝突をストラテジーに委譲）
/// </summary>
[Meta]
public partial class BulletLogic : LogicBlock, IBulletLogic
{
    public BulletLogic()
    {
        // ステートを事前確保
        Set(new BulletState.EmitWait());
        Set(new BulletState.InFlight());
        Set(new BulletState.Blast());
    }


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

        /// <summary>
        /// ターゲットヒット（Enemy以外：Shipなど）
        /// </summary>
        public readonly record struct TargetHit();
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

        /// <summary>
        /// SPD更新（バフ乗数適用後の値）
        /// </summary>
        public readonly record struct SpdUpdated(float Spd);
    }

}

/// <summary>
/// 状態定義
/// </summary>
[StateDiagram]
public abstract partial record BulletState : LogicBlockState
{
    /// <summary>
    /// 射出待機
    /// </summary>
    public record EmitWait : BulletState, IGet<BulletLogic.Input.Emit>
    {
        public EmitWait()
        {
        }

        public Type On(in BulletLogic.Input.Emit input)
        {
            // SPDバフ乗数を適用してOutput
            IBattleRepo battleRepo = Get<IBattleRepo>();
            IBaseBullet baseBullet = Get<IBaseBullet>();
            float spd = baseBullet.Status.Spd * battleRepo.BulletSpdMultiplier;
            Output(new BulletLogic.Output.SpdUpdated(spd));

            // 事前確保済みのInFlightステートに発射情報を設定してから遷移
            InFlight inFlight = Get<InFlight>();
            inFlight.ShotGlobalPosition = input.ShotGlobalPosition;
            inFlight.ShotGlobalAngle = input.ShotGlobalAngle;
            return To<InFlight>();
        }
    }

    /// <summary>
    /// 飛翔
    /// </summary>
    public record InFlight : BulletState, IGet<BulletLogic.Input.PhysicsProcess>, IGet<BulletLogic.Input.EnemyHit>, IGet<BulletLogic.Input.TargetHit>, IGet<BulletLogic.Input.Miss>
    {
        public Vector2 ShotGlobalPosition { get; set; }
        public float ShotGlobalAngle { get; set; }

        public InFlight()
        {
        }

        public Type On(in BulletLogic.Input.PhysicsProcess input)
        {
            // 移動ストラテジーに移動計算を委譲
            Vector2 nextPositionDelta = input.MovementStrategy.CalculateMovement(
                input.CurrentPosition,
                input.Direction,
                input.Speed,
                input.ElapsedTime
            );
            Output(new BulletLogic.Output.Move(nextPositionDelta));
            // 耐久値チェック
            IBaseBullet baseBullet = Get<IBaseBullet>();
            return CheckUnderZeroDurability(baseBullet.Status.CurrentDur);
        }

        public Type On(in BulletLogic.Input.EnemyHit input)
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
            Output(new BulletLogic.Output.CurrentDurChange(currentDur));
            // 耐久値チェック
            return CheckUnderZeroDurability(currentDur);
        }

        public Type On(in BulletLogic.Input.TargetHit input)
        {
            // EnemyHit と同様に耐久値を減少させる（ステータスエフェクトなし）
            IBattleRepo battleRepo = Get<IBattleRepo>();
            IBaseBullet baseBullet = Get<IBaseBullet>();
            IBulletCollisionStrategy collisionStrategy = baseBullet.CollisionStrategy;

            float durabilityCost = collisionStrategy.GetDurabilityCost();
            float currentDur = battleRepo.ReduceBulletDurability(baseBullet.Status.CurrentDur, durabilityCost);

            Output(new BulletLogic.Output.CurrentDurChange(currentDur));
            return CheckUnderZeroDurability(currentDur);
        }

        public Type On(in BulletLogic.Input.Miss input)
        {
            // 崩壊を出力して射出待機に遷移
            Output(new BulletLogic.Output.RemoveSelf());
            return To<EmitWait>();
        }

        private Type CheckUnderZeroDurability(float currentDur)
        {
            if (currentDur <= 0)
            {
                // 衝突ストラテジーに基づいて分岐
                IBulletCollisionStrategy collisionStrategy = Get<IBaseBullet>().CollisionStrategy;
                OnDepletedAction action = collisionStrategy.GetOnDepletedAction();

                Output(new BulletLogic.Output.Collapse());
                switch (action)
                {
                    case OnDepletedAction.Blast:
                        return To<Blast>();
                    default:
                        Output(new BulletLogic.Output.RemoveSelf());
                        return To<EmitWait>();
                }
            }
            return ToSelf();
        }
    }

    /// <summary>
    /// 爆風（衝突ストラテジーがBlastを返した場合のみ遷移）
    /// </summary>
    public record Blast : BulletState, IGet<BulletLogic.Input.BlastTimerTimeout>, IGet<BulletLogic.Input.EnemyHit>, IGet<BulletLogic.Input.TargetHit>
    {
        public Blast()
        {
        }

        public Type On(in BulletLogic.Input.BlastTimerTimeout input)
        {
            // 自ノード除去を出力
            Output(new BulletLogic.Output.RemoveSelf());
            // 射出待機に遷移
            return To<EmitWait>();
        }

        public Type On(in BulletLogic.Input.EnemyHit input)
        {
            return ToSelf();
        }

        public Type On(in BulletLogic.Input.TargetHit input)
        {
            return ToSelf();
        }
    }
}
