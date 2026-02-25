---
name: ui-toolkit
description: UI Toolkit 开发技能 - 创建/更新 UXML/USS/PanelSettings，并配套实现 UI 脚本（UIDocument 绑定与数据刷新）。禁止 UGUI。脚本修改后必须 compile_and_validate。
---

# UI Toolkit

## 适用范围（强制）

- ✅ UXML / USS / PanelSettings 的创建与维护
- ✅ 与 UI Toolkit 配套的 UI 脚本（例如 HUD、胜负弹窗、按钮回调、数据刷新）
- ❌ 禁止 UGUI（Canvas/Image/Text/Button 等）

## 输出契约（必须）

为了让 `unity-executor` 能整合，你必须在输出中明确：
- UXML/USS/PanelSettings 的资产路径（稳定、可复用）
- UXML 中关键元素的 `name`（供脚本 Query）
- UI 脚本类名与文件路径
- 禁止用absolute position，为适配各种screen ratio，必须用relative position
- size使用百分比，禁止写死px
- 需要被 `unity-operator` 挂载/引用的点（例如：哪一个 GameObject 需要 `UIDocument`，需要引用哪个 `PanelSettings` 与 `VisualTreeAsset`）

## 工作流（必须）

1) 用 `todo_write` 拆任务（UXML、USS、PanelSettings、UI 脚本）
2) 资产创建：
   - UXML：根节点 + 明确的容器结构；关键显示元素必须有唯一 name（例如：`scoreLabel`、`movesLabel`、`targetLabel`、`resultPanel`、`restartButton`）
   - USS：只负责样式，不写行为
   - PanelSettings：如项目需要多套面板配置，按路径区分
3) 脚本实现：
   - UI 脚本只做 UI：绑定 `UIDocument`，Query 元素，暴露 `SetScore(int)` / `SetMoves(int)` / `ShowWin()` / `ShowLose()` 等接口
   - 不依赖具体游戏逻辑对象的 instanceID；通过事件/接口输入数据
4) 编译校验（脚本变更后必须）：
   - `unity_workflow { \"action\": \"compile_and_validate\" }` 直到无 error

## 常见失败与处理

- 找不到元素：先核对 UXML `name`，再核对脚本 Query 字符串
- USS 未生效：确认已 link 到 UXML 或在运行时加载
- 引用资产赋值失败（VisualTreeAsset/PanelSettings）：这是 `UnityEngine.Object` 引用赋值问题，若 builtin tools 转换失败，把 wiring 交给 `unity-coder` 用 `execute_csharp_script` 兜底

