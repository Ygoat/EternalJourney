namespace EternalJourney.Weapon.Continuous.State;

using System;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

/// <summary>
/// 継続照射武器ロジックインターフェース
/// </summary>
public interface IContinuousWeaponLogic : ILogicBlock;

/// <summary>
/// 継続照射武器ロジック（レーザー・放射武器の共通ロジック）
/// ターゲットがいる間照射を続け、到達距離を一定速度で伸ばし、一定間隔でダメージを与える
/// </summary>
[Meta]
public partial class ContinuousWeaponLogic : LogicBlock, IContinuousWeaponLogic
{
    public ContinuousWeaponLogic()
    {
        // ステートを事前確保
        Set(new ContinuousWeaponState.Idle());
        Set(new ContinuousWeaponState.Channeling());
    }

    /// <summary>
    /// 照射設定
    /// </summary>
    /// <param name="ExtendSpeed">到達距離の伸びる速さ（武器ローカル座標/秒）</param>
    /// <param name="MaxReach">最大到達距離（武器ローカル座標）</param>
    /// <param name="DamageInterval">ダメージ間隔（秒）</param>
    public record Settings(float ExtendSpeed, float MaxReach, float DamageInterval);

    /// <summary>
    /// 入力定義
    /// </summary>
    public static class Input
    {
        /// <summary>
        /// 照射開始（ターゲット発見）
        /// </summary>
        public readonly record struct StartChannel;

        /// <summary>
        /// 照射停止（ターゲット消失）
        /// </summary>
        public readonly record struct StopChannel;

        /// <summary>
        /// 物理処理
        /// </summary>
        /// <param name="Delta">経過時間（秒）</param>
        /// <param name="ObstructionDistance">先頭を遮る障害物までの距離。遮られない場合は無限大</param>
        public readonly record struct PhysicsProcess(double Delta, float ObstructionDistance);
    }

    /// <summary>
    /// 出力定義
    /// </summary>
    public static class Output
    {
        /// <summary>
        /// 待機に入った
        /// </summary>
        public readonly record struct Idling;

        /// <summary>
        /// 照射に入った
        /// </summary>
        public readonly record struct Channeling;

        /// <summary>
        /// 到達距離の更新
        /// </summary>
        public readonly record struct ReachChanged(float Reach);

        /// <summary>
        /// ダメージ発生タイミング
        /// </summary>
        public readonly record struct DamageTick;
    }
}

/// <summary>
/// 継続照射武器状態定義
/// </summary>
[StateDiagram]
public abstract partial record ContinuousWeaponState : LogicBlockState
{
    /// <summary>
    /// 待機（ターゲットなし）
    /// </summary>
    public record Idle : ContinuousWeaponState, IGet<ContinuousWeaponLogic.Input.StartChannel>
    {
        public Idle()
        {
            this.OnEnter(() => Output(new ContinuousWeaponLogic.Output.Idling()));
        }

        public Type On(in ContinuousWeaponLogic.Input.StartChannel input) => To<Channeling>();
    }

    /// <summary>
    /// 照射中
    /// </summary>
    public record Channeling : ContinuousWeaponState,
        IGet<ContinuousWeaponLogic.Input.StopChannel>,
        IGet<ContinuousWeaponLogic.Input.PhysicsProcess>
    {
        /// <summary>
        /// 現在の到達距離（OnEnterで初期化する）
        /// </summary>
        private float _reach;

        /// <summary>
        /// 前回ダメージからの経過時間（OnEnterで初期化する）
        /// </summary>
        private float _damageElapsed;

        public Channeling()
        {
            this.OnEnter(() =>
            {
                _reach = 0f;
                _damageElapsed = 0f;
                Output(new ContinuousWeaponLogic.Output.Channeling());
            });
        }

        public Type On(in ContinuousWeaponLogic.Input.StopChannel input) => To<Idle>();

        public Type On(in ContinuousWeaponLogic.Input.PhysicsProcess input)
        {
            ContinuousWeaponLogic.Settings settings = Get<ContinuousWeaponLogic.Settings>();
            float delta = (float)input.Delta;

            // 到達距離は一定速度で伸び、最大距離と障害物の手前で止まる
            _reach = Math.Min(_reach + settings.ExtendSpeed * delta, settings.MaxReach);
            _reach = Math.Min(_reach, input.ObstructionDistance);
            Output(new ContinuousWeaponLogic.Output.ReachChanged(_reach));

            _damageElapsed += delta;
            if (_damageElapsed >= settings.DamageInterval)
            {
                _damageElapsed -= settings.DamageInterval;
                Output(new ContinuousWeaponLogic.Output.DamageTick());
            }
            return ToSelf();
        }
    }
}
