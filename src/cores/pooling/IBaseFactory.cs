namespace EternalJourney.Cores.Pooling;

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
    System.Collections.Generic.Queue<T> ObjectQueue { get; }

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
