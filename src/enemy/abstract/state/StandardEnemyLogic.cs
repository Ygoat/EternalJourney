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
        /// 破壊
        /// </summary>
        public readonly record struct Destroyed;

        /// <summary>
        /// 移動
        /// </summary>
        /// <param name="NextPositionDelta"></param>
        public readonly record struct Move(Vector2 NextPositionDelta);

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
                Input.Spawn ip = input;
                return To<Invading>().With(
                    (state) =>
                    {
                        ((Invading)state).SpawnGlobalPosition = ip.SpawnGlobalPosition;
                        ((Invading)state).SpawnGlobalAngle = ip.SpawnGlobalAngle;
                    }
                );
            }
        }

        /// <summary>
        /// 侵攻
        /// </summary>
        public record Invading : State, IGet<Input.PhysicsProcess>, IGet<Input.BulletHit>, IGet<Input.OutOfArea>
        {
            public Vector2 SpawnGlobalPosition { get; set; }
            public float SpawnGlobalAngle { get; set; }

            public Invading()
            {
            }

            public Transition On(in Input.PhysicsProcess input)
            {
                // 位置を更新
                Vector2 nextPositionDelta = input.Direction.Normalized() * input.Speed;
                Output(new Output.Move(nextPositionDelta));
                return ToSelf();
            }

            public Transition On(in Input.BulletHit input)
            {
                IBattleRepo battleRepo = Get<IBattleRepo>();
                IBaseEnemy baseEnemy = Get<IBaseEnemy>();
                battleRepo.EnemyDamagedByBullet(baseEnemy, input.BaseBullet);
                // 耐久値が0以下の場合
                if (baseEnemy.Status.CurrentDur <= 0)
                {
                    // 破壊を出力する
                    Output(new Output.Destroyed());
                    // スポーン待機に遷移する
                    return To<SpawnWait>();
                }
                return ToSelf();
            }

            public Transition On(in Input.OutOfArea input)
            {
                // 破壊を出力する
                Output(new Output.Destroyed());
                // スポーン待機に遷移する
                return To<SpawnWait>();
            }
        }
    }
}
