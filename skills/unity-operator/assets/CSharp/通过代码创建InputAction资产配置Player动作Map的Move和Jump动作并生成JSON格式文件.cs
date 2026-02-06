using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using System.IO;

namespace EditorAutomation
{
    public class InputAssetCreator
    {
        public static void execute()
        {
            // 1. 在内存中创建资源实例
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();

            // 2. 配置 Action Map 和 Actions
            var playerMap = asset.AddActionMap("Player");

            // Move 动作
            var moveAction = playerMap.AddAction("Move", InputActionType.Value, expectedControlLayout: "Vector2");
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

            // Jump 动作
            var jumpAction = playerMap.AddAction("Jump", InputActionType.Button);
            jumpAction.AddBinding("<Keyboard>/space");

            // 3. 获取 JSON 字符串 (这是关键！)
            string assetJson = asset.ToJson();

            // 4. 手动写入文件，后缀名必须是 .inputactions
            string path = "Assets/SimplePlayerControls.inputactions";
            File.WriteAllText(path, assetJson);

            // 5. 刷新资源数据库，让 Unity 的 InputActionImporter 接管该文件
            AssetDatabase.ImportAsset(path);
            AssetDatabase.Refresh();

            // 6. 销毁内存中的临时实例，避免内存泄漏
            Object.DestroyImmediate(asset);

            Debug.Log($"<color=green>成功！</color> 已正确生成 JSON 格式的 Input Asset: {path}");

            // 选中生成的资源
            Object obj = AssetDatabase.LoadMainAssetAtPath(path);
            Selection.activeObject = obj;
        }
    }
}