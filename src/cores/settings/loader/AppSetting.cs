namespace EternalJourney.Cores.Settings.Loader;

using EternalJourney.Cores.Consts;
using EternalJourney.Cores.Helpers;
using Godot;

/// <summary>
/// ゲームアプリの設定
/// </summary>
public static class AppSetting
{
    /// <summary>
    /// ゲームアプリ設定インスタンス
    /// </summary>
    public static AppSettingModel Instance { get; private set; } = default!;

    /// <summary>
    /// ロード済みフラグ
    /// </summary>
    private static bool isLoaded { get; set; }

    /// <summary>
    /// ゲームアプリ設定ロード
    /// </summary>
    public static void LoadSettings()
    {
        // ロード済みチェック
        if (isLoaded)
        {
            // エラー出力
            GD.PushError(Message.GetMessage(Message.ERR_MESSAGE_001, "appsettings.json"));
            return;
        }
        // 設定情報取得
        AppSettingModel settings = JsonHelper.JsonMap<AppSettingModel>(Const.APP_SETTINGS_JSON_FILE_PATH);
        // インスタンスにセット
        Instance = settings;
        // ロード済みフラグをtrueにする
        isLoaded = true;
    }
}
