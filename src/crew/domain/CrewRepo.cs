namespace EternalJourney.Crew.Domain;

using System;
using System.Collections.Generic;
using System.Linq;
using EternalJourney.Cores.Models.Crews;

/// <summary>
/// クルーリポジトリインターフェース（銃座の操作状況と、クルーによる武器の補正）
/// </summary>
public interface ICrewRepo : IDisposable
{
    /// <summary>
    /// 指揮官
    /// </summary>
    CrewDefinition? Commander { get; }

    /// <summary>
    /// 作業クルー
    /// </summary>
    IReadOnlyList<CrewDefinition> Workers { get; }

    /// <summary>
    /// 乗船するクルーを設定する
    /// </summary>
    void Configure(CrewDefinition commander, IReadOnlyList<CrewDefinition> workers);

    /// <summary>
    /// 銃座の操作者が変わったイベント（引数は銃座番号）
    /// </summary>
    event Action<int>? StationChanged;

    /// <summary>
    /// 銃座を操作している作業クルー（いなければ null）
    /// </summary>
    CrewDefinition? WorkerAt(int station);

    /// <summary>
    /// 銃座が操作されているか（銃座番号が負の武器＝敵の武器などは常に true）
    /// </summary>
    bool IsManned(int station);

    /// <summary>
    /// 銃座の操作者を設定する（離れるときは null）
    /// </summary>
    void SetManned(int station, CrewDefinition? worker);

    /// <summary>
    /// 銃座が捕捉している敵の数を報告する
    /// </summary>
    void ReportThreat(int station, int count);

    /// <summary>
    /// 銃座が捕捉している敵の数
    /// </summary>
    int ThreatAt(int station);

    /// <summary>
    /// 銃座の武器に掛かる補正（作業クルー不在時は自動攻撃モードの弱体化込み）
    /// </summary>
    CrewBonus BonusFor(int station, WeaponKind kind);
}

/// <summary>
/// クルーリポジトリ
/// </summary>
public class CrewRepo : ICrewRepo
{
    /// <summary>
    /// 銃座の数
    /// </summary>
    public const int STATION_COUNT = 4;

    private readonly CrewDefinition?[] _manned = new CrewDefinition?[STATION_COUNT];
    private readonly int[] _threats = new int[STATION_COUNT];

    public CrewDefinition? Commander { get; private set; }

    public IReadOnlyList<CrewDefinition> Workers { get; private set; } = Array.Empty<CrewDefinition>();

    public event Action<int>? StationChanged;

    public void Configure(CrewDefinition commander, IReadOnlyList<CrewDefinition> workers)
    {
        Commander = commander;
        Workers = workers.ToArray();
        // 指揮官の全体パッシブが変わるので全銃座を通知する
        for (int i = 0; i < STATION_COUNT; i++)
        {
            StationChanged?.Invoke(i);
        }
    }

    public CrewDefinition? WorkerAt(int station) => IsValid(station) ? _manned[station] : null;

    public bool IsManned(int station) => !IsValid(station) || _manned[station] != null;

    public void SetManned(int station, CrewDefinition? worker)
    {
        if (!IsValid(station) || _manned[station] == worker)
        {
            return;
        }
        _manned[station] = worker;
        StationChanged?.Invoke(station);
    }

    public void ReportThreat(int station, int count)
    {
        if (IsValid(station))
        {
            _threats[station] = count;
        }
    }

    public int ThreatAt(int station) => IsValid(station) ? _threats[station] : 0;

    public CrewBonus BonusFor(int station, WeaponKind kind)
    {
        if (!IsValid(station))
        {
            return CrewBonus.None;
        }
        CrewDefinition? worker = _manned[station];
        CrewBonus bonus = CrewBonus.For(kind, Commander, worker);
        // 作業クルーがいない銃座は自動攻撃モード（弱体化して撃ち続ける）
        return worker == null ? bonus.WithAutoMode() : bonus;
    }

    private static bool IsValid(int station) => station >= 0 && station < STATION_COUNT;

    public void Dispose()
    {
        StationChanged = null;
        GC.SuppressFinalize(this);
    }
}
