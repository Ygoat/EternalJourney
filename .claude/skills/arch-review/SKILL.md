---
name: arch-review
description: 現在の差分（またはブランチ）を EternalJourney のアーキテクチャ基準（docs/ARCHITECTURE_REFERENCE.md §12）でレビューする。「アーキレビューして」「設計観点でチェックして」という依頼で使用する。
---

# アーキテクチャレビュースキル

`docs/ARCHITECTURE_REFERENCE.md` §12 のチェックリストに基づき、変更されたコードをレビューする。

## 手順

1. `git diff`（未コミットなら working tree、指定があればブランチ間）で変更ファイル一覧を取得する。
2. 変更された `.cs` / `.tscn` ファイルを読み、以下の観点でチェックする。
3. 結果は「観点 / 該当箇所(ファイル:行) / 問題 / 推奨対応」の形式で日本語報告する。問題なしの観点も一行で明記する。

## チェックリスト

1. **View にロジックがないか** — 条件分岐によるゲームルール判定・ダメージ計算・スポーン判定などが View（Node派生クラス）にあれば LogicBlock / Repo へ移すよう指摘。`OnPhysicsProcess` 内の判断ロジックは特に注意。
2. **状態 vs フラグ** — 新しい bool フラグ（`_isDead`, `CanAttack`, `IsLoading` 等）は「状態」ではないか。状態なら LogicBlock の record 状態として表現させる。
3. **メッセージ型** — Input/Output が `readonly record struct` か。ハンドラは `in` 引数か。
4. **インターフェース** — 新しいノード・Logic・Repo に `I` プレフィックスのインターフェースがあるか。View が Logic/子ノードを具象型で保持していないか（テスト容易性）。
5. **後始末** — `OnTreeExiting()` で `Binding.Dispose()` / `Logic.Dispose()` / 所有 Repo の `Dispose()` / イベント購読解除（`+=` に対応する `-=`）が揃っているか。
6. **Logic 間の直接参照がないか** — ステートマシン間の連携は Repo のイベント/観測可能値を経由しているか。
7. **DI 規約** — `Setup()` で DI 依存に触っていないか。依存の使用は `OnResolved()` 以降か。`this.Provide()` の呼び忘れはないか。
8. **命名** — 定数 `ALL_UPPER_SNAKE`、private `_camelCase`、フォルダ snake_case、ファイル名=クラス名。タイポ・表記ゆれ（SP/Sp 等）も指摘。
9. **switch 式** — 網羅 + `_ =>` フォールバックがあるか。
10. **マジックナンバー** — ダメージ量・時間・半径などの数値リテラルは定数化または `data/masters/` の設定へ外出しされているか。
11. **デバッグ残骸** — `GD.Print` のデバッグ出力、コメントアウトされたコード、未使用変数が残っていないか。
12. **.g.puml** — 自動生成ファイルを手動編集していないか（差分に手動変更が含まれていたら警告）。

## 重大度の目安

- **高**: Dispose 漏れ（リーク）、View へのルール直書き、Logic 間直接参照
- **中**: bool フラグ状態、具象型依存、Setup での DI 使用
- **低**: 命名・タイポ・マジックナンバー・デバッグ残骸
