namespace EternalJourney.ResearchLab.State;

using System;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using Chickensoft.Sync.Primitives;
using EternalJourney.Battle.Domain;

/// <summary>
/// 研究室ロジックインターフェース
/// </summary>
public interface IResearchLabLogic : ILogicBlock;

/// <summary>
/// 研究室ロジック
/// スクラップと研究コストを監視して研究可否を表示に伝え、タップされたら研究を開始する
/// </summary>
[Meta]
public partial class ResearchLabLogic : LogicBlock, IResearchLabLogic
{
    public ResearchLabLogic()
    {
        // ステートを事前確保
        Set(new ResearchLabState.Watching());
    }

    /// <summary>
    /// 入力定義
    /// </summary>
    public static class Input
    {
        /// <summary>
        /// 研究室がタップされた
        /// </summary>
        public readonly record struct Pressed;
    }

    /// <summary>
    /// 出力定義
    /// </summary>
    public static class Output
    {
        /// <summary>
        /// 研究可否・コストの変化
        /// </summary>
        /// <param name="CanResearch">スクラップが足りているか</param>
        /// <param name="CostText">研究室に表示するコスト文字列</param>
        public readonly record struct AvailabilityChanged(bool CanResearch, string CostText);
    }
}

/// <summary>
/// 研究室状態定義
/// </summary>
[StateDiagram]
public abstract partial record ResearchLabState : LogicBlockState
{
    /// <summary>
    /// 監視中（スクラップ・コストの変化を表示に伝える）
    /// </summary>
    public record Watching : ResearchLabState, IGet<ResearchLabLogic.Input.Pressed>
    {
        private AutoValue<int>.Binding? _scrapBinding;
        private AutoValue<int>.Binding? _costBinding;

        /// <summary>
        /// 最新のスクラップ数（OnEnterで初期化する）
        /// </summary>
        private int _scrap;

        /// <summary>
        /// 最新の研究コスト（OnEnterで初期化する）
        /// </summary>
        private int _cost;

        public Watching()
        {
            this.OnEnter(() =>
            {
                IBattleRepo battleRepo = Get<IBattleRepo>();
                _scrap = battleRepo.Scrap.Value;
                _cost = battleRepo.ResearchCost.Value;
                // 購読登録時に現在値が即時通知される
                _scrapBinding = battleRepo.Scrap.Bind();
                _scrapBinding.OnValue(v => { _scrap = v; Notify(); });
                _costBinding = battleRepo.ResearchCost.Bind();
                _costBinding.OnValue(v => { _cost = v; Notify(); });
            });
            this.OnExit(() =>
            {
                _scrapBinding?.Dispose();
                _scrapBinding = null;
                _costBinding?.Dispose();
                _costBinding = null;
            });
        }

        private void Notify() =>
            Output(new ResearchLabLogic.Output.AvailabilityChanged(_scrap >= _cost, $"LAB {_cost}"));

        public Type On(in ResearchLabLogic.Input.Pressed input)
        {
            // 可否の最終判定は Repo が行う（不足なら何も起きない）
            Get<IBattleRepo>().TryStartResearch();
            return ToSelf();
        }
    }
}
