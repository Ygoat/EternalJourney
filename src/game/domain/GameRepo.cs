namespace EternalJourney.Game.Domain;

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
}
