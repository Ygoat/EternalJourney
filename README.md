# GamersLab企画 EternalJourney（エタジャニ）開発

GamersLab第一弾企画！
放置シューティングゲーム EternalJourney 開発！

## 開発環境構築

VSCodeおよびGodotエディタによるゲーム開発になります
ChickenSoftさんのテンプレートを使用してます

- 必要なもの
  - GitHubアカウント
  - Git
  - EternalJourneyソースコード
    - 八木所有のgithubプロジェクトに招待するのでGithubアカウント名を共有ください
  - .NET Sdk
  - Godot
  - VSCode
    - 拡張機能

### Githubアカウント

- [こちら](https://docs.github.com/ja/get-started/start-your-journey/creating-an-account-on-github)に沿って作成する

### Gitインストール

- [こちら](https://gitforwindows.org/)からダウンロードする
- インストーラーに沿ってインストールする
  - 改行文字の自動変換は無効化を推奨

### ソースコード

- GithubのEternalJourneyリポジトリからクローンする

### .NETインストール

- ver 8.0.405 以上
  - [こちらからダウンロード](https://dotnet.microsoft.com/ja-jp/download/dotnet/8.0)
    - Windowsの場合：Windows x64
  - インストーラーに従ってインストールする

### Godotインストール

- ver 4.3.0 固定 ※後々アップデート予定
  - [こちらからダウンロード](https://godotengine.org/download/archive/4.3-stable/)
    - Windowsの場合：Widowsの.NET
  - zipファイルを解凍し、解凍後のフォルダを```C:\Program Files```に配置する
    - 必要であればフォルダを開いてexeファイルをショートカット等に登録する

### VSCode

- VSCodeをインストールする
  - [こちらからダウンロード](https://code.visualstudio.com/)

- Recommendのものを入れる
  - "alfish.godot-files",
  - "christian-kohler.path-intellisense",
  - "DavidAnson.vscode-markdownlint",
  - "EditorConfig.EditorConfig",
  - "gurumukhi.selected-lines-count",
  - "jjkim.gdscript",
  - "josefpihrt-vscode.roslynator",
  - "ms-dotnettools.csharp",
  - "selcukermaya.se-csproj-extensions",
  - "streetsidesoftware.code-spell-checker",
  - "VisualStudioExptTeam.vscodeintellicode"

- ターミナルを開き```dotnet build```を実行する
  - nugetパッケージのインストール等が行われる

### Godot側のエディタ指定

- エディター > エディター設定 からエディター設定ウィンドウを開く
- .NET > エディター > Externa lEditor 項目でVisual Studio Codeを選択する
- Godotエディタの画面で.csファイルを開くとVSCodeで開かれるようになる

### VSCodeでデバッグする場合

- VSCodeのGodot拡張機能を入れておく
- 以下の環境変数を追加する
  - 変数名：GODOT
  - 値：＜Godot実行ファイルパス＞
- 実行とデバッグで「Debug Game」を選択して「デバッグの開始」する
  - ブレークポイントで処理を止めることが可能

## 実装

### コーディング規約

- 文字コード：UTF-8
- 改行文字：LF
- Godotのコーディング規約に準拠（一般的なC#コーディング規約）
  - [Godotのコーディング規約](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_style_guide.html)

### ノードにアタッチするスクリプトについて

#### クラス内の構成　※参考

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

#### ノードの初期化

- ChickenSoftのAutoInject(AutoNode)ライブラリにより4つの初期化処理に分割される
  - Initialize() -> OnReady() -> Setup() -> OnResolved()
  - ※Godotのデフォルトでは_Ready()の１つだけの初期化処理のみ
    - 依存関係の処理を行うため4つに分割している

- Initialize()
  - シーンツリーにノードが追加される前に処理が走る
  - ノードへのアクセスはできない
  - ノード以外の初期化（定数の設定、ファイルロードやユーティリティクラスのインスタンス化）を行う

- OnReady() ※_Ready()
  - シーンツリーがノードに追加された後に処理が走る
    - この処理以降からノードへのアクセスが可能となる

- Setup()
  - 依存関係が注入されOnResolved()が実行される前に処理が走る
    - この処理以降から[Dependency]属性がついたノードへのアクセスが可能となる
  - IsTestingがfalseの場合に実行される ※テスト環境では実行されない
    - テストスクリプトでSetup()を実装することで、テスト用の初期処理を実装可能
  - プロパティの初期値設定を行う

- OnResolved()
  - ステートのバインドや初期化、イベントへのファンクションの設定等の初期処理を行う

- テスト環境では以下の2つの初期化処理となる
  - OnReady() -> OnResolved()
    - テストスクリプトでSetup()を実装することで、テスト用の初期化処理を実装可能
  - ※基本的にテストスクリプトは使用しない

## その他

### 用語について

| 用語 | 読み | 概要 |
| --- | --- | --- |
| Godot | ごどー | 本開発で使用するゲームエンジン |
| ChickenSoft | ちきんそふと | Godot .NET版のOSSツール<br>[詳細はこちら](https://chickensoft.games/) |
| シーン | しーん | キャラクターやステージなどゲーム内のオブジェクトやロジックをまとめた再利用可能な単位 |
| ノード | のーど | シーンを構成する最小要素<br>ツリー型に配置することが可能 |
| シーンツリー | しーんつりー | シーンを構成するノードのツリー |
| シグナル | しぐなる | ノードでイベントが発生した場合に発信される<br>ノード同士を疎結合に保つ |
| スクリプト | スクリプト | C#コードのファイル |
| ステート | すてーと | オブジェクトの状態 |
| ステートロジック | すてーとろじっく | オブジェクトの状態におうじて異なるふるまいをさせる |
| 依存関係/依存性 | いぞんかんけい/いぞんせい | Provide()メソッド/[Dependency] 属性で<br>注入/取得されるオブジェクト|
|  |  |  |
