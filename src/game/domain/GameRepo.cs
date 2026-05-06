namespace EternalJourney.Game.Domain;

using System;
using System.Collections.Generic;
using EternalJourney.Cores.Models.Skill;

/// <summary>
/// ゲームリポジトリインターフェース
/// </summary>
public interface IGameRepo
{
    /// <summary>
    /// 選択済みスキル一覧
    /// </summary>
    IReadOnlyList<SkillType> SelectedSkills { get; }

    /// <summary>
    /// 選択済みスキルを保存する
    /// </summary>
    void SetSelectedSkills(IReadOnlyList<SkillType> skills);

    /// <summary>
    /// 最終スコア
    /// </summary>
    int FinalScore { get; }

    /// <summary>
    /// 最終タイム
    /// </summary>
    float FinalTime { get; }

    /// <summary>
    /// リザルトを保存する
    /// </summary>
    void SaveResult(int score, float time);

    /// <summary>
    /// スキル選択完了イベント
    /// </summary>
    event Action? SkillSelected;

    /// <summary>
    /// スキル選択完了を通知する
    /// </summary>
    void NotifySkillSelected();

    /// <summary>
    /// バトル初期化完了イベント
    /// </summary>
    event Action? BattleInitialized;

    /// <summary>
    /// バトル初期化完了を通知する
    /// </summary>
    void NotifyBattleInitialized();

    /// <summary>
    /// ゲームオーバーイベント
    /// </summary>
    event Action? GameOver;

    /// <summary>
    /// ゲームオーバーを通知する
    /// </summary>
    void NotifyGameOver();

    /// <summary>
    /// バトル終了イベント
    /// </summary>
    event Action? BattleEnded;

    /// <summary>
    /// バトル終了を通知する
    /// </summary>
    void NotifyBattleEnded();
}

/// <summary>
/// ゲームリポジトリクラス
/// </summary>
public class GameRepo : IGameRepo
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public IReadOnlyList<SkillType> SelectedSkills { get; private set; } = new List<SkillType>();

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void SetSelectedSkills(IReadOnlyList<SkillType> skills) => SelectedSkills = skills;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public int FinalScore { get; private set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public float FinalTime { get; private set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void SaveResult(int score, float time)
    {
        FinalScore = score;
        FinalTime = time;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event Action? SkillSelected;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void NotifySkillSelected() => SkillSelected?.Invoke();

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event Action? BattleInitialized;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void NotifyBattleInitialized() => BattleInitialized?.Invoke();

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event Action? GameOver;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void NotifyGameOver() => GameOver?.Invoke();

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event Action? BattleEnded;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void NotifyBattleEnded() => BattleEnded?.Invoke();
}
