namespace EternalJourney.Common.Traits;

using Godot;

public partial class Status : Resource
{
    /// <summary>
    /// 最大耐久値
    /// </summary>
    [Export]
    public float MaxDur { get; set; }

    /// <summary>
    /// 攻撃力
    /// </summary>
    [Export]
    public float Atk { get; set; }

    /// <summary>
    /// スピード
    /// </summary>
    [Export]
    public float Spd { get; set; }

    /// <summary>
    /// 防御力
    /// </summary>
    [Export]
    public float Def { get; set; }
}
