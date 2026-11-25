namespace EternalJourney.Cores.Utils;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ランダム選択のユーティリティ（選択画面のランダムボタン用）
/// </summary>
public static class RandomPick
{
    /// <summary>
    /// 0〜count-1 から重複なしで k 個を選ぶ（昇順で返す）
    /// </summary>
    public static IReadOnlyList<int> Distinct(Random rng, int count, int k)
    {
        if (k > count)
        {
            throw new ArgumentOutOfRangeException(nameof(k), k, "候補数より多くは選べません");
        }
        List<int> pool = Enumerable.Range(0, count).ToList();
        // フィッシャー–イェーツで先頭 k 個を確定させる
        for (int i = 0; i < k; i++)
        {
            int j = rng.Next(i, count);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }
        return pool.Take(k).OrderBy(i => i).ToArray();
    }
}
