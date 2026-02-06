---
name: unity-operator
description: Unity Editor自动化技能 - 优先使用 unity_workflow / unity_* builtin tools（含 create_batch/edit_batch）完成 State → Action → Verify → Correct；仅当内置工具不足时，才使用 execute_csharp_script 在 Editor 中执行短小、安全、可验证的 C# 脚本来完成复杂/批量逻辑。包含脚本模板、验证流程和 few-shot 示例。
---

# 操作者 - Unity Editor自动化技能

## Overview

将用户的自然语言需求转化为可执行的 Unity 工具调用（必要时补充短小的 Unity C# 编辑器脚本），通过工具链安全、可靠地执行编辑器自动化操作。

## 核心工作流程

### 步骤1：准备工作

0. **推荐**：优先使用 `unity_workflow { "action": "init_session" }` 一次完成「状态同步 + 清空 Console（拿到 since_token）+ 等待 idle」。

1. 使用 `unity_editor` 同步状态（推荐，但**写操作 tools 会自动同步并注入 `client_state_rev`**；不要手动填写 `client_state_rev`）
2. 使用 `unity_console` 清空Unity Console日志，记录 `sinceToken`（用于只读取本次操作的新日志）
3. 使用 `unity_editor` 等待Unity空闲（`wait_for_idle`）
4. （可选）检查 skill 的 `assets/CSharp/` 文件夹，查找类似代码文件作为参考

### 步骤2：理解需求并拆解为可验证的操作

1. 明确目标对象/场景/资产的定位方式（名称、层级路径、GUID、选择集）
2. 优先选择 builtin tools（`unity_*` / `unity_workflow`）完成：State → Action → Verify → Correct
   - 对于重复/连续操作，只有在 **ops 完全确定**（不需要“做完一步再看结果决定下一步”）时，才用 `create_batch` / `edit_batch` 压缩调用数
     - ✅ `create_batch`：**只做写**，做完再用 read-only 工具观测（console/hierarchy/get_info）
     - ✅ `edit_batch`：**先 find/search 再写**；若 find/search 到 0 个会 early-stop 且不算 error
     - ✅ 每个 batch **≤ 10 ops**
     - ✅ 写批里 target/parent 必须是 **instanceID / $alias / hierarchy_path(by_path)**（避免 by_name）
     - ✅ 新建对象链路：优先用 `unity_gameobject { "action": "create_batch" }`（只写，不依赖 find）
     - ✅ 编辑已有对象：优先用 `unity_gameobject { "action": "edit_batch" }`（先 find + captureAs，再用 `$alias` 写；若 find 到 0 个会 early-stop 且不算 error）
   - 对于「脚本编译 + Console 校验」连续模式，优先用 `unity_workflow { "action": "compile_and_validate" }`
   - 对于“装包 → 域加载/编译 → Console 校验”链路，优先用 `unity_workflow { "action": "install_package_and_validate", "id_or_url": "com.xxx" }`
   - **组件属性设置（关键）**：优先用 `unity_gameobject` 的 *批量形态*，减少填参出错
     - ✅ 推荐（批量）：`{ action: "set_component_properties" | "set_component_property", targetRef/target, componentName, componentProperties }`
     - ✅ 兼容（简写）：`{ action: "set_component_property", targetRef/target, componentName, propertyName, propertyValue }`
     - ⚠️ 若出现参数校验/类型转换/成员不存在等错误 1 次以上：直接切换到 `execute_csharp_script`，用最小脚本完成属性赋值并 `Debug.Log` 验证
3. 当 builtin tools（含 batch/workflow）仍不足以表达/需要复杂自定义逻辑时，才切换到 `execute_csharp_script`

### 步骤3：编写脚本（execute_csharp_script 模式）

直接准备要执行的 C# script 字符串（无需创建 `.cs` 文件），并通过 `execute_csharp_script` 执行。

关键点：
- 脚本运行在 Unity Editor 进程内（Roslyn C# Scripting），默认已导入常用命名空间（如 `System`、`System.Linq`、`UnityEngine`、`UnityEditor`、`UnityEditor.SceneManagement`、`UnityEngine.SceneManagement`），通常可直接写 `Debug.Log(...)` / `Selection...` / `EditorSceneManager...`。
- 使用“可验证”输出：**一定要 `Debug.Log` 关键结果**；脚本最后一行**可选**返回一个字符串结果（推荐，出现在 tool 的 `data.result`）。
- 避免长时间阻塞：不要写无限循环/大范围遍历不加限制。

#### 长脚本 / 落盘执行策略（Python → lint → 转译 C#）

当满足以下任一条件时，**不要直接写很长的 C#**：
- 需要写 **超过10行** 的 C# 脚本（可读性与错误率会显著变差）
- 需要创建/修改 `.cs` 文件并走编译后再执行（例如要复用、要进入版本库、或需要更复杂的结构）

推荐流程：
1. **确保安装 Unity Python**：项目需包含 `com.unity.scripting.python`（可用 `unity_package.install_package` 安装）
2. **先用 Python 写出同等逻辑**：把核心算法/遍历/过滤/边界条件写清楚，并输出可验证日志
3. **先跑 lint / 语法检查**（至少要做到“无语法错误”）：
   - 推荐：`ruff` / `flake8`（如果环境可用）
   - 最低要求：`python -m py_compile your_script.py` 或对字符串做 `compile(...)` 的语法检查
4. **跑通 Python 行为后**，由 LLM **逐句转译为等价 C#**（保持同样的边界条件、日志、返回值语义）
5. 最终再选择执行方式：
   - **短小逻辑**：用 `execute_csharp_script` 分段执行（每段尽量 ≤10 行），每段都验证日志/结果
   - **文件化逻辑**：用 `unity_script` 写入 `.cs` → `unity_editor.request_compile` → `unity_editor.wait_for_compile` → `unity_console` 仅看新日志 → 再执行/验证

### 步骤4：执行脚本

调用 `execute_csharp_script`（建议 `capture_logs: true`），拿到 `data.logs` 和 `data.result`。

### 步骤5：验证与纠错

1. 检查 tool 返回的 `data.logs` 与 `data.result`
2. 必要时调用 `unity_console` 用 `sinceToken` 读取本次操作产生的新日志
3. 通过 read-only 工具确认状态（例如：`unity_scene.get_hierarchy` / `unity_gameobject.find` / `unity_asset.get_info`）
4. 若执行出现错误，回到步骤3调整脚本并重试

### 步骤6：关键边界处理（需要时）

1. 如果脚本修改了 Scene / Prefab / Asset 等，需要按任务边界调用 `unity_scene.ensure_scene_saved` 或相关 ensure/save 工具进行持久化
2. 如果涉及脚本文件变更（常见于 `unity_script` 编辑），**优先**走 `unity_workflow { "action": "compile_and_validate" }`；若不可用再手动走 `unity_editor.start_compilation_pipeline` → `unity_editor.wait_for_compile` → `unity_console` 增量校验链路

### 步骤7：检查任务执行结果
1. 调用 `unity_console` 获取任务执行的日志
2. 若执行出现错误，请回到步骤3重新修改代码并重新执行
3. 确保prd文档中所有story都成功完成

### 步骤8：更新任务状态
3. 若执行成功，更新prd文档，把已完成story的passes设置为true
4. 如果整个prd.json文档内所有story都已经完成，所有passes都为true，这时候把该文档的文件名加上后缀-down， 比如场景搭建-done.json

## 高频压缩工具（减少多步调用）

### unity_workflow（推荐优先用）

- `unity_workflow { "action": "init_session" }`
  - 内部：`unity_editor.get_current_state` → `unity_console.clear` → `unity_editor.wait_for_idle`
- `unity_workflow { "action": "compile_and_validate" }`
  - 内部：`unity_editor.start_compilation_pipeline` → `unity_editor.wait_for_compile(op_id)` → `unity_console.get(since_token)`
  - 输出：`hasErrors` / `hasWarnings`（基于 Console 新日志判定）
- `unity_workflow { "action": "checkpoint" }`
  - 内部：`unity_scene.ensure_scene_saved` →（可选）`unity_screenshot.capture*`

### `unity_asset` / `unity_gameobject` 的 `create_batch` / `edit_batch` actions

当你发现自己在重复调用同一个工具（例如连续 create/modify/add_component/ensure_*），优先改用 create_batch / edit_batch：

```json
{
  "action": "create_batch",
  "mode": "stop_on_error",
  "ops": [
    { "id": "1", "action": "create", "params": { "name": "Root", "primitiveType": "Cube" }, "captureAs": "$root" },
    { "id": "2", "action": "create", "params": { "name": "Child", "parent": "$root" } }
  ]
}
```

> 说明：
> - `unity_gameobject.create_batch` 支持用 `captureAs: "$alias"` 绑定新建对象的 instanceID，并在后续 op 的 params 中用 `"$alias"` 或 `{ "ref": "$alias" }` 引用，减少额外的 find。
> - `unity_gameobject.edit_batch`：用 `find` op 的 `captureAs` 抓取 instanceID，后续写 op 必须用 `$alias` 作为 target/parent。
> - `unity_asset.edit_batch`：用 `search` op 的 `captureAs` 抓取 asset path，后续写 op 必须用 `$alias` 作为 path。
>
> ⚠️ 约束：写批里不要用 by_name 的 target/parent（名字定位属于“需要观测确认”的步骤，应拆成：先读 → 再写批）。

## 代码生成规范

### execute_csharp_script 脚本模板（推荐）

```csharp
using System;
using UnityEngine;
using UnityEditor;

string result;
try
{
    // ====================
    // 具体的操作逻辑代码（尽量写成可重复执行/可验证）
    // ====================

    Debug.Log("[unity-operator] OK");
    result = "success";
}
catch (Exception e)
{
    Debug.LogError("[unity-operator] FAILED: " + e);
    result = "failed: " + e.Message;
}

result;
```

### 关键要点

- **脚本最后一行可选返回字符串（推荐）**：tool 的 `data.result` 会带回该返回值；不返回也可以，主要依赖日志验证
- **必须输出可验证日志**：用 `Debug.Log/Warning/Error` 打点关键结果（数量、路径、对象名、是否找到等）
- 使用正确的Unity API和Editor API（Editor环境）
- 尽量写成幂等：重复执行不会产生额外副作用（或可检测并跳过）

### ⚠️ 禁止事项

- **禁止编写任何弹出对话框的代码**
- 不要使用`EditorUtility.DisplayDialog()`或类似弹窗函数
- 不要使用`EditorUtility.DisplayDialogComplex()`或任何GUI弹窗
- 只能通过`Debug.Log()`、`Debug.LogWarning()`、`Debug.LogError()`输出信息到控制台

### 编码规范

- **错误处理**: 必须使用 try-catch 包裹所有逻辑
- **返回值**: 最后一行返回 string，建议 `"success"` / `"failed: ..."` 这种人类可读结果
- **日志**: 统一前缀（如 `[unity-operator]`）便于筛选

## 并发安全

### 文件命名

- execute_csharp_script 模式无需创建文件
- 如必须创建临时脚本文件，使用动态路径：`Assets/Temp/{ActionName}_{timestamp}.cs`

### 错误隔离

- 每次操作使用独立的脚本/逻辑块
- 一个操作的失败不影响其他操作（可通过返回值和日志定位）

## 示例（execute_csharp_script）

```csharp
// 1) Log Message
UnityEngine.Debug.Log("Hello from C#!");

// 2) Get Scene Info
var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
UnityEngine.Debug.Log($"Active Scene: {scene.name}");
UnityEngine.Debug.Log($"Scene Path: {scene.path}");
UnityEngine.Debug.Log($"Root Object Count: {scene.rootCount}");

// 3) List GameObjects
var gameObjects = UnityEngine.Object.FindObjectsOfType<UnityEngine.GameObject>();
foreach (var go in gameObjects)
{
    if (go.transform.parent == null)
        UnityEngine.Debug.Log($"Root GameObject: {go.name}");
}

// 4) Create GameObject
var go = new UnityEngine.GameObject("MyObject");
go.AddComponent<UnityEngine.Light>();
UnityEngine.Debug.Log($"Created GameObject: {go.name}");

// 5) Get Project Path
UnityEngine.Debug.Log($"Data Path: {UnityEngine.Application.dataPath}");
UnityEngine.Debug.Log($"Persistent Data Path: {UnityEngine.Application.persistentDataPath}");
```

## Bundled Assets

本技能包含以下 C# 编辑器脚本资产（位于 `assets/CSharp/`），可作为参考模板或直接复制到项目中使用：

### 脚本清单

1. **包管理器自动化检查安装包并获取元数据.cs** - 自动化检查和管理Unity包
2. **测试Unity碰撞矩阵设置包括添加Layer获取ID查询和验证矩阵.cs** - 碰撞层配置测试
3. **创建3个URPUnlitShader材质红蓝绿并保存.cs** - URP材质创建
4. **创建PanelSettings并配置UIToolkit包括UIDocument和场景保存.cs** - UI Toolkit配置
5. **创建Player预制体并保存到AssetsPrefabsPlayerprefab然后实例化5个对象在指定位置验证预制体链接和组件完整性.cs** - 预制体创建和验证
6. **配置PlayerInput组件并加载InputActionAsset.cs** - 输入系统配置
7. **配置URP光照环境包括主光源雾效Procedural天空盒和环境光.cs** - URP光照配置
8. **批量查找BatchCube对象并按名称排序水平排列从x负6间距1点5创建橙色材质保存到AssetsTempMaterials并应用到MeshRenderer.cs** - 批量对象处理
9. **批量创建立方体对象可配置随机生成工具通过环境变量接收参数控制创建数量和名称前缀并在随机位置生成指定数量的立方体对象并记录创建信息.cs** - 批量对象生成
10. **批量给名称以BatchCube_开头的对象添加Rigidbody组件并配置参数以及修改BoxCollider尺寸.cs** - 批量组件添加
11. **通过代码创建InputAction资产配置Player动作Map的Move和Jump动作并生成JSON格式文件.cs** - InputAction资产创建
12. **为MainCamera查找或创建并添加CameraController组件并设置目标偏移平滑速度参数.cs** - 相机控制器配置
13. **物理模拟测试创建场景刚体力场并验证物理参数.cs** - 物理模拟测试
14. **Layer操作检查添加设置查询验证测试对象并返回JSON.cs** - Layer管理工具
15. **Tag操作检查添加设置查询验证测试对象并返回JSON.cs** - Tag管理工具

### 使用方式

这些脚本可以作为：
- **参考模板**：在编写新脚本时参考这些示例的代码结构
- **直接使用**：将脚本复制到项目的 `Assets/Temp/` 目录并执行
- **学习资源**：了解Unity编辑器API的常见用法

注意：这些assets文件不会被加载到上下文中，需要时通过文件系统工具读取。
