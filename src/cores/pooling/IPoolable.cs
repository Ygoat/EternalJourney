namespace EternalJourney.Cores.Pooling;

using Godot;

/// <summary>
/// オブジェクトプールに対応するオブジェクトのインターフェース
/// Godot のノードライフサイクルと統合し、プールからの取得・返却時の動作を定義します
/// </summary>
public interface IPoolable
{
    /// <summary>
    /// プールから取得された時に呼ばれるコールバック
    /// オブジェクトの初期化や状態のリセットを行います
    /// </summary>
    void OnAcquired();

    /// <summary>
    /// プールに返却される時に呼ばれるコールバック
    /// オブジェクトの後処理やクリーンアップを行います
    /// </summary>
    void OnReleased();
}
