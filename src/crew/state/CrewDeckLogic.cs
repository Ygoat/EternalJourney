namespace EternalJourney.Crew.State;

using System;
using System.Collections.Generic;
using System.Linq;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Battle.Domain;
using EternalJourney.Cores.Models.Crews;
using EternalJourney.Crew.Domain;
using EternalJourney.Game.Domain;

/// <summary>
/// クルー配置ロジックインターフェース
/// </summary>
public interface ICrewDeckLogic : ILogicBlock;

/// <summary>
/// クルー配置ロジック（指揮官）
/// バトル開始時にクルーを乗船させ、一定間隔で敵の配置と船の HP から作業クルーの配置を決め直す
/// </summary>
[Meta]
public partial class CrewDeckLogic : LogicBlock, ICrewDeckLogic
{
    public CrewDeckLogic()
    {
        // ステートを事前確保
        Set(new CrewDeckState.Waiting());
        Set(new CrewDeckState.Commanding());
    }

    /// <summary>
    /// 設定
    /// </summary>
    /// <param name="CommandInterval">配置を決め直す間隔（秒）</param>
    public record Settings(float CommandInterval);

    /// <summary>
    /// 入力定義
    /// </summary>
    public static class Input
    {
        /// <summary>
        /// バトル開始（Repo から通知）
        /// </summary>
        public readonly record struct BattleStarted;

        /// <summary>
        /// 時間経過
        /// </summary>
        public readonly record struct Tick(double Delta);
    }

    /// <summary>
    /// 出力定義
    /// </summary>
    public static class Output
    {
        /// <summary>
        /// クルーの乗船（作業クルーは Stations の銃座に直接置く）
        /// </summary>
        public readonly record struct Embark(
            CrewDefinition Commander,
            IReadOnlyList<CrewDefinition> Workers,
            IReadOnlyList<int> Stations);

        /// <summary>
        /// 作業クルーへの配置指示
        /// </summary>
        public readonly record struct Assign(int WorkerIndex, int Station);
    }
}

/// <summary>
/// クルー配置状態定義
/// </summary>
[StateDiagram]
public abstract partial record CrewDeckState : LogicBlockState
{
    /// <summary>
    /// バトル開始待ち
    /// </summary>
    public record Waiting : CrewDeckState, IGet<CrewDeckLogic.Input.BattleStarted>
    {
        public Waiting()
        {
            this.OnEnter(() => Get<IBattleRepo>().BattleStarted += OnBattleStarted);
            this.OnExit(() => Get<IBattleRepo>().BattleStarted -= OnBattleStarted);
        }

        private void OnBattleStarted() => Input(new CrewDeckLogic.Input.BattleStarted());

        public Type On(in CrewDeckLogic.Input.BattleStarted input) => To<Commanding>();
    }

    /// <summary>
    /// 指揮中
    /// </summary>
    public record Commanding : CrewDeckState, IGet<CrewDeckLogic.Input.Tick>
    {
        // 指揮の状態（OnEnterで初期化する）
        private float _elapsed;
        private float _hpRatio = 1f;
        private int[] _current = Array.Empty<int>();

        public Commanding()
        {
            this.OnEnter(() =>
            {
                IGameRepo gameRepo = Get<IGameRepo>();
                CrewDefinition commander = gameRepo.SelectedCommander;
                IReadOnlyList<CrewDefinition> workers = gameRepo.SelectedWorkers;
                _elapsed = 0f;
                _hpRatio = 1f;
                // 最初は全員を銃座に置く（銃座より多い作業クルーは修理へ回す）
                _current = Enumerable.Range(0, workers.Count)
                    .Select(i => i < CrewRepo.STATION_COUNT ? i : CrewCommander.REPAIR)
                    .ToArray();

                Get<IBattleRepo>().ShipHpChanged += OnShipHpChanged;
                Get<ICrewRepo>().Configure(commander, workers);
                Output(new CrewDeckLogic.Output.Embark(commander, workers, _current.ToArray()));
            });
            this.OnExit(() => Get<IBattleRepo>().ShipHpChanged -= OnShipHpChanged);
        }

        private void OnShipHpChanged(float current, float max) => _hpRatio = max > 0f ? current / max : 0f;

        public Type On(in CrewDeckLogic.Input.Tick input)
        {
            _elapsed += (float)input.Delta;
            CrewDeckLogic.Settings settings = Get<CrewDeckLogic.Settings>();
            if (_elapsed < settings.CommandInterval)
            {
                return ToSelf();
            }
            _elapsed = 0f;

            ICrewRepo crewRepo = Get<ICrewRepo>();
            int[] threats = Enumerable.Range(0, CrewRepo.STATION_COUNT).Select(crewRepo.ThreatAt).ToArray();
            CrewPersonality commander = crewRepo.Commander?.Personality ?? CrewPersonality.Steady;
            int[] plan = CrewCommander.Plan(_hpRatio, threats, crewRepo.Workers, commander, _current);
            for (int i = 0; i < plan.Length; i++)
            {
                if (plan[i] != _current[i])
                {
                    Output(new CrewDeckLogic.Output.Assign(i, plan[i]));
                }
            }
            _current = plan;
            return ToSelf();
        }
    }
}
