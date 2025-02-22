namespace EternalJourney.Cores.Helpers;

using System;
using System.Text.Json;
using Chickensoft.GoDotLog;
using EternalJourney.Cores.Consts;
using Godot;

/// <summary>
/// JSONヘルパー
/// </summary>
public static class JsonHelper
{

    /// <summary>
    /// JSONからクラスにマッピングする
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="FilePath"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static T JsonMap<T>(string FilePath)
    {
        FileAccess file = FileAccess.Open(FilePath, FileAccess.ModeFlags.Read);
        // テキスト抽出
        string jsonText = file.GetAsText();

        // ファイルクローズ
        file.Close();
        // Jsonパース
        T? parseModel = JsonSerializer.Deserialize<T>(jsonText);

        file.Close();

        // パースが成功したか
        if (parseModel == null)
        {
            // エラー
            GDLog.PushErrorAction(Message.GetMessage(Message.ERR_MESSAGE_002, FilePath));
            throw new InvalidOperationException(Message.GetMessage(Message.ERR_MESSAGE_002, FilePath));
        }
        return parseModel;
    }
}
