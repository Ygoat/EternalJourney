---
name: new-feature
description: EternalJourney に新機能（フィーチャー）を追加する際のスキャフォールド手順。View + LogicBlock + (必要なら) Repo を本プロジェクトの規約どおりに揃える。「新しい敵/スキル/UI/エンティティを追加して」という依頼で使用する。
---

# 新機能追加スキル

EternalJourney の規約に沿って新機能一式を作成する手順。詳細な設計原則は
`docs/ARCHITECTURE_REFERENCE.md`（特に §7 新機能追加の完全な一例）と `CLAUDE.md` を正とする。

## 手順

1. **配置を決める**: `src/{feature}/`（フォルダ名は snake_case）を作成する。
   - View: `src/{feature}/{Feature}.cs` + `{Feature}.tscn`
   - LogicBlock: `src/{feature}/state/{Feature}Logic.cs`（本プロジェクトは全状態を単一ファイルに定義。抽象基底を持つ場合のみ `abstract/state/`）
   - ドメイン横断ルールがあるなら: `src/{feature}/domain/{Feature}Repo.cs`

2. **View を作る**: `docs/ScriptTempate.md` のテンプレートに従う。必須事項:
   - `public interface I{Feature} : INode2D { }` を必ず定義（I プレフィックス）
   - `[Meta(typeof(IAutoNode))]` + `public override void _Notification(int what) => this.Notify(what);`
   - `#region` は `Signals / State / Exports / Nodes / Provisions / Dependencies` の定型区分のみ
   - 子ノードは `[Node]` + インターフェース型（`ICollisionShape2D` 等、具象型は使わない）
   - Logic はインターフェース型（`I{Feature}Logic`）で保持する
   - **View に条件分岐によるゲームルールを書かない**。判断はすべて LogicBlock へ、横断ルールは Repo へ

3. **ライフサイクルの分担を守る**:
   - `Setup()`: Logic の生成・`Bind()`（DI 依存には触らない）
   - `OnResolved()`: `Logic.Set(依存)` → Binding の `.When<>()` / `.Handle<>()` 定義 → `Start()`
   - `OnTreeExiting()`: `LogicBinding.Dispose()` と `Logic.Dispose()` を必ず呼ぶ。イベント購読があれば解除。Repo を所有しているなら `Repo.Dispose()` も

4. **LogicBlock を作る**: CLAUDE.md「LogicBlocksパターン」のプロジェクト流儀に従う。
   - `[Meta, LogicBlock(typeof(State), Diagram = true)]`
   - Input/Output は `static class Input / Output` 内の `readonly record struct`
   - 状態は「ロード中」「攻撃中」「死亡」等をすべて record 状態として列挙する。**bool フラグで状態を持たない**
   - Logic 同士は直接参照しない。他の機能との連携は Repo のイベント経由

5. **DI 接続**: 依存は `[Dependency] ... this.DependOn<T>()`、提供側は `IProvide<T>` + `OnResolved()`（または OnReady）での `this.Provide()`。Provider は必ずシーンツリーの上位に置く。

6. **マスターデータが必要なら**: `data/masters/{Feature}Config.json` + `src/cores/repositories/` の `BaseJsonReader` 派生で読み込む（`BulletConfigReader` が参考実装）。

7. **仕上げ**: `dotnet build` で警告ゼロを確認。`.g.puml` は自動生成されるので手で触らない。

## 参考実装（模範例）

- View + Logic の分離: `src/ship/Ship.cs` + `src/ship/state/ShipLogic.cs`
- Repo イベント連携: `src/battle/domain/BattleRepo.cs` と `src/game/state/GameLogic.cs`
- ストラテジー切替: `src/bullet/strategies/` + `data/masters/BulletConfig.json`
