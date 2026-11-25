namespace EternalJourney.Enemy.Orbit.State;

using System;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Battle.Domain;
using EternalJourney.Bullet.Abstract.Base;
using EternalJourney.Ship;
using Godot;

/// <summary>
/// 周回エネミーロジックインターフェース
/// </summary>
public interface IOrbitEnemyLogic : ILogicBlock;

/// <summary>
/// 周回エネミーロジック
/// 画面外からShipへ一直線に接近 → 一定距離を保って周回しながら攻撃 → 画面外へ逃走して消滅
/// </summary>
[Meta]
public partial class OrbitEnemyLogic : LogicBlock, IOrbitEnemyLogic
{
    public OrbitEnemyLogic()
    {
        // ステートを事前確保
        Set(new OrbitEnemyState.SpawnWait());
        Set(new OrbitEnemyState.Approaching());
        Set(new OrbitEnemyState.Orbiting());
        Set(new OrbitEnemyState.Fleeing());
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
        /// 弾丸ヒット
        /// </summary>
        public readonly record struct BulletHit(IBaseBullet BaseBullet);

        /// <summary>
        /// 画面外に出た
        /// </summary>
        public readonly record struct OutOfArea;

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
        /// 侵攻開始
        /// </summary>
        public readonly record struct StartInvade(Vector2 SpawnGlobalPosition, float SpawnGlobalAngle);

        /// <summary>
        /// 耐久値変化
        /// </summary>
        public readonly record struct CurrentDurChange(float CurrentDur);

        /// <summary>
        /// 移動
        /// </summary>
        public readonly record struct Move(Vector2 PositionDelta);

        /// <summary>
        /// カラー更新
        /// </summary>
        public readonly record struct UpdateColor(Color Color);

        /// <summary>
        /// 武器の有効・無効
        /// </summary>
        public readonly record struct WeaponEnabled(bool Enabled);

        /// <summary>
        /// 撃破された（除去する）
        /// </summary>
        public readonly record struct Destroyed;

        /// <summary>
        /// 画面外へ逃げ切った（撃破扱いにせず除去する）
        /// </summary>
        public readonly record struct Escaped;
    }
}

/// <summary>
/// 周回エネミー状態定義
/// </summary>
[StateDiagram]
public abstract partial record OrbitEnemyState : LogicBlockState
{
    /// <summary>
    /// スポーン待機
    /// </summary>
    public record SpawnWait : OrbitEnemyState, IGet<OrbitEnemyLogic.Input.Spawn>
    {
        public Type On(in OrbitEnemyLogic.Input.Spawn input)
        {
            Vector2 dir = input.SpawnGlobalPosition.DirectionTo(Get<IShip>().EnemyTargetMarker.GlobalPosition);
            Output(new OrbitEnemyLogic.Output.StartInvade(input.SpawnGlobalPosition, dir.Angle()));
            return To<Approaching>();
        }
    }

    /// <summary>
    /// 生存中の共通処理（被弾と撃破判定）
    /// </summary>
    public abstract record Alive : OrbitEnemyState, IGet<OrbitEnemyLogic.Input.BulletHit>
    {
        public Type On(in OrbitEnemyLogic.Input.BulletHit input)
        {
            IOrbitEnemy enemy = Get<IOrbitEnemy>();
            float currentDur = Get<IBattleRepo>().ReduceEnemyDurability(
                enemy.Status.CurrentDur,
                input.BaseBullet.Status.Atk * input.BaseBullet.DamageMultiplier,
                input.BaseBullet.Status.Def,
                enemy.Status.Def);
            Output(new OrbitEnemyLogic.Output.CurrentDurChange(currentDur));
            UpdateColor(currentDur, enemy.Status.MaxDur);
            return currentDur <= 0 ? Kill() : ToSelf();
        }

        /// <summary>
        /// 毒などで耐久値が尽きていたら撃破する。生きていればnullを返す
        /// </summary>
        protected Type? KillIfDepleted()
        {
            IOrbitEnemy enemy = Get<IOrbitEnemy>();
            UpdateColor(enemy.Status.CurrentDur, enemy.Status.MaxDur);
            return enemy.Status.CurrentDur <= 0 ? Kill() : null;
        }

        /// <summary>
        /// 撃破処理（スコア加算・撃破通知）
        /// </summary>
        private Type Kill()
        {
            Output(new OrbitEnemyLogic.Output.Destroyed());

            IBattleRepo battleRepo = Get<IBattleRepo>();
            IOrbitEnemy enemy = Get<IOrbitEnemy>();
            battleRepo.ScoreCountUp(enemy.ScoreValue);
            battleRepo.OnEnemyDestroyed(enemy);
            return To<SpawnWait>();
        }

        private void UpdateColor(float currentDur, float maxDur)
        {
            // 耐久割合（0〜1）
            float ratio = Mathf.Clamp(currentDur / maxDur, 0f, 1f);
            // 赤（Full）から青（Zero）へ補間
            Color result = new Color(1f, 0f, 0f).Lerp(new Color(0f, 0f, 1f), 1f - ratio);
            Output(new OrbitEnemyLogic.Output.UpdateColor(result));
        }
    }

    /// <summary>
    /// 接近（Shipへ一直線）
    /// </summary>
    public record Approaching : Alive, IGet<OrbitEnemyLogic.Input.PhysicsProcess>
    {
        public Approaching()
        {
            // 周回に入るまでは攻撃しない
            this.OnEnter(() => Output(new OrbitEnemyLogic.Output.WeaponEnabled(false)));
        }

        public Type On(in OrbitEnemyLogic.Input.PhysicsProcess input)
        {
            Type? killed = KillIfDepleted();
            if (killed != null)
            {
                return killed;
            }

            IOrbitEnemy enemy = Get<IOrbitEnemy>();
            Vector2 center = Get<IShip>().CenterMarker.GlobalPosition;
            Vector2 position = enemy.GlobalPosition;

            // 周回半径まで近づいたら周回を始める
            if (position.DistanceTo(center) <= enemy.OrbitRadius)
            {
                return To<Orbiting>();
            }

            Output(new OrbitEnemyLogic.Output.Move(
                OrbitMath.ApproachStep(position, center, enemy.ApproachSpeed, (float)input.Delta)));
            return ToSelf();
        }
    }

    /// <summary>
    /// 周回（Shipとの距離を保って円形に移動し、攻撃する）
    /// </summary>
    public record Orbiting : Alive, IGet<OrbitEnemyLogic.Input.PhysicsProcess>
    {
        /// <summary>
        /// Shipから見た現在角度（OnEnterで初期化する）
        /// </summary>
        private float _angle;

        /// <summary>
        /// 周回経過時間（OnEnterで初期化する）
        /// </summary>
        private float _elapsed;

        public Orbiting()
        {
            this.OnEnter(() =>
            {
                _elapsed = 0f;
                // 進入した位置から途切れなく周回を始める
                _angle = OrbitMath.AngleOf(
                    Get<IShip>().CenterMarker.GlobalPosition,
                    Get<IOrbitEnemy>().GlobalPosition);
                Output(new OrbitEnemyLogic.Output.WeaponEnabled(true));
            });
        }

        public Type On(in OrbitEnemyLogic.Input.PhysicsProcess input)
        {
            Type? killed = KillIfDepleted();
            if (killed != null)
            {
                return killed;
            }

            IOrbitEnemy enemy = Get<IOrbitEnemy>();
            float delta = (float)input.Delta;

            // 周回時間が終わったら逃走する
            _elapsed += delta;
            if (_elapsed >= enemy.OrbitDuration)
            {
                return To<Fleeing>();
            }

            // 円周上を進み、現在位置との差を移動量にする
            float direction = enemy.OrbitClockwise ? 1f : -1f;
            _angle += direction * Mathf.DegToRad(enemy.OrbitAngularSpeed) * delta;
            Vector2 target = OrbitMath.OrbitPosition(
                Get<IShip>().CenterMarker.GlobalPosition, enemy.OrbitRadius, _angle);
            Output(new OrbitEnemyLogic.Output.Move(target - enemy.GlobalPosition));
            return ToSelf();
        }
    }

    /// <summary>
    /// 逃走（Shipから離れて画面外へ。逃げ切っても撃破にはならない）
    /// </summary>
    public record Fleeing : Alive,
        IGet<OrbitEnemyLogic.Input.PhysicsProcess>,
        IGet<OrbitEnemyLogic.Input.OutOfArea>
    {
        public Fleeing()
        {
            this.OnEnter(() => Output(new OrbitEnemyLogic.Output.WeaponEnabled(false)));
        }

        public Type On(in OrbitEnemyLogic.Input.PhysicsProcess input)
        {
            Type? killed = KillIfDepleted();
            if (killed != null)
            {
                return killed;
            }

            IOrbitEnemy enemy = Get<IOrbitEnemy>();
            Output(new OrbitEnemyLogic.Output.Move(OrbitMath.FleeStep(
                enemy.GlobalPosition,
                Get<IShip>().CenterMarker.GlobalPosition,
                enemy.FleeSpeed,
                (float)input.Delta)));
            return ToSelf();
        }

        public Type On(in OrbitEnemyLogic.Input.OutOfArea input)
        {
            // 撃破扱いにしない（スコア・撃破通知・スクラップなし）
            Output(new OrbitEnemyLogic.Output.Escaped());
            return To<SpawnWait>();
        }
    }
}
