# README

## 開発環境構築

### .NETバージョン

- 8.0.406

### VSCode拡張機能

Recommendのものを入れる

### VSCodeでデバッグする場合

- VSCodeのGodot拡張機能を入れておく
- 以下の環境変数を追加する
  - 変数名：GODOT
  - 値：＜Godot実行ファイルパス＞
- 実行とデバッグで「Debug Game」を選択して「デバッグの開始」する

### Godot側のエディタ指定

- エディター > エディター設定 からエディター設定ウィンドウを開く
- .NET > エディター > Externa lEditor 項目でVisual Studio Codeを選択する
- Godotエディタの画面で.csファイルを開くとVSCodeで開かれるようになる

## 実装

### コーディング規約

Godotのコーディング規約に準拠
[Godotのコーディング規約](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_style_guide.html)

### クラス内（ノードにアタッチするスクリプト）の構成 ※参考

```csharp
public interface ISample : INode2D
{
}

// [必須]依存関係の提供・取得を可能にする属性
// ノードから通知を受け取った後に依存関係の処理が行われる
[Meta(typeof(IAutoNode))]
public partial class Sample : ISample, Node2D
{
    // [必須]ノードの準備完了や破棄などの通知を行う
    public override void _Notification(int what) => this.Notify(what);

    #region Signals
    // シグナル
    #endregion Signals
    #region State
    // ステートロジック
    #endregion State
    #region Exports
    // Godotエディタや親ノードから設定するプロパティ
    #endregion Exports
    #region PackedScenes
    #endregion PackedScenes
    #region Nodes
    #endregion Nodes
    #region Provisions
    // 子ノードへの依存性の提供
    #endregion Provisions
    #region Dependencies
    // 親ノードから依存性を取得
    #endregion Dependencies

    public void Initialize(){}
    public void OnReady(){}
    public void Setup(){}
    public void OnResolved(){}    
}
```

### ノードの初期化

- Initialiez
- OnReady
- Setup
- OnResolved

### 処理順について

Initialize()
-> シーンツリーにノードが追加される前に処理が走る
- ノードへのアクセスはできない
- 定数の設定、ファイルロード、初期化やノード以外のユーティリティクラスのインスタンス化などを行う
- 依存関係の解決
  - 本メソッドを呼び出すシーンが親の場合（プロバイダー）
    - this.Provide()で依存性の解決を行うことが可能
  - 本メソッドを呼び出すシーンが子の場合（レシーバー）
    - 親から依存性が注入されていない状態

OnReady() ※_Ready()
-> シーンツリーがノードに追加された後に処理が走る
- ノードへアクセス可能
- 依存関係の解決
  - 本メソッドを呼び出すシーンが親の場合（プロバイダー）
    - this.Provide()で依存性の解決を行うことが可能
  - 本メソッドを呼び出すシーンが子の場合（レシーバー）
    - 親から依存性が注入されていない状態

Setup()
-> 依存関係が解決しOnResolved()が実行される前に処理が走る
- IsTestingがfalseの場合に実行される
  - テストプログラムはテスト用にSetup()を実行する
  - テスト用の環境を別でセットするのでその切り分けで使う
- 依存関係の解決
  - 本メソッドを呼び出すシーンが親の場合（プロバイダー）
    - this.Provide()で依存性の解決を行うことが可能
  - 本メソッドを呼び出すシーンが子の場合（レシーバー）
    - this.DependOn()で親から依存性が注入されており、依存関係を利用することが可能
- 依存関係を使ってプロパティを初期化する等を行う

OnResolved()
-> 依存関係が解決された後に実行される
- 依存関係の解決
  - 本メソッドを呼び出すシーンが親の場合（プロバイダー）
    - this.Provide()で依存性の解決を行うことが可能
  - 本メソッドを呼び出すシーンが子の場合（レシーバー）
    - this.DependOn()で親から依存性が注入されており、依存関係を利用することが可能
- 依存関係を使った処理を記述する

Child:Initialize -> ChildLabel -> ParentLabel null
Child:Ready -> ChildLabel -> ParentLabel null
Parent:Initialize -> ParentLabel
Child:Setup -> ChildLabel -> ParentLabel Initialize
Child:Resolved -> ChildLabel -> ParentLabel Initialize
Parent:Setup -> ParentLabel
Parent:Resolved -> ParentLabel
Parent:Ready -> ParentLabel

# その他
Initialize() -> OnReady() -> Setup() -> OnResolved()

## VSCode再起動
インテリセンス等が聞かなくなった場合、再起動する
Ctrl + Shift + p
でreload windowを実行する
