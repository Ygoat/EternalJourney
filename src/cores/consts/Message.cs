namespace EternalJourney.Cores.Consts;

/// <summary>
/// メッセージ
/// </summary>
public static class Message
{
    // 通常メッセージ
    public const string INF_MESSAGE_001 = "ゲーム開始";

    // エラーメッセージ
    public const string ERR_MESSAGE_001 = "{0}はすでに読み込まれています。";
    public const string ERR_MESSAGE_002 = "{0}の読み込みに失敗しました。";

    /// <summary>
    ///  メッセージ取得
    /// </summary>
    /// <param name="message"></param>
    /// <param name="replace"></param>
    /// <returns></returns>
    public static string GetMessage(string message, params string[] replace)
    {
        return string.Format(message, replace);
    }
}