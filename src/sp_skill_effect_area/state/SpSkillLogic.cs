namespace EternalJourney.SpSkillEffectArea.State;

using System;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Battle.Domain;
using EternalJourney.Cores.Models.Skill;
using EternalJourney.Game.Domain;

/// <summary>
/// SPスキルロジックインターフェース
/// </summary>
public interface ISpSkillLogic : ILogicBlock;

/// <summary>
/// SPスキルロジック
/// 選択されたSPスキルの衝撃波を発生させ、デバフがあれば持続時間の間だけ敵に掛ける
/// </summary>
[Meta]
public partial class SpSkillLogic : LogicBlock, ISpSkillLogic
{
    public SpSkillLogic()
    {
        // ステートを事前確保
        Set(new SpSkillState.Ready());
        Set(new SpSkillState.Debuffing());
    }

    /// <summary>
    /// 入力定義
    /// </summary>
    public static class Input
    {
        /// <summary>
        /// SPスキル発動（Repo から通知）
        /// </summary>
        public readonly record struct Activated;

        /// <summary>
        /// フレーム処理
        /// </summary>
        public readonly record struct Process(double Delta);
    }

    /// <summary>
    /// 出力定義
    /// </summary>
    public static class Output
    {
        /// <summary>
        /// 衝撃波の発生
        /// </summary>
        /// <param name="Damage">命中した敵へのダメージ（研究効果込み）</param>
        /// <param name="Stun">命中した敵をスタンさせるか</param>
        public readonly record struct Burst(float Damage, bool Stun);

        /// <summary>
        /// デバフ中のフレーム処理の要否
        /// </summary>
        public readonly record struct ProcessingChanged(bool Enabled);
    }
}

/// <summary>
/// SPスキル状態定義
/// </summary>
[StateDiagram]
public abstract partial record SpSkillState : LogicBlockState, IGet<SpSkillLogic.Input.Activated>
{
    public SpSkillState()
    {
        this.OnEnter(() => Get<IBattleRepo>().SPActivated += OnActivated);
        this.OnExit(() => Get<IBattleRepo>().SPActivated -= OnActivated);
    }

    private void OnActivated() => Input(new SpSkillLogic.Input.Activated());

    /// <summary>
    /// 発動処理（どの状態でも衝撃波を出し、デバフがあれば掛け直す）
    /// </summary>
    public Type On(in SpSkillLogic.Input.Activated input)
    {
        SpSkillDefinition def = SpSkillCatalog.Get(Get<IGameRepo>().SelectedSpSkill);
        IBattleRepo battleRepo = Get<IBattleRepo>();
        Output(new SpSkillLogic.Output.Burst(battleRepo.CalcSpSkillDamage(def.Damage), def.Stun));
        if (!def.HasDebuff)
        {
            return ToSelf();
        }
        Debuffing debuffing = Get<Debuffing>();
        debuffing.Definition = def;
        if (this is Debuffing current)
        {
            // デバフ中の再発動は同じ状態への遷移で OnEnter が走らないため、ここで掛け直す
            current.Restart();
            return ToSelf();
        }
        return To<Debuffing>();
    }

    /// <summary>
    /// 待機
    /// </summary>
    public record Ready : SpSkillState
    {
        public Ready()
        {
            this.OnEnter(() => Output(new SpSkillLogic.Output.ProcessingChanged(false)));
        }
    }

    /// <summary>
    /// デバフ中
    /// </summary>
    public record Debuffing : SpSkillState, IGet<SpSkillLogic.Input.Process>
    {
        /// <summary>
        /// 掛けているデバフの定義（遷移前に設定される）
        /// </summary>
        public SpSkillDefinition? Definition { get; set; }

        /// <summary>
        /// 経過時間（OnEnterで初期化する）
        /// </summary>
        private float _elapsed;

        public Debuffing()
        {
            this.OnEnter(() =>
            {
                Restart();
                Output(new SpSkillLogic.Output.ProcessingChanged(true));
            });
            this.OnExit(() => Get<IBattleRepo>().SetEnemyDebuff(1f, 1f));
        }

        /// <summary>
        /// 経過時間を戻し、デバフを掛け直す
        /// </summary>
        public void Restart()
        {
            _elapsed = 0f;
            if (Definition is SpSkillDefinition def)
            {
                Get<IBattleRepo>().SetEnemyDebuff(def.EnemyAtkMultiplier, def.EnemyFireIntervalMultiplier);
            }
        }

        public Type On(in SpSkillLogic.Input.Process input)
        {
            _elapsed += (float)input.Delta;
            float duration = Definition?.DebuffDuration ?? 0f;
            return _elapsed >= duration ? To<Ready>() : ToSelf();
        }
    }
}
