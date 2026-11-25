namespace EternalJourney.Cores.Models.Research;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 研究カードの抽選
/// </summary>
public static class ResearchCardPicker
{
    /// <summary>
    /// 重みに従ってレアリティを1つ抽選する
    /// </summary>
    public static ResearchRarity DrawRarity(Random rng)
    {
        IReadOnlyList<float> weights = ResearchCardCatalog.RarityWeights;
        double roll = rng.NextDouble() * weights.Sum();
        for (int i = 0; i < weights.Count; i++)
        {
            roll -= weights[i];
            if (roll < 0)
            {
                return (ResearchRarity)i;
            }
        }
        return (ResearchRarity)(weights.Count - 1);
    }

    /// <summary>
    /// カードを count 枚抽選する。1回の抽選で同じ種類は出さない
    /// </summary>
    public static IReadOnlyList<ResearchCard> Draw(Random rng, int count)
    {
        var result = new List<ResearchCard>(count);
        var used = new HashSet<ResearchCardType>();
        while (result.Count < count)
        {
            ResearchRarity rarity = DrawRarity(rng);
            List<ResearchCardDefinition> candidates = ResearchCardCatalog.All
                .Where(d => !used.Contains(d.Type) && d.MinRarity <= rarity)
                .ToList();
            if (candidates.Count == 0)
            {
                // 候補がない場合（カード種類が枚数より少ない設定）は打ち切る
                if (ResearchCardCatalog.All.All(d => used.Contains(d.Type)))
                {
                    break;
                }
                continue;
            }
            ResearchCardDefinition def = candidates[rng.Next(candidates.Count)];
            used.Add(def.Type);
            result.Add(new ResearchCard(def.Type, rarity));
        }
        return result;
    }

    /// <summary>
    /// 一番レアリティの高いカードの番号を返す（同じレアリティなら左のもの。空なら -1）
    /// </summary>
    public static int BestIndex(IReadOnlyList<ResearchCard> cards)
    {
        int best = -1;
        for (int i = 0; i < cards.Count; i++)
        {
            if (best < 0 || cards[i].Rarity > cards[best].Rarity)
            {
                best = i;
            }
        }
        return best;
    }
}
