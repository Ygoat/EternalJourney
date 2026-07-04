namespace EternalJourney.BattleUI.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Battle.Domain;
using EternalJourney.Cores.Models.Skill;
using EternalJourney.Game.Domain;
using EternalJourney.SukillButton;

/// <summary>
/// バトルUIロジック
/// </summary>
public interface IBattleUILogic : ILogicBlock<BattleUILogic.State>;

/// <summary>
/// アプリケーションロジック
/// </summary>
[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class BattleUILogic : LogicBlock<BattleUILogic.State>, IBattleUILogic
{
    /// <summary>
    /// 初期状態
    /// </summary>
    /// <returns></returns>
    public override Transition GetInitialState() => To<State.InActive>();

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

    /// <summary>
    /// 状態定義
    /// </summary>
    public abstract record State : StateLogic<State>
    {
        /// <summary>
        /// 非アクティブ（バトル開始待ち）
        /// </summary>
        public record InActive : State, IGet<Input.BattleStarted>
        {
            public InActive()
            {
                OnAttach(() => Get<IBattleRepo>().BattleStarted += OnBattleStarted);
                OnDetach(() => Get<IBattleRepo>().BattleStarted -= OnBattleStarted);
            }

            private void OnBattleStarted() => Input(new Input.BattleStarted());

            public Transition On(in Input.BattleStarted input)
            {
                IBattleUI battleUI = Get<IBattleUI>();
                ISkillButton[] slots = { battleUI.SkillButton1, battleUI.SkillButton2, battleUI.SkillButton3, battleUI.SkillButton4 };
                var skills = Get<IGameRepo>().SelectedSkills;
                for (int i = 0; i < slots.Length && i < skills.Count; i++)
                {
                    slots[i].SetLabel(SkillInfo.GetName(skills[i]));
                    slots[i].Activated += battleUI.GetSkill(skills[i]).Activate;
                }
                Output(new Output.ActivateBattleUI());
                return To<Active>();
            }
        }

        /// <summary>
        /// アクティブ（バトル中）
        /// </summary>
        public record Active : State, IGet<Input.PhysicsProcess>, IGet<Input.SPButtonPressed>
        {
            public Active()
            {
                OnAttach(() =>
                {
                    IBattleRepo battleRepo = Get<IBattleRepo>();
                    battleRepo.Score.Sync += OnScoreCountUp;
                    battleRepo.ShipHpChanged += OnShipHpChanged;
                    battleRepo.GameOverOccurred += OnGameOver;
                    battleRepo.SpPercentChanged += OnSpPercentChanged;
                });
                OnDetach(() =>
                {
                    IBattleRepo battleRepo = Get<IBattleRepo>();
                    battleRepo.Score.Sync -= OnScoreCountUp;
                    battleRepo.ShipHpChanged -= OnShipHpChanged;
                    battleRepo.GameOverOccurred -= OnGameOver;
                    battleRepo.SpPercentChanged -= OnSpPercentChanged;
                });
            }

            public void OnScoreCountUp(int currentScore) =>
                Output(new Output.ScoreChanged(currentScore));

            public void OnShipHpChanged(float currentHp, float maxHp) =>
                Output(new Output.ShipHpChanged(CalcHpGaugeRatio(currentHp, maxHp)));

            public void OnGameOver()
            {
                IBattleRepo battleRepo = Get<IBattleRepo>();
                Get<IGameRepo>().SaveResult(battleRepo.Score.Value, Get<IBattleUI>().Count);
                Get<IGameRepo>().NotifyGameOver();
                Output(new Output.GameOver());
            }

            public Transition On(in Input.PhysicsProcess input)
            {
                Output(new Output.TikCount());
                return ToSelf();
            }

            public Transition On(in Input.SPButtonPressed input)
            {
                Get<IBattleRepo>().ActivateSP();
                return ToSelf();
            }

            public void OnSpPercentChanged(float spPercent) =>
                Output(new Output.SpPercentChanged(spPercent / 100f));

            public float CalcHpGaugeRatio(float currentHp, float maxHp)
            {
                float ratio = maxHp > 0f ? currentHp / maxHp : 0f;
                return ratio;
            }
        }
    }
}
