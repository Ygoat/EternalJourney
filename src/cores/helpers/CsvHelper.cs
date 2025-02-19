namespace EternalJourney.Cores.Helpers;

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;

/// <summary>
/// CSVヘルパー
/// </summary>
public static class GDCsvHelper
{
    /// <summary>
    /// csvを読み込み、クラスにマッピングしたリストを返却する
    /// </summary>
    /// <typeparam name="T">マッピングするクラス</typeparam>
    /// <param name="filePath">csvのファイルパス</param>
    /// <returns>マッピングされたクラスのリスト</returns>
    public static List<T> CsvMap<T>(string filePath)
    {
        // csvファイルアクセス
        Godot.FileAccess file = Godot.FileAccess.Open(filePath, Godot.FileAccess.ModeFlags.Read);

        // CSV読み込み

        using (var reader = new StreamReader(file.GetPathAbsolute()))
        {
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // csv データが行毎にクラスに格納され、IEnumerable<T> として割り当て、List化
                List<T> mappedClassList = csv.GetRecords<T>().ToList();
                return mappedClassList;
                //// records は IEnumerable なので、こんな使い方ができます。

            }
        }
    }
}
