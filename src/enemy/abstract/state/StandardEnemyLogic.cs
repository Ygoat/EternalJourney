namespace EternalJourney.Enemy.Abstract.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Battle.Domain;
using EternalJourney.Bullet.Abstract.Base;
using EternalJourney.Enemy.Abstract.Base;
using Godot;



/// <summary>
/// エネミーロジックインターフェース
/// </summary>
public interface IStandardEnemyLogic : ILogicBlock<StandardEnemyLogic.State>;

/// <summary>
/// エネミーロジック
/// </summary>
[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class StandardEnemyLogic : LogicBlock<StandardEnemyLogic.State>, IStandardEnemyLogic
{
    /// <summary>
    /// 初期状態
    /// </summary>
    /// <returns></returns>
    public override Transition GetInitialState() => To<State.SpawnWait>();

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

    /// <summary>
    /// 状態定義
    /// </summary>
    public abstract record State : StateLogic<State>
    {
        /// <summary>
        /// スポーン待機
        /// </summary>
        public record SpawnWait : State, IGet<Input.Spawn>
        {
            public SpawnWait()
            {
            }

            public Transition On(in Input.Spawn input)
            {
                Output(new Output.StartInvade(input.SpawnGlobalPosition, input.SpawnGlobalAngle));
                return To<Invading>();
            }
        }

        /// <summary>
        /// 侵攻
        /// </summary>
        public record Invading : State, IGet<Input.PhysicsProcess>, IGet<Input.BulletHit>, IGet<Input.OutOfArea>
        {
            public Invading()
            {
            }

            public Transition On(in Input.PhysicsProcess input)
            {
                // 位置を更新
                Vector2 nextPositionDelta = input.Direction.Normalized() * input.Speed;
                IStandardEnemy standardEnemy = Get<IStandardEnemy>();
                Output(new Output.Move(nextPositionDelta));
                UpdateColor(standardEnemy.Status.CurrentDur, standardEnemy.Status.MaxDur);
                return CheckUnderZeroDurability(standardEnemy.Status.CurrentDur);
            }

            public Transition On(in Input.BulletHit input)
            {
                IBattleRepo battleRepo = Get<IBattleRepo>();
                IStandardEnemy standardEnemy = Get<IStandardEnemy>();
                float currentDur = battleRepo.ReduceEnemyDurability(standardEnemy.Status.CurrentDur, 1.0f);
                Output(new Output.CurrentDurChange(currentDur));
                UpdateColor(currentDur, standardEnemy.Status.MaxDur);
                return CheckUnderZeroDurability(currentDur);
            }

            public Transition On(in Input.OutOfArea input)
            {
                // 破壊を出力する
                Output(new Output.Destroyed());
                // スポーン待機に遷移する
                return To<SpawnWait>();
            }

            private Transition CheckUnderZeroDurability(float currentDur)
            {
                // 耐久値が0以下の場合
                if (currentDur <= 0)
                {
                    // 破壊を出力する
                    Output(new Output.Destroyed());
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

                Output(new Output.UpdateColor(result));
            }
        }
    }
}
