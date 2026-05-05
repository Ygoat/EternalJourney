namespace EternalJourney.SelectUI.State;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Game.Domain;
using EternalJourney.SelectUI;

/// <summary>
/// スキル選択UIロジックインターフェース
/// </summary>
public interface ISelectSkillUILogic : ILogicBlock<SelectSkillUILogic.State>;

/// <summary>
/// スキル選択UIロジック
/// </summary>
[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class SelectSkillUILogic : LogicBlock<SelectSkillUILogic.State>, ISelectSkillUILogic
{
    public override Transition GetInitialState() => To<State.Idle>();

    public abstract record State : StateLogic<State>
    {
        /// <summary>
        /// 待機
        /// </summary>
        public record Idle : State
        {
            public Idle()
            {
                OnAttach(() => Get<ISelectSkillUI>().Selected += OnSelected);
                OnDetach(() => Get<ISelectSkillUI>().Selected -= OnSelected);
            }

            private void OnSelected() => Get<IGameRepo>().NotifySkillSelected();
        }
    }
}
