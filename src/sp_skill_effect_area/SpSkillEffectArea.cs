namespace EternalJourney.SpSkillEffectArea;

using System;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Battle.Domain;
using EternalJourney.Common.StatusEffect;
using EternalJourney.Cores.Consts;
using EternalJourney.Enemy.Base;
using EternalJourney.Game.Domain;
using EternalJourney.SpSkillEffectArea.State;
using Godot;

/// <summary>
/// SPスキル効果範囲インターフェース
/// </summary>
public interface ISpSkillEffectArea : IArea2D
{
}

/// <summary>
/// SPスキル効果範囲クラス（衝撃波の表示と命中処理のみ。効果の内容は SpSkillLogic が決める）
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class SpSkillEffectArea : Area2D, ISpSkillEffectArea
{
    public override void _Notification(int what) => this.Notify(what);

    #region State
    /// <summary>
    /// SPスキルロジック
    /// </summary>
    public ISpSkillLogic SpSkillLogic { get; set; } = default!;

    /// <summary>
    /// SPスキルロジックバインド
    /// </summary>
    public LogicBlock.Binding SpSkillBinding { get; set; } = default!;
    #endregion State

    #region Exports
    /// <summary>
    /// 衝撃波の開始半径
    /// </summary>
    [Export]
    public float StartRadius { get; set; } = 10f;

    /// <summary>
    /// 衝撃波の最大半径
    /// </summary>
    [Export]
    public float MaxRadius { get; set; } = 1000f;

    /// <summary>
    /// 衝撃波が最大半径に達するまでの時間（秒）
    /// </summary>
    [Export]
    public float ExpandDuration { get; set; } = 1f;
    #endregion Exports

    #region Nodes
    [Node]
    public CollisionShape2D CollisionShape2D { get; set; } = default!;
    #endregion Nodes

    #region Dependencies
    [Dependency]
    public IBattleRepo BattleRepo => this.DependOn<IBattleRepo>();

    [Dependency]
    public IGameRepo GameRepo => this.DependOn<IGameRepo>();
    #endregion Dependencies

    /// <summary>
    /// 発生中の衝撃波のダメージ
    /// </summary>
    private float _burstDamage;

    /// <summary>
    /// 発生中の衝撃波がスタンを伴うか
    /// </summary>
    private bool _burstStun;

    public void Setup()
    {
        // コリジョンレイヤをプレイヤー弾に、マスクを敵に設定
        CollisionLayer = CollisionEntity.PlayerBullet;
        CollisionMask = CollisionEntity.Enemy;
        // 衝撃波が出ていない間は判定しない
        Monitoring = false;
        SetProcess(false);
        SpSkillLogic = new SpSkillLogic();
        SpSkillBinding = SpSkillLogic.Bind();
    }

    public void OnResolved()
    {
        SpSkillLogic.Set(BattleRepo);
        SpSkillLogic.Set(GameRepo);
        SpSkillBinding
            .OnOutput((in SpSkillLogic.Output.Burst output) => Burst(output.Damage, output.Stun))
            .OnOutput((in SpSkillLogic.Output.ProcessingChanged output) => SetProcess(output.Enabled));
        AreaEntered += OnAreaEntered;
        SpSkillLogic.Start<SpSkillState.Ready>();
    }

    public void OnProcess(double delta) => SpSkillLogic.Input(new SpSkillLogic.Input.Process(delta));

    /// <summary>
    /// 衝撃波を広げる
    /// </summary>
    private void Burst(float damage, bool stun)
    {
        _burstDamage = damage;
        _burstStun = stun;
        var shape = (CircleShape2D)CollisionShape2D.Shape;
        shape.Radius = StartRadius;
        SetDeferred(Area2D.PropertyName.Monitoring, true);
        Tween tween = CreateTween();
        tween.TweenProperty(shape, "radius", MaxRadius, ExpandDuration);
        tween.TweenCallback(Callable.From(() =>
        {
            shape.Radius = StartRadius;
            SetDeferred(Area2D.PropertyName.Monitoring, false);
        }));
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area is not IBaseEnemy enemy)
        {
            return;
        }
        enemy.TakeDamage(_burstDamage);
        if (_burstStun)
        {
            enemy.StatusEffectReceiverManager.Get<StunEffect>()?.Apply();
        }
    }

    public void OnExitTree()
    {
        AreaEntered -= OnAreaEntered;
        SpSkillBinding.Dispose();
        ((IDisposable)SpSkillLogic).Dispose();
    }
}
