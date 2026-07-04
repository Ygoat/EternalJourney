# EternalJourney プロジェクトレビュー

> **基準**: [ARCHITECTURE_REFERENCE.md](ARCHITECTURE_REFERENCE.md)（Chickensoft GameDemo流アーキテクチャ）
> **実施日**: 2026-07-04（レビュー時ブランチ: feature/special_skill 相当のコードベース）
> **スコープ**: `src/` 全体・ビルド/テスト/CI設定・Claude Code 設定

---

## 1. 総合評価

| 観点 | 評価 | 一言 |
| --- | :---: | --- |
| 3層分離（View–Logic–Domain） | ○ | Ship/App/Game は模範的。新しめの機能（SPスキル・スポナー・武器）で View にロジック漏れ |
| LogicBlocks の使い方 | ○ | Input/Output の record struct は完全準拠。1状態1ファイル・HSM・Preallocate は未導入 |
| DI（AutoInject） | ○ | Setup/OnResolved の分担は概ね良好。Provide のタイミングに不統一 |
| インターフェース分離 | △ | Repo/主要ノードは I 型完備。Logic を具象型で持つ View が半数 |
| Repo（Domain層） | △ | Logic間の疎結合は達成。イベントが生 `event Action` 主体で Dispose 不完全 |
| 後始末（リソース解放） | × | **App と Battle に Dispose 漏れ（リーク）**。他は概ね良好 |
| フラグ vs 状態 | △ | UI系は状態化できている。死亡・攻撃可否が bool フラグ |
| テスト | × | **実質ゼロ**（GameTest 1件・中身コメントアウト）。基盤（GoDotTest/CI/coverage）は完備 |
| 命名規約 | ○ | `_camelCase`・snake_case は徹底。定数と SP/Sp 表記に揺れ、タイポ数件 |
| ツーリング/CI | ◎ | editorconfig・CI・カバレッジ・renovate は Chickensoft テンプレート水準を維持 |

**総評**: 土台（テンプレート由来のツーリング、Repo イベントバス、record struct メッセージ）は高品質。一方で「機能追加の速度優先」で入った直近のコード（SPスキル・スキル群・スポナー）ほど規約から逸脱しており、**リソース解放漏れ2件とテスト不在**が最大のリスク。改善は §2 の優先度順に対応することを推奨。

---

## 2. 改善事項一覧

### 優先度: 高（バグ・リークに直結）

| # | 事項 | 該当箇所 | 基準 | 推奨対応 |
| --- | --- | --- | --- | --- |
| H-1 | App の後始末が皆無。AppLogic / AppBinding / **所有する AppRepo** が Dispose されない | `src/app/App.cs` | §5, §12 | `OnTreeExiting()` で Binding.Dispose() / Logic.Dispose() / AppRepo.Dispose() を追加 |
| H-2 | Battle の `OnExitTree()` が空。所有する **BattleRepo（AutoProp保持）がリーク** | `src/battle/Battle.cs:112` | §5, §12 | BattleRepo.Dispose() を追加 |
| H-3 | `BattleRepo.Dispose` が不完全（`_score` 未破棄・イベント購読未解除） | `src/battle/domain/BattleRepo.cs:383-395` | 付録 Repo テンプレート | `_score.Dispose()` とイベントの null 化を追加 |
| H-4 | SpSkillEffectArea が LogicBlock を持たず、ダメージ100・スタン付与のルールを View に直書き。`AreaEntered` 購読解除漏れ・マジックナンバー・デバッグ `GD.Print` 残存・コメント誤りも同居 | `src/sp_skill_effect_area/SpSkillEffectArea.cs:43,48-64` | §2, §12 | SpSkillEffectAreaLogic を新設しルールを移動。ダメージ量等は定数または `data/masters/` へ。購読解除を追加 |
| H-5 | テストが実質ゼロ（1件・全行コメントアウト）。基盤はある | `test/src/GameTest.cs` | §10 | まず StateTester による状態テストから整備（§3-2 ロードマップ参照） |

### 優先度: 中（設計原則からの逸脱）

| # | 事項 | 該当箇所 | 基準 | 推奨対応 |
| --- | --- | --- | --- | --- |
| M-1 | EnemySpawner: 毎フレームのスポーン判定・敵ID抽選が View に直書き（Logicなし） | `src/enemy_spawner/EnemySpawner.cs:100-122` | §2 | SpawnerLogic を新設し判定を移動 |
| M-2 | StandardWeapon: 最近接敵探索・照準補間・攻撃判断が View の物理処理内 | `src/weapon/abstract/StandardWeapon.cs:190-207,239-251` | §2 | 判断部分を StandardWeaponLogic へ Input/Output で移動 |
| M-3 | 死亡フラグ `_isDead`。`Dead` 状態が存在しない | `src/ship/state/ShipLogic.cs:125` | §12「状態 vs フラグ」 | `Alive` / `Dead` の状態遷移に置き換え |
| M-4 | BaseEnemyLogic が `DummyState` 1状態のみ。スタンを Output トグルで表現 | `src/enemy/base/state/BaseEnemyLogic.cs:80,95-105` | §6 | `Active` / `Stunned` / `Dead` の状態列挙へ再設計（HSM の好例になる） |
| M-5 | 攻撃可否フラグ `CanAttack` | `src/weapon/abstract/base/BaseWeapon.cs:26` | §12 | Weapon 側 Logic の状態（`Ready` / `Cooldown`）へ |
| M-6 | View が Logic を具象型で保持（モック不可） | `src/enemy/base/BaseEnemy.cs:52`、`src/enemy/standard/StandardEnemy.cs:42`、`src/weapon/abstract/StandardWeapon.cs:45`、`src/skills/status_up/StatusUpSkillBase.cs:18` | §1 原則4 | `I{Name}Logic` 型で受ける（Ship.cs:41 と同じ形に統一） |
| M-7 | `IGameRepo` が IDisposable 未実装（IBattleRepo/IAppRepo は実装済みで不統一）。`AppRepo.Dispose` は空実装 | `src/game/domain/GameRepo.cs:10`、`src/app/domain/AppRepo.cs:130` | 付録 | IDisposable を実装し、購読解除を Dispose に集約 |
| M-8 | `this.Provide()` のタイミング不統一（Initialize/OnReady/OnResolved が混在） | `src/app/App.cs:119`、`src/game/Game.cs:93`、`src/enemy_spawner/EnemySpawner.cs:83`、`src/weapon/abstract/StandardWeapon.cs:121` | §5 | OnResolved()（依存が無い最上位のみ OnReady）に統一 |
| M-9 | 文字列分岐によるステータス効果設定（`"poison"/"stun"`） | `src/bullet/abstract/base/BaseBullet.cs:162-175` | §6.2 | enum 化 + switch 式（`_ =>` フォールバック付き）へ |
| M-10 | csproj 堅牢化未達: `WarningsAsErrors CS9057` なし / `EmitCompilerGeneratedFiles` コメントアウト / `CodeAnalyzers.ruleset` なし | `EternalJourney.csproj` | §11 | 3点を有効化（ソースジェネレータ多用プロジェクトでは CS9057 が特に有効） |

### 優先度: 低（品質・一貫性）

| # | 事項 | 該当箇所 | 推奨対応 |
| --- | --- | --- | --- |
| L-1 | 定数命名の不統一（`GameNodePath` 等が PascalCase） | `src/cores/consts/Const.cs:21,26,31` | `ALL_UPPER_SNAKE` へ統一 |
| L-2 | namespace タイポ `SukillButton`（フォルダは skill_button） | `src/skill_button/` 配下 | `SkillButton` へリネーム |
| L-3 | `SP`/`Sp` 表記ゆれ（クラス SpSkillEffectArea vs シーン SPSkillEffectArea.tscn） | `src/sp_skill_effect_area/` | どちらかに統一（C# 規約なら `SpSkill…`） |
| L-4 | 未使用変数+タイポ `upgradeDpendencies` | `src/battle/Battle.cs:93` | 削除 |
| L-5 | `SPSkillType.cs` が参照ゼロのデッドコード | `src/cores/models/skill/SPSkillType.cs` | SPスキル実装に結線するか削除 |
| L-6 | `EternalJourney.csproj.old` の残骸 | リポジトリルート | 削除（git 履歴に残る） |
| L-7 | data/masters の単数形 CSV と複数形 import/translation 残骸の混在 | `data/masters/` | 未使用の import/translation を整理 |
| L-8 | `docs/ScriptTempate.md` のファイル名誤字（Tempate→Template） | `docs/` | リネーム |
| L-9 | ブランチが多数残存（tutorial/・0124refactor 等） | git | マージ済み・不要ブランチの削除 |

### 参考: 本プロジェクトで「準拠不要」と判断してよい項目

- **セーブシステム（§8）**: 放置ゲーの進行保存を実装する段になってから SaveChunk 導入を検討すればよい
- **Chickensoft.Sync（AutoChannel/AutoValue）への全面移行**: 生 `event Action` でも Repo イベントバスとしては機能している。新規 Repo から段階的に採用すれば十分（BattleRepo の `AutoProp` が先行例）
- **1状態1ファイル（§6.2）**: 現状の「1 Logic = 1ファイル」も一貫していれば可。ただし状態数が増えた Logic（BulletLogic 等）から分割を検討

---

## 3. 推奨事項

### 3-1. 直近の推奨アクション（順序つき）

1. **H-1〜H-3 の Dispose 漏れ修正**（半日以内の作業量、効果大）
2. **H-4 SpSkillEffectArea の Logic 化**（開発中の機能のうちに直すのが最も安い）
3. **M-10 csproj 堅牢化**（3行の変更でビルドの安全性が上がる）
4. **H-5 テスト整備の着手**（下記ロードマップ）

### 3-2. テスト整備ロードマップ

基盤（GoDotTest / GodotTestDriver / LightMoq / Shouldly / coverage / CI）は既に揃っているため、書き始めるだけの状態:

1. **状態テストから**: `ShipLogic` / `BattleUILogic` など純粋な遷移テスト（ARCHITECTURE_REFERENCE.md §10.1）。Godot 実行不要で速い
2. **Repo テスト**: `BattleRepo` のダメージ計算・スコア加算はドメインロジックの中核。モック不要でテストしやすい
3. **View テスト**: FakeBinding + FakeNodeTree（§10.2）。まず Ship から
4. `test/src/` を `src/` のミラー構成に保ち、CI の Visual Tests でカバレッジバッジを更新

### 3-3. Claude Code 設定（今回整備済み）

| ファイル | 内容 |
| --- | --- |
| `.claude/settings.json` | ①permissions.allow: `dotnet build/test/format`・git 読み取り系を許可（プロンプト削減） ②PreToolUse hook: `*.g.puml` と `.generated/` への Edit/Write をブロック（「自動生成ファイル手動編集禁止」規約の強制） |
| `.claude/skills/new-feature/SKILL.md` | 新機能追加のスキャフォールド手順（本プロジェクトの実態規約準拠） |
| `.claude/skills/arch-review/SKILL.md` | ARCHITECTURE_REFERENCE.md §12 ベースの差分レビュー手順 |
| `CLAUDE.md` | ステートマシン配置規則を実態（`src/{entity}/state/`）に修正、ARCHITECTURE_REFERENCE.md への参照を追加 |

**追加で検討する価値がある設定**（今回は未実施）:

- **PostToolUse hook で `dotnet format` 自動実行**: `.cs` 編集後に自動フォーマット。ビルド時間とのトレードオフがあるため必要になったら導入
- **Stop hook で `dotnet build` 警告チェック**: セッション終了時に警告ゼロを検証（editorconfig の warning 化と相性が良い）。ビルドが重い場合は却下してよい
- **`/verify` 用のプロジェクトスキル**: Godot をヘッドレス起動してスモークテストする手順を skill 化すると、Claude が変更検証を自走できる

### 3-4. その他の推奨

- **CLAUDE.md のダイエット**: 現在 580 行超で、Chickensoft の一般的な使い方説明が大半。ARCHITECTURE_REFERENCE.md と重複する解説を削り「本プロジェクト固有の規約・逸脱点」に絞ると、毎セッションのコンテキスト消費が減り指示の遵守率も上がる
- **「仮コミット」の抑制**: コミット履歴に「仮コミット 要リファクタリング」が散見される。WIP は feature ブランチ内で `git commit --fixup` や squash 前提にし、main 系へ入れない運用を推奨
- **docs の整理**: `コンテキスト履歴.md` は Claude の自動メモリと役割が重複。ARCHITECTURE_REFERENCE.md / PROJECT_REVIEW.md / 各アーキテクチャ文書（Bullet/Enemy）に集約する
