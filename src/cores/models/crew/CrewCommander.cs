namespace EternalJourney.Cores.Models.Crews;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 指揮官の判断（作業クルーを銃座と修理に割り振る純粋ロジック）
/// </summary>
public static class CrewCommander
{
    /// <summary>
    /// 修理を表す配置先
    /// </summary>
    public const int REPAIR = -1;

    /// <summary>
    /// 修理をやめて銃座に戻すまでの HP 比率の余裕（行き来の繰り返しを防ぐ）
    /// </summary>
    public const float HYSTERESIS = 0.15f;

    /// <summary>
    /// 同時に修理へ回す最大人数
    /// </summary>
    public const int MAX_REPAIRERS = 2;

    /// <summary>
    /// 今操作している銃座に残る優先度（敵の数に換算。小さな差で持ち場を移らないようにする）
    /// </summary>
    public const int STAY_BONUS = 2;

    /// <summary>
    /// 指揮官の個性ごとの、修理を指示し始める HP 比率
    /// </summary>
    public static float RepairThreshold(CrewPersonality personality) => personality switch
    {
        CrewPersonality.Coward => 0.7f,
        CrewPersonality.Steady => 0.5f,
        CrewPersonality.Brave => 0.25f,
        _ => 0.5f,
    };

    /// <summary>
    /// 修理へ回す人数を決める
    /// </summary>
    /// <param name="hpRatio">船の HP 比率</param>
    /// <param name="commander">指揮官の個性</param>
    /// <param name="currentRepairers">現在修理に回している人数</param>
    public static int RepairerCount(float hpRatio, CrewPersonality commander, int currentRepairers)
    {
        float threshold = RepairThreshold(commander);
        int count = 0;
        for (int level = 1; level <= MAX_REPAIRERS; level++)
        {
            // 1人目はしきい値、2人目はしきい値の半分で回す。回復しても余裕分を超えるまでは維持する
            float levelThreshold = threshold / level;
            bool below = hpRatio < levelThreshold;
            bool keep = currentRepairers >= level && hpRatio < levelThreshold + HYSTERESIS;
            if (below || keep)
            {
                count = level;
            }
        }
        return count;
    }

    /// <summary>
    /// 作業クルーの配置先を決める
    /// </summary>
    /// <param name="hpRatio">船の HP 比率</param>
    /// <param name="threats">銃座ごとの敵の数</param>
    /// <param name="workers">作業クルー</param>
    /// <param name="commander">指揮官の個性</param>
    /// <param name="current">現在の配置先（作業クルーと同じ並び。REPAIR は修理）</param>
    /// <returns>新しい配置先（作業クルーと同じ並び）</returns>
    public static int[] Plan(
        float hpRatio,
        IReadOnlyList<int> threats,
        IReadOnlyList<CrewDefinition> workers,
        CrewPersonality commander,
        IReadOnlyList<int> current)
    {
        int workerCount = workers.Count;
        int currentRepairers = current.Count(s => s == REPAIR);
        int repairers = Math.Min(RepairerCount(hpRatio, commander, currentRepairers), workerCount);

        // 修理担当: 今修理している者を優先し、次に臆病な者から選ぶ
        HashSet<int> repairSet = Enumerable.Range(0, workerCount)
            .OrderByDescending(i => current[i] == REPAIR)
            .ThenBy(i => (int)workers[i].Personality)
            .ThenBy(i => i)
            .Take(repairers)
            .ToHashSet();

        // 操作する銃座: 敵の多い順。今操作されている銃座は移動の手間がないぶん優先する
        int gunners = workerCount - repairers;
        HashSet<int> manned = current.Where(s => s >= 0).ToHashSet();
        List<int> stations = Enumerable.Range(0, threats.Count)
            .OrderByDescending(s => threats[s] + (manned.Contains(s) ? STAY_BONUS : 0))
            .ThenByDescending(s => manned.Contains(s))
            .ThenBy(s => s)
            .Take(gunners)
            .ToList();

        int[] plan = new int[workerCount];
        var open = new List<int>(stations);
        var unplaced = new List<int>();
        for (int i = 0; i < workerCount; i++)
        {
            if (repairSet.Contains(i))
            {
                plan[i] = REPAIR;
            }
            else if (open.Remove(current[i]))
            {
                // 今の銃座が引き続き操作対象なら、そのまま残す
                plan[i] = current[i];
            }
            else
            {
                unplaced.Add(i);
            }
        }
        foreach (int i in unplaced)
        {
            plan[i] = open.Count > 0 ? open[0] : REPAIR;
            if (open.Count > 0)
            {
                open.RemoveAt(0);
            }
        }
        return plan;
    }
}
