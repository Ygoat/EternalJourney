namespace EternalJourney.ResearchUI;

using EternalJourney.Cores.Models.Research;
using Godot;

/// <summary>
/// 研究カードの表示スタイル（色・表記）
/// </summary>
public static class ResearchCardStyle
{
    /// <summary>
    /// レアリティの色
    /// </summary>
    public static Color RarityColor(ResearchRarity rarity) => rarity switch
    {
        ResearchRarity.Common => new Color(0.85f, 0.85f, 0.85f),
        ResearchRarity.Rare => new Color(0.35f, 0.65f, 1f),
        ResearchRarity.Epic => new Color(0.75f, 0.4f, 1f),
        ResearchRarity.Legendary => new Color(1f, 0.75f, 0.2f),
        _ => Colors.White,
    };
}
