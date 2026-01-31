namespace EternalJourney.Common.BaseFactory;

using System.Collections.Generic;
using System.Linq;
using EternalJourney.Cores.Pooling;
using EternalJourney.Cores.Utils;
using Godot;

/// <summary>
/// ベースファクトリインターフェース
/// オブジェクトプールを使用したファクトリの共通インターフェースを定義します
/// </summary>
/// <typeparam name="T">プールするオブジェクトの型（Node派生型）</typeparam>
public interface IBaseFactory<T> where T : Node
{
    /// <summary>
    /// プールされたオブジェクトのキュー
    /// </summary>
    Queue<T> ObjectQueue { get; }

    /// <summary>
    /// プールサイズ
    /// </summary>
    int PoolSize { get; set; }

    /// <summary>
    /// プールされたオブジェクトの配列
    /// </summary>
    T[] PooledObjects { get; set; }

    /// <summary>
    /// クールダウン待機時間（秒）
    /// </summary>
    float WaitTime { get; set; }
}


/// <summary>
/// ベースファクトリクラス
/// オブジェクトプーリングパターンを実装した汎用ファクトリ基底クラス
/// Godot のノードライフサイクルと統合し、効率的なオブジェクト再利用を実現します
///
/// 注意: ジェネリック型はChickensoftのイントロスペクションをサポートしないため、
/// [Meta]属性と AutoInject は使用していません
/// </summary>
/// <typeparam name="T">プールするオブジェクトの型（Node派生型）</typeparam>
public abstract partial class BaseFactory<T> : Node2D, IBaseFactory<T> where T : Node
{
    #region Exports
    /// <summary>
    /// プールサイズ（事前に生成するオブジェクトの数）
    /// </summary>
    [Export]
    public int PoolSize { get; set; } = 100;

    /// <summary>
    /// クールダウン待機時間（秒）
    /// </summary>
    [Export]
    public float WaitTime { get; set; } = 0.1f;

    /// <summary>
    /// プールされたオブジェクトの配列
    /// </summary>
    public T[] PooledObjects { get; set; } = default!;

    /// <summary>
    /// プールされたオブジェクトのキュー（利用可能なオブジェクトを管理）
    /// </summary>
    public Queue<T> ObjectQueue { get; set; } = new Queue<T>();
    #endregion Exports

    #region Nodes
    /// <summary>
    /// タイマーノード（クールダウン管理用）
    /// </summary>
    protected Timer Timer { get; set; } = default!;
    #endregion Nodes

    #region Dependencies
    /// <summary>
    /// インスタンス化ユーティリティ（シーンのロードと生成を担当）
    /// </summary>
    protected IInstantiator Instantiator { get; set; } = default!;
    #endregion Dependencies

    /// <summary>
    /// 初期化処理
    /// </summary>
    public virtual void Initialize()
    {
        // Instantiatorの初期化
        Instantiator = new Instantiator(GetTree());

        // Timerノードの取得
        Timer = GetNode<Timer>("Timer");
    }

    /// <summary>
    /// セットアップ処理（オブジェクトプールの初期化）
    /// 子クラスでオーバーライドして、シーンパスの指定とカスタム初期化を実装してください
    /// </summary>
    public virtual void Setup()
    {
        // プール配列の初期化
        PooledObjects = new T[PoolSize];

        // シーンパスの取得（子クラスで実装）
        string scenePath = GetScenePath();

        // オブジェクトのプール生成
        PooledObjects = PooledObjects.Select(obj =>
        {
            // シーンからオブジェクトをインスタンス化
            obj = Instantiator.LoadAndInstantiate<T>(scenePath);

            // プールライフサイクルの設定
            SetupPoolableObject(obj);

            // キューに追加（利用可能な状態にする）
            ObjectQueue.Enqueue(obj);

            return obj;
        }).ToArray();
    }

    /// <summary>
    /// 解決後処理（タイマーの設定など）
    /// </summary>
    public virtual void OnResolved()
    {
        // タイマー設定
        Timer.OneShot = true;
        Timer.WaitTime = WaitTime;
        Timer.Timeout += OnTimeout;
    }

    /// <summary>
    /// プールされるオブジェクトのシーンパスを取得
    /// 子クラスで必ず実装してください
    /// </summary>
    /// <returns>シーンファイルのパス</returns>
    protected abstract string GetScenePath();

    /// <summary>
    /// プール可能なオブジェクトのセットアップ
    /// オブジェクトがプールに返却される際のイベントハンドラを設定します
    /// </summary>
    /// <param name="obj">セットアップするオブジェクト</param>
    protected virtual void SetupPoolableObject(T obj)
    {
        // IPoolableインターフェースを実装している場合は自動的に処理されます
        // 追加のセットアップが必要な場合は子クラスでオーバーライドしてください
    }

    /// <summary>
    /// プールからオブジェクトを取得
    /// </summary>
    /// <returns>プールから取得したオブジェクト（プールが空の場合はnull）</returns>
    protected virtual T? AcquireFromPool()
    {
        if (ObjectQueue.Count == 0)
        {
            GD.PrintErr($"[{GetType().Name}] Pool is empty! Consider increasing PoolSize.");
            return null;
        }

        T obj = ObjectQueue.Dequeue();

        // IPoolableインターフェースの OnAcquired を呼び出し
        if (obj is IPoolable poolable)
        {
            poolable.OnAcquired();
        }

        return obj;
    }

    /// <summary>
    /// プールにオブジェクトを返却
    /// </summary>
    /// <param name="obj">返却するオブジェクト</param>
    protected virtual void ReleaseToPool(T obj)
    {
        // IPoolableインターフェースの OnReleased を呼び出し
        if (obj is IPoolable poolable)
        {
            poolable.OnReleased();
        }

        // キューに返却
        ObjectQueue.Enqueue(obj);
    }

    /// <summary>
    /// タイマータイムアウトイベント
    /// 子クラスでオーバーライドして、クールダウン完了時の処理を実装してください
    /// </summary>
    protected virtual void OnTimeout()
    {
        // 子クラスで実装
    }

    /// <summary>
    /// タイマー開始
    /// </summary>
    protected virtual void StartTimer()
    {
        Timer.Start();
    }
}
