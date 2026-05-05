namespace EternalJourney.Game.Domain;

using System;

/// <summary>
/// ゲームリポジトリインターフェース
/// </summary>
public interface IGameRepo
{
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
}

/// <summary>
/// ゲームリポジトリクラス
/// </summary>
public class GameRepo : IGameRepo
{
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
}
