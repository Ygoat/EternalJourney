namespace EternalJourney.SelectUI.State;

using System;
using System.Collections.Generic;
using System.Linq;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using EternalJourney.Cores.Models.Crews;
using EternalJourney.Cores.Utils;
using EternalJourney.Game.Domain;

/// <summary>
/// クルー選択UIロジックインターフェース
/// </summary>
public interface ISelectCrewUILogic : ILogicBlock;

/// <summary>
/// クルー選択UIロジック（指揮官1名・作業クルー4名を選ぶ）
/// </summary>
[Meta]
public partial class SelectCrewUILogic : LogicBlock, ISelectCrewUILogic
{
    public SelectCrewUILogic()
    {
        // ステートを事前確保
        Set(new SelectCrewUIState.Idle());
    }

    /// <summary>
    /// 未選択を表すインデックス
    /// </summary>
    public const int NO_SELECTION = -1;

    /// <summary>
    /// 何も選んでいないときの説明文
    /// </summary>
    public const string GUIDE_TEXT = "指揮官を1名、作業クルーを4名選択してください";

    /// <summary>
    /// 入力定義
    /// </summary>
    public static class Input
    {
        /// <summary>
        /// 指揮官ボタンのトグル
        /// </summary>
        public readonly record struct CommanderToggled(int Index, bool Pressed);

        /// <summary>
        /// 作業クルーボタンのトグル
        /// </summary>
        public readonly record struct WorkerToggled(int Index, bool Pressed);

        /// <summary>
        /// 決定ボタン押下
        /// </summary>
        public readonly record struct ConfirmPressed;

        /// <summary>
        /// ランダムボタン押下
        /// </summary>
        public readonly record struct RandomPressed;
    }

    /// <summary>
    /// 出力定義
    /// </summary>
    public static class Output
    {
        /// <summary>
        /// 候補の表示名
        /// </summary>
        public readonly record struct Initialized(IReadOnlyList<string> CommanderNames, IReadOnlyList<string> WorkerNames);

        /// <summary>
        /// 選択状態の変化
        /// </summary>
        /// <param name="CommanderIndex">選択中の指揮官（未選択は NO_SELECTION）</param>
        /// <param name="WorkerSelected">作業クルーごとの選択状態</param>
        /// <param name="Description">説明欄の文字列</param>
        /// <param name="CanConfirm">決定できるか</param>
        public readonly record struct SelectionChanged(
            int CommanderIndex,
            IReadOnlyList<bool> WorkerSelected,
            string Description,
            bool CanConfirm);
    }

    /// <summary>
    /// 候補の表示名（名前と個性）
    /// </summary>
    public static string ButtonLabel(CrewDefinition crew) =>
        $"{crew.Name}\n{CrewCatalog.PersonalityName(crew.Personality)}";
}

/// <summary>
/// クルー選択UI状態定義
/// </summary>
[StateDiagram]
public abstract partial record SelectCrewUIState : LogicBlockState
{
    /// <summary>
    /// 選択待ち
    /// </summary>
    public record Idle : SelectCrewUIState,
        IGet<SelectCrewUILogic.Input.CommanderToggled>,
        IGet<SelectCrewUILogic.Input.WorkerToggled>,
        IGet<SelectCrewUILogic.Input.ConfirmPressed>,
        IGet<SelectCrewUILogic.Input.RandomPressed>
    {
        // 選択状態（OnEnterで初期化する）
        private int _commander = SelectCrewUILogic.NO_SELECTION;
        private readonly List<int> _workers = new();
        private string _description = SelectCrewUILogic.GUIDE_TEXT;

        public Idle()
        {
            this.OnEnter(() =>
            {
                _commander = SelectCrewUILogic.NO_SELECTION;
                _workers.Clear();
                _description = SelectCrewUILogic.GUIDE_TEXT;
                Output(new SelectCrewUILogic.Output.Initialized(
                    CrewCatalog.Commanders.Select(SelectCrewUILogic.ButtonLabel).ToArray(),
                    CrewCatalog.Workers.Select(SelectCrewUILogic.ButtonLabel).ToArray()));
                NotifySelection();
            });
        }

        public Type On(in SelectCrewUILogic.Input.CommanderToggled input)
        {
            if (input.Index < 0 || input.Index >= CrewCatalog.Commanders.Count)
            {
                return ToSelf();
            }
            if (input.Pressed)
            {
                // 指揮官は1名だけ（前の選択は外れる）
                _commander = input.Index;
                _description = CrewCatalog.Commanders[input.Index].Description;
            }
            else if (input.Index == _commander)
            {
                _commander = SelectCrewUILogic.NO_SELECTION;
            }
            NotifySelection();
            return ToSelf();
        }

        public Type On(in SelectCrewUILogic.Input.WorkerToggled input)
        {
            if (input.Index < 0 || input.Index >= CrewCatalog.Workers.Count)
            {
                return ToSelf();
            }
            if (input.Pressed && !_workers.Contains(input.Index) && _workers.Count < CrewCatalog.WORKER_COUNT)
            {
                _workers.Add(input.Index);
                _description = CrewCatalog.Workers[input.Index].Description;
            }
            else if (!input.Pressed)
            {
                _workers.Remove(input.Index);
            }
            // 5人目は選択状態に戻さない（NotifySelection で押下が解除される）
            NotifySelection();
            return ToSelf();
        }

        public Type On(in SelectCrewUILogic.Input.RandomPressed input)
        {
            Random rng = Get<Random>();
            _commander = rng.Next(CrewCatalog.Commanders.Count);
            _workers.Clear();
            _workers.AddRange(RandomPick.Distinct(rng, CrewCatalog.Workers.Count, CrewCatalog.WORKER_COUNT));
            _description = string.Join("\n",
                new[] { CrewCatalog.Commanders[_commander] }.Concat(_workers.Select(i => CrewCatalog.Workers[i]))
                    .Select(c => $"{c.Name}（{CrewCatalog.PersonalityName(c.Personality)}）{CrewCatalog.PassiveText(c.Passive)}"));
            NotifySelection();
            return ToSelf();
        }

        public Type On(in SelectCrewUILogic.Input.ConfirmPressed input)
        {
            if (!CanConfirm)
            {
                return ToSelf();
            }
            IGameRepo gameRepo = Get<IGameRepo>();
            gameRepo.SetSelectedCrew(
                CrewCatalog.Commanders[_commander],
                _workers.Select(i => CrewCatalog.Workers[i]).ToArray());
            gameRepo.NotifyCrewSelected();
            return ToSelf();
        }

        private bool CanConfirm =>
            _commander != SelectCrewUILogic.NO_SELECTION && _workers.Count == CrewCatalog.WORKER_COUNT;

        private void NotifySelection()
        {
            bool[] selected = Enumerable.Range(0, CrewCatalog.Workers.Count).Select(_workers.Contains).ToArray();
            Output(new SelectCrewUILogic.Output.SelectionChanged(_commander, selected, _description, CanConfirm));
        }
    }
}
