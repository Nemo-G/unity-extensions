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
游戏场景中出现异常的大面积黄色/橙色矩形块，遮挡了大部分游戏内容。同时弹弓只显示一个叉子，另一个叉子不可见。

### 触发条件
- 背景层 Sprite 的位置不正确，导致 Sprite 中的地面/前景部分进入相机可见范围
- Sprite Sheet 切片后，多个子 Sprite 需要分别放置在不同位置
- 弹弓由两个独立的 Sprite 组成，需要创建两个 GameObject 分别显示

## 解决方案

### 关键步骤
1. 分析 Sprite 内容 - 使用 `analyze_multimedia` 检查 Sprite 图片的实际内容
2. 调整背景层位置 - 将背景 Sprite 移到相机可见范围之外
3. 创建多个 GameObject 显示 Sprite Sheet 的不同部分
4. 调整 GameObject 的 localPosition 确保完整显示

### 关键代码
```csharp
// 检查 Sprite 的 bounds 和位置
var sprite = sr.sprite;
Debug.Log($"Sprite bounds: {sprite.bounds}");
Debug.Log($"Sprite height in units: {sprite.rect.height / sprite.pixelsPerUnit}");

// 调整背景位置 - 移到相机上方
sky.transform.position = new Vector3(0, 15, 0);

// 创建多个 GameObject 显示 Sprite Sheet 的不同部分
var leftFork = new GameObject("LeftFork");
leftFork.transform.SetParent(slingshot.transform);
leftFork.transform.localPosition = new Vector3(-0.4f, 0.5f, 0);
var leftSr = leftFork.AddComponent<SpriteRenderer>();
leftSr.sprite = leftForkSprite; // slingshot_0
```

### 最终方案
1. **背景遮挡问题**：分析 Sprite 图片内容，发现 Sky Sprite 底部有大面积黄色地面区域。将 Sky 移到相机可见范围上方（y=15），让黄色区域不在屏幕内显示。

2. **弹弓显示问题**：Sprite Sheet 包含两个弹弓叉子（slingshot_0 和 slingshot_1），需要创建两个独立的 GameObject 分别显示，并调整 localPosition 让它们正确排列。

### 验证方式
使用 `analyze_multimedia` 分析截图，确认：
- 黄色矩形块不存在
- 弹弓完整显示（两个叉子）
- 鸟在弹弓上

---
**文档版本**: v4.0
**维护者**: Experience Manager
**最后更新**: 2026-02-25
