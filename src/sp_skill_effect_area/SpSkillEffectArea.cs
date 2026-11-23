namespace EternalJourney.SpSkillEffectArea;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using EternalJourney.Battle.Domain;
using EternalJourney.Common.StatusEffect;
using EternalJourney.Cores.Consts;
using EternalJourney.Enemy.Base;
using Godot;

public interface ISpSkillEffectArea : IArea2D
{
    public void Activate();
}

[Meta(typeof(IAutoNode))]
public partial class SpSkillEffectArea : Area2D, ISpSkillEffectArea
{
    public override void _Notification(int what) => this.Notify(what);

    #region Exports
    /// <summary>
    /// SPスキルの基本ダメージ
    /// </summary>
    [Export]
    public float Damage { get; set; } = 100f;
    #endregion Exports

    #region Nodes
    [Node]
    public CollisionShape2D CollisionShape2D { get; set; } = default!;
    #endregion Nodes

    #region Dependencies
    [Dependency]
    public IBattleRepo BattleRepo => this.DependOn<IBattleRepo>();
    #endregion Dependencies

    public void Setup()
    {
        // コリジョンレイヤをエネミーに設定
        CollisionLayer = CollisionEntity.PlayerBullet;
        // コリジョンマスクを船と弾丸に設定
        CollisionMask = CollisionEntity.Enemy;
    }

    public void OnResolved()
    {
        BattleRepo.SPActivated += Activate;
        AreaEntered += OnAreaEntered;
    }

    public void Activate()
    {
        var shape = (CircleShape2D)CollisionShape2D.Shape;
        shape.Radius = 10f;
        var tween = CreateTween();
        tween.TweenProperty(shape, "radius", 1000.0f, 1.0);
        tween.TweenCallback(Callable.From(() => shape.Radius = 10f));
        GD.Print("SPスキルエリア発動");
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area is not IBaseEnemy enemy)
        {
            return;
        }
        enemy.TakeDamage(BattleRepo.CalcSpSkillDamage(Damage));
        enemy.StatusEffectReceiverManager.Get<StunEffect>()?.Apply();
    }

    public void OnExitTree()
    {
        BattleRepo.SPActivated -= Activate;
    }
}
