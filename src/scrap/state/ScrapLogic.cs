namespace EternalJourney.Scrap.State;

using System;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Battle.Domain;
using EternalJourney.Ship;
using Godot;

/// <summary>
/// スクラップロジックインターフェース
/// </summary>
public interface IScrapLogic : ILogicBlock;

/// <summary>
/// スクラップロジック
/// </summary>
[Meta]
public partial class ScrapLogic : LogicBlock, IScrapLogic
{
    public ScrapLogic()
    {
        // ステートを事前確保
        Set(new ScrapState.Idle());
        Set(new ScrapState.Drifting());
        Set(new ScrapState.Attracted());
    }

    /// <summary>
    /// 入力定義
    /// </summary>
    public static class Input
    {
        /// <summary>
        /// ドロップ（撃破位置に出現する）
        /// </summary>
        public readonly record struct Drop(Vector2 GlobalPosition);

        /// <summary>
        /// 物理処理
        /// </summary>
        public readonly record struct PhysicsProcess(double Delta);

        /// <summary>
        /// 磁気パルスに触れた
        /// </summary>
        public readonly record struct PulseTouched;

        /// <summary>
        /// Shipに触れた
        /// </summary>
        public readonly record struct ShipTouched;
    }

    /// <summary>
    /// 出力定義
    /// </summary>
    public static class Output
    {
        /// <summary>
        /// 出現位置の設定
        /// </summary>
        public readonly record struct Place(Vector2 GlobalPosition);

        /// <summary>
        /// 移動
        /// </summary>
        public readonly record struct Move(Vector2 PositionDelta);

        /// <summary>
        /// 除去（プールへ返却する）
        /// </summary>
        public readonly record struct RemoveSelf;
    }
}

/// <summary>
/// スクラップ状態定義
/// </summary>
[StateDiagram]
public abstract partial record ScrapState : LogicBlockState
{
    /// <summary>
    /// 待機（プール内、またはドロップ待ち）
    /// </summary>
    public record Idle : ScrapState, IGet<ScrapLogic.Input.Drop>
    {
        public Type On(in ScrapLogic.Input.Drop input)
        {
            Output(new ScrapLogic.Output.Place(input.GlobalPosition));
            return To<Drifting>();
        }
    }

    /// <summary>
    /// 下に流れている
    /// </summary>
    public record Drifting : ScrapState,
        IGet<ScrapLogic.Input.PhysicsProcess>,
        IGet<ScrapLogic.Input.PulseTouched>,
        IGet<ScrapLogic.Input.ShipTouched>
    {
        public Type On(in ScrapLogic.Input.PhysicsProcess input)
        {
            IScrap scrap = Get<IScrap>();
            Output(new ScrapLogic.Output.Move(Vector2.Down * scrap.FallSpeed * (float)input.Delta));

            // 画面下に流れ切ったら回収されずに消える
            if (scrap.GlobalPosition.Y > scrap.DespawnY)
            {
                Output(new ScrapLogic.Output.RemoveSelf());
                return To<Idle>();
            }
            return ToSelf();
        }

        public Type On(in ScrapLogic.Input.PulseTouched input) => To<Attracted>();

        public Type On(in ScrapLogic.Input.ShipTouched input) => Collect();
    }

    /// <summary>
    /// Shipに吸い寄せられている
    /// </summary>
    public record Attracted : ScrapState,
        IGet<ScrapLogic.Input.PhysicsProcess>,
        IGet<ScrapLogic.Input.ShipTouched>
    {
        /// <summary>
        /// 現在の吸引速度（OnEnterで初期化する）
        /// </summary>
        private float _speed;

        public Attracted()
        {
            this.OnEnter(() => _speed = Get<IScrap>().AttractSpeed);
        }

        public Type On(in ScrapLogic.Input.PhysicsProcess input)
        {
            IScrap scrap = Get<IScrap>();
            Vector2 shipPosition = Get<IShip>().CenterMarker.GlobalPosition;
            float delta = (float)input.Delta;

            // 近づくほど加速する
            _speed += scrap.AttractAcceleration * delta;

            Vector2 toShip = shipPosition - scrap.GlobalPosition;
            float step = _speed * delta;
            // 行き過ぎないよう、残りの距離で移動量を制限する
            Vector2 move = toShip.Length() <= step ? toShip : toShip.Normalized() * step;
            Output(new ScrapLogic.Output.Move(move));
            return ToSelf();
        }

        public Type On(in ScrapLogic.Input.ShipTouched input) => Collect();
    }

    /// <summary>
    /// 回収処理（所持数を加算してプールへ返す）
    /// </summary>
    protected Type Collect()
    {
        Get<IBattleRepo>().AddScrap(Get<IScrap>().Value);
        Output(new ScrapLogic.Output.RemoveSelf());
        return To<Idle>();
    }
}
