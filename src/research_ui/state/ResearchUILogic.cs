namespace EternalJourney.ResearchUI.State;

using System;
using System.Collections.Generic;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Battle.Domain;
using EternalJourney.Cores.Models.Research;

/// <summary>
/// 研究UIロジックインターフェース
/// </summary>
public interface IResearchUILogic : ILogicBlock;

/// <summary>
/// 研究UIロジック
/// 研究開始 → 演出 → カード提示 → 1枚選択 → 効果適用、の流れを管理する。表示中はバトルを止める
/// </summary>
[Meta]
public partial class ResearchUILogic : LogicBlock, IResearchUILogic
{
    public ResearchUILogic()
    {
        // ステートを事前確保
        Set(new ResearchUIState.Hidden());
        Set(new ResearchUIState.Researching());
        Set(new ResearchUIState.Choosing());
    }

    /// <summary>
    /// 研究設定
    /// </summary>
    /// <param name="Duration">演出の長さ（秒）</param>
    /// <param name="CardCount">提示するカード枚数</param>
    public record Settings(float Duration, int CardCount);

    /// <summary>
    /// 演出中のタイトル
    /// </summary>
    public const string RESEARCHING_TITLE = "研究中...";

    /// <summary>
    /// カード選択中のタイトル
    /// </summary>
    public const string CHOOSING_TITLE = "研究成果を1つ選択";

    /// <summary>
    /// カード1枚分の表示内容
    /// </summary>
    public readonly record struct CardView(string Text, ResearchRarity Rarity);

    /// <summary>
    /// 提示カードを表示内容に変換する（対象・名前・効果・レアリティ）
    /// </summary>
    public static IReadOnlyList<CardView> ToViews(IReadOnlyList<ResearchCard> cards)
    {
        var views = new CardView[cards.Count];
        for (int i = 0; i < cards.Count; i++)
        {
            ResearchCard card = cards[i];
            ResearchCardDefinition def = ResearchCardCatalog.Get(card.Type);
            string text = $"{ResearchCardCatalog.TargetLabel(def.Target)}\n\n{def.Name}\n\n{def.Describe(card.Rarity)}\n\n{card.Rarity}";
            views[i] = new CardView(text, card.Rarity);
        }
        return views;
    }

    /// <summary>
    /// 入力定義
    /// </summary>
    public static class Input
    {
        /// <summary>
        /// 研究開始（Repo から通知）
        /// </summary>
        public readonly record struct ResearchStarted;

        /// <summary>
        /// フレーム処理
        /// </summary>
        public readonly record struct Process(double Delta);

        /// <summary>
        /// カード選択
        /// </summary>
        public readonly record struct CardSelected(int Index);
    }

    /// <summary>
    /// 出力定義
    /// </summary>
    public static class Output
    {
        /// <summary>
        /// 研究UIを開いた（バトル停止）
        /// </summary>
        public readonly record struct Opened(string Title);

        /// <summary>
        /// 演出の進み具合（0〜1）
        /// </summary>
        public readonly record struct Progress(float Ratio);

        /// <summary>
        /// カード提示
        /// </summary>
        public readonly record struct CardsShown(string Title, IReadOnlyList<CardView> Cards);

        /// <summary>
        /// 研究UIを閉じた（バトル再開）
        /// </summary>
        public readonly record struct Closed;
    }
}

/// <summary>
/// 研究UI状態定義
/// </summary>
[StateDiagram]
public abstract partial record ResearchUIState : LogicBlockState
{
    /// <summary>
    /// 非表示（研究開始待ち）
    /// </summary>
    public record Hidden : ResearchUIState, IGet<ResearchUILogic.Input.ResearchStarted>
    {
        public Hidden()
        {
            this.OnEnter(() =>
            {
                Get<IBattleRepo>().ResearchStarted += OnResearchStarted;
                Output(new ResearchUILogic.Output.Closed());
            });
            this.OnExit(() => Get<IBattleRepo>().ResearchStarted -= OnResearchStarted);
        }

        private void OnResearchStarted() => Input(new ResearchUILogic.Input.ResearchStarted());

        public Type On(in ResearchUILogic.Input.ResearchStarted input) => To<Researching>();
    }

    /// <summary>
    /// 研究中（演出）
    /// </summary>
    public record Researching : ResearchUIState, IGet<ResearchUILogic.Input.Process>
    {
        /// <summary>
        /// 経過時間（OnEnterで初期化する）
        /// </summary>
        private float _elapsed;

        public Researching()
        {
            this.OnEnter(() =>
            {
                _elapsed = 0f;
                Output(new ResearchUILogic.Output.Opened(ResearchUILogic.RESEARCHING_TITLE));
            });
        }

        public Type On(in ResearchUILogic.Input.Process input)
        {
            ResearchUILogic.Settings settings = Get<ResearchUILogic.Settings>();
            _elapsed += (float)input.Delta;
            Output(new ResearchUILogic.Output.Progress(Math.Min(1f, _elapsed / settings.Duration)));
            if (_elapsed < settings.Duration)
            {
                return ToSelf();
            }

            // 事前確保済みの Choosing に抽選結果を設定してから遷移
            Get<Choosing>().Cards = ResearchCardPicker.Draw(Get<Random>(), settings.CardCount);
            return To<Choosing>();
        }
    }

    /// <summary>
    /// カード選択中
    /// </summary>
    public record Choosing : ResearchUIState, IGet<ResearchUILogic.Input.CardSelected>
    {
        /// <summary>
        /// 提示中のカード（遷移前に設定される）
        /// </summary>
        public IReadOnlyList<ResearchCard> Cards { get; set; } = Array.Empty<ResearchCard>();

        public Choosing()
        {
            this.OnEnter(() => Output(new ResearchUILogic.Output.CardsShown(
                ResearchUILogic.CHOOSING_TITLE, ResearchUILogic.ToViews(Cards))));
        }

        public Type On(in ResearchUILogic.Input.CardSelected input)
        {
            if (input.Index < 0 || input.Index >= Cards.Count)
            {
                return ToSelf();
            }
            Get<IBattleRepo>().ApplyResearchCard(Cards[input.Index]);
            return To<Hidden>();
        }
    }
}
