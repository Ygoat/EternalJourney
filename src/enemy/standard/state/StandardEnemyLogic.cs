namespace EternalJourney.Enemy.Standard.State;

using System;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Battle.Domain;
using EternalJourney.Bullet.Base;
using EternalJourney.Enemy.Standard;
using EternalJourney.Enemy.Strategies.Movement;
using EternalJourney.Ship;
using Godot;



/// <summary>
/// エネミーロジックインターフェース
/// </summary>
public interface IStandardEnemyLogic : ILogicBlock;

/// <summary>
/// エネミーロジック
/// </summary>
[Meta]
public partial class StandardEnemyLogic : LogicBlock, IStandardEnemyLogic
{
    public StandardEnemyLogic()
    {
        // ステートを事前確保
        Set(new StandardEnemyState.SpawnWait());
        Set(new StandardEnemyState.Invading());
    }


    /// <summary>
    /// 入力定義
    /// </summary>
    public static class Input
    {
        /// <summary>
        /// スポーン
        /// </summary>
        public readonly record struct Spawn(Vector2 SpawnGlobalPosition, float SpawnGlobalAngle);

        /// <summary>
        /// ターゲット検知
        /// </summary>
        public readonly record struct Detect;

        /// <summary>
        /// ヒット
        /// </summary>
        public readonly record struct BulletHit(IBaseBullet BaseBullet);

        /// <summary>
        /// エリア外
        /// </summary>
        public readonly record struct OutOfArea;


        /// <summary>
        /// 物理処理
        /// </summary>
        /// <param name="Direction">方向</param>
        /// <param name="Speed">速度</param>
        /// <param name="ElapsedTime">経過時間</param>
        /// <param name="MovementStrategy">移動戦略</param>
        /// <param name="CurrentPosition">現在位置</param>
        public readonly record struct PhysicsProcess(
            Vector2 Direction,
            float Speed,
            float ElapsedTime,
            IMovementStrategy MovementStrategy,
            Vector2 CurrentPosition);
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
        /// 侵攻開始
        /// </summary>
        /// <param name="SpawnGlobalPosition"></param>
        /// <param name="SpawnGlobalAngle"></param>
        public readonly record struct StartInvade(Vector2 SpawnGlobalPosition, float SpawnGlobalAngle);

        /// <summary>
        /// 破壊
        /// </summary>
        public readonly record struct Destroyed;

        /// <summary>
        /// 移動
        /// </summary>
        /// <param name="NextPositionDelta"></param>
        public readonly record struct Move(Vector2 NextPositionDelta);

        /// <summary>
        /// カラー更新
        /// </summary>
        public readonly record struct UpdateColor(Color Color);

    }

}

/// <summary>
/// 状態定義
/// </summary>
[StateDiagram]
public abstract partial record StandardEnemyState : LogicBlockState
{
    /// <summary>
    /// スポーン待機
    /// </summary>
    public record SpawnWait : StandardEnemyState, IGet<StandardEnemyLogic.Input.Spawn>
    {
        public SpawnWait()
        {
        }

        public Type On(in StandardEnemyLogic.Input.Spawn input)
        {
            IShip ship = Get<IShip>();
            Vector2 dir = input.SpawnGlobalPosition.DirectionTo(ship.EnemyTargetMarker.GlobalPosition);
            Output(new StandardEnemyLogic.Output.StartInvade(input.SpawnGlobalPosition, dir.Angle()));
            return To<Invading>();
        }
    }

    /// <summary>
    /// 侵攻
    /// </summary>
    public record Invading : StandardEnemyState, IGet<StandardEnemyLogic.Input.PhysicsProcess>, IGet<StandardEnemyLogic.Input.BulletHit>, IGet<StandardEnemyLogic.Input.OutOfArea>
    {
        public Invading()
        {
        }

        public Type On(in StandardEnemyLogic.Input.PhysicsProcess input)
        {
            // 移動戦略を使用して位置を更新
            Vector2 nextPositionDelta = input.MovementStrategy.CalculateMovement(
                input.CurrentPosition,
                input.Direction,
                input.Speed,
                input.ElapsedTime);
            IStandardEnemy standardEnemy = Get<IStandardEnemy>();
            Output(new StandardEnemyLogic.Output.Move(nextPositionDelta));
            UpdateColor(standardEnemy.Status.CurrentDur, standardEnemy.Status.MaxDur);
            return CheckUnderZeroDurability(standardEnemy.Status.CurrentDur);
        }

        public Type On(in StandardEnemyLogic.Input.BulletHit input)
        {
            IBattleRepo battleRepo = Get<IBattleRepo>();
            IStandardEnemy standardEnemy = Get<IStandardEnemy>();
            float currentDur = battleRepo.ReduceEnemyDurability(
                standardEnemy.Status.CurrentDur,
                input.BaseBullet.Status.Atk * input.BaseBullet.DamageMultiplier,
                input.BaseBullet.Status.Def,
                standardEnemy.Status.Def
            );
            Output(new StandardEnemyLogic.Output.CurrentDurChange(currentDur));
            UpdateColor(currentDur, standardEnemy.Status.MaxDur);
            return CheckUnderZeroDurability(currentDur);
        }

        public Type On(in StandardEnemyLogic.Input.OutOfArea input)
        {
            // 破壊を出力する
            Output(new StandardEnemyLogic.Output.Destroyed());
            // スポーン待機に遷移する
            return To<SpawnWait>();
        }

        private Type CheckUnderZeroDurability(float currentDur)
        {
            // 耐久値が0以下の場合
            if (currentDur <= 0)
            {
                // 破壊を出力する
                Output(new StandardEnemyLogic.Output.Destroyed());

                // スコアをカウントアップさせる（JSONの設定値を使用）
                IBattleRepo battleRepo = Get<IBattleRepo>();
                IStandardEnemy standardEnemy = Get<IStandardEnemy>();
                battleRepo.ScoreCountUp(standardEnemy.ScoreValue);
                battleRepo.OnEnemyDestroyed(standardEnemy);

                // スポーン待機に遷移する
                return To<SpawnWait>();
            }
            return ToSelf();
        }

        private void UpdateColor(float currentDur, float maxDur)
        {
            // 耐久割合（0〜1）
            float ratio = Mathf.Clamp(currentDur / maxDur, 0f, 1f);
            // 赤（Full）から青（Zero）へ補間
            Color fullColor = new Color(1f, 0f, 0f);  // 赤
            Color emptyColor = new Color(0f, 0f, 1f); // 青
            Color result = fullColor.Lerp(emptyColor, 1f - ratio);

            Output(new StandardEnemyLogic.Output.UpdateColor(result));
        }
    }
}
