namespace EternalJourney.Cores.Settings;

using System;
using System.Collections.Generic;
using System.Reflection;
using EternalJourney.Cores.Consts;
using EternalJourney.Cores.Helpers;
using Godot;

public static partial class AppSetting
{
    private static bool isLoaded { get; set; }

    public static void LoadSettings()
    {
        var settings = JsonHelper.JsonMap<Dictionary<string, object>>(Const.APP_SETTINGS_JSON_FILE_PATH);
        // Testクラスの静的プロパティを取得
        var properties = typeof(AppSetting).GetProperties(BindingFlags.Public | BindingFlags.Static);

        // 各プロパティに対して設定値をマッピング
        foreach (var property in properties)
        {
            // 設定値が辞書に含まれているか確認
            if (settings.TryGetValue(property.Name, out var value))
            {
                GD.Print(value);
                // 型に合わせて値を変換し、設定
                var targetType = property.PropertyType;
                // 基本型の処理
                if (targetType == typeof(string))
                {
                    property.SetValue(null, value.ToString());
                }
                else if (targetType == typeof(int) && int.TryParse(value.ToString(), out int intValue))
                {
                    property.SetValue(null, intValue);
                }
                else if (targetType == typeof(bool) && bool.TryParse(value.ToString(), out bool boolValue))
                {
                    property.SetValue(null, boolValue);
                }
                else if (targetType == typeof(DateTime) && DateTime.TryParse(value.ToString(), out DateTime dateValue))
                {
                    property.SetValue(null, dateValue);
                }
                else
                {
                    // 他の型の場合、必要に応じて追加の処理を追加
                    GD.PushError($"Unsupported type or invalid value for property {property.Name}");
                }
            }
        }
    }


}
