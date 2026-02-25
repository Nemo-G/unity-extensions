# 开发经验文档

## 编写原则
- **通用性**: 避免游戏特定术语，经验应适用于各种游戏开发
- **简洁性**: 描述简洁明了，避免冗余，代码示例只保留关键部分

## 基本信息
- **创建日期**: 2026-02-25
- **相关任务**: Angry Birds 风格游戏开发
- **技术栈**: Unity 2D

## 问题描述

### 问题表现
1. 运行时 Console 出现警告：`There are no audio listeners in the scene. Please ensure there is always one audio listener in the scene`
2. 代码中使用 `Camera.main` 返回 null，导致 NullReferenceException
3. 游戏对象中使用 `mainCamera.ScreenToWorldPoint()` 时抛出空引用异常

### 触发条件
- 主相机 GameObject 的 Tag 未设置为 "MainCamera"
- 主相机上缺少 AudioListener 组件
- 代码依赖 `Camera.main` 获取主相机引用

## 解决方案

### 关键步骤
1. 确保主相机 GameObject 的 Tag 设置为 "MainCamera"
2. 确保主相机上有 AudioListener 组件
3. 使用 `execute_csharp_script` 验证场景配置

### 关键代码
```csharp
// 验证脚本 - 检查相机配置
var mainCam = Camera.main;
if (mainCam == null)
{
    Debug.LogError("No Main Camera found! Check if camera tag is 'MainCamera'");
}

var audioListener = mainCam?.GetComponent<AudioListener>();
if (audioListener == null)
{
    Debug.LogWarning("Main Camera missing AudioListener!");
}
```

```csharp
// 修复脚本 - 自动配置
var mainCam = GameObject.Find("Main Camera");
if (mainCam != null)
{
    mainCam.tag = "MainCamera";
    if (mainCam.GetComponent<AudioListener>() == null)
    {
        mainCam.AddComponent<AudioListener>();
    }
}
```

### 最终方案
1. 主相机必须设置 Tag 为 "MainCamera"，这样 `Camera.main` 才能正确获取
2. 主相机必须添加 AudioListener 组件，否则音频无法播放
3. 在场景搭建完成后，使用验证脚本检查配置是否正确

### 验证方式
使用 `execute_csharp_script` 执行检查脚本，确认：
- `Camera.main` 返回非 null
- AudioListener 存在且数量为 1

---
**文档版本**: v4.0
**维护者**: Experience Manager
**最后更新**: 2026-02-25
