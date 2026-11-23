namespace EternalJourney.BattleUI.State;

using System;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using Chickensoft.Sync.Primitives;
using EternalJourney.Battle.Domain;
using EternalJourney.Cores.Models.Skill;
using EternalJourney.Game.Domain;
using EternalJourney.SkillButton;

/// <summary>
/// バトルUIロジック
/// </summary>
public interface IBattleUILogic : ILogicBlock;

/// <summary>
/// アプリケーションロジック
/// </summary>
[Meta]
public partial class BattleUILogic : LogicBlock, IBattleUILogic
{
    public BattleUILogic()
    {
        // ステートを事前確保
        Set(new BattleUIState.InActive());
        Set(new BattleUIState.Active());
    }


    /// <summary>
    /// 入力定義
    /// </summary>
    public static class Input
    {
        /// <summary>
        /// 物理プロセス
        /// </summary>
        public readonly record struct PhysicsProcess;

        /// <summary>
        /// バトル開始
        /// </summary>
        public readonly record struct BattleStarted;

        /// <summary>
        /// SPボタン押下
        /// </summary>
        public readonly record struct SPButtonPressed;
    }

    /// <summary>
    /// 出力定義
    /// </summary>
    public static class Output
    {
        /// <summary>
        /// スコア変化
        /// </summary>
        public readonly record struct ScoreChanged(int CurrentScore);

        /// <summary>
        /// ゲーム終了
        /// </summary>
        public readonly record struct GameOver;

        /// <summary>
        /// 船HP変化
        /// </summary>
        public readonly record struct ShipHpChanged(float Ratio);

        /// <summary>
        /// バトルUI起動
        /// </summary>
        public readonly record struct ActivateBattleUI;

        /// <summary>
        /// タイムカウント
        /// </summary>
        public readonly record struct TikCount;

        /// <summary>
        /// SPパーセント変化
        /// </summary>
        public readonly record struct SpPercentChanged(float Ratio);
    }

    // 不必要なヒープの割り当てを減らすために、入力と出力は読み取り専用のレコード構造体（readonly record struct）にすべき

}

/// <summary>
/// 状態定義
/// </summary>
[StateDiagram]
public abstract partial record BattleUIState : LogicBlockState
{
    /// <summary>
    /// 非アクティブ（バトル開始待ち）
    /// </summary>
    public record InActive : BattleUIState, IGet<BattleUILogic.Input.BattleStarted>
    {
        public InActive()
        {
            this.OnEnter(() => Get<IBattleRepo>().BattleStarted += OnBattleStarted);
            this.OnExit(() => Get<IBattleRepo>().BattleStarted -= OnBattleStarted);
        }

        private void OnBattleStarted() => Input(new BattleUILogic.Input.BattleStarted());

        public Type On(in BattleUILogic.Input.BattleStarted input)
        {
            IBattleUI battleUI = Get<IBattleUI>();
            ISkillButton[] slots = { battleUI.SkillButton1, battleUI.SkillButton2, battleUI.SkillButton3, battleUI.SkillButton4 };
            var skills = Get<IGameRepo>().SelectedSkills;
            for (int i = 0; i < slots.Length && i < skills.Count; i++)
            {
                slots[i].SetLabel(SkillInfo.GetName(skills[i]));
                slots[i].Activated += battleUI.GetSkill(skills[i]).Activate;
            }
            Output(new BattleUILogic.Output.ActivateBattleUI());
            return To<Active>();
        }
    }

    /// <summary>
    /// アクティブ（バトル中）
    /// </summary>
    public record Active : BattleUIState, IGet<BattleUILogic.Input.PhysicsProcess>, IGet<BattleUILogic.Input.SPButtonPressed>
    {
        /// <summary>
        /// スコア購読用バインディング
        /// </summary>
        private AutoValue<int>.Binding? _scoreBinding;

        public Active()
        {
            this.OnEnter(() =>
            {
                IBattleRepo battleRepo = Get<IBattleRepo>();
                // 購読登録時に現在値が即時通知される
                _scoreBinding = battleRepo.Score.Bind();
                _scoreBinding.OnValue(OnScoreCountUp);
                battleRepo.ShipHpChanged += OnShipHpChanged;
                battleRepo.GameOverOccurred += OnGameOver;
                battleRepo.SpPercentChanged += OnSpPercentChanged;
            });
            this.OnExit(() =>
            {
                IBattleRepo battleRepo = Get<IBattleRepo>();
                _scoreBinding?.Dispose();
                _scoreBinding = null;
                battleRepo.ShipHpChanged -= OnShipHpChanged;
                battleRepo.GameOverOccurred -= OnGameOver;
                battleRepo.SpPercentChanged -= OnSpPercentChanged;
            });
        }

        public void OnScoreCountUp(int currentScore) =>
            Output(new BattleUILogic.Output.ScoreChanged(currentScore));

        public void OnShipHpChanged(float currentHp, float maxHp) =>
            Output(new BattleUILogic.Output.ShipHpChanged(CalcHpGaugeRatio(currentHp, maxHp)));

        public void OnGameOver()
        {
            IBattleRepo battleRepo = Get<IBattleRepo>();
            Get<IGameRepo>().SaveResult(battleRepo.Score.Value, Get<IBattleUI>().Count);
            Get<IGameRepo>().NotifyGameOver();
            Output(new BattleUILogic.Output.GameOver());
        }

        public Type On(in BattleUILogic.Input.PhysicsProcess input)
        {
            Output(new BattleUILogic.Output.TikCount());
            return ToSelf();
        }

        public Type On(in BattleUILogic.Input.SPButtonPressed input)
        {
            Get<IBattleRepo>().ActivateSP();
            return ToSelf();
        }

        public void OnSpPercentChanged(float spPercent) =>
            Output(new BattleUILogic.Output.SpPercentChanged(spPercent / 100f));

        public float CalcHpGaugeRatio(float currentHp, float maxHp)
        {
            float ratio = maxHp > 0f ? currentHp / maxHp : 0f;
            return ratio;
        }
    }
}
