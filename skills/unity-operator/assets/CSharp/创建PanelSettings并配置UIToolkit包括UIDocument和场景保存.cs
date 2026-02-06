using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace EditorAutomation
{
    public class CreatePanelSettingsAndUIToolkitConfig_20260104_093500
    {
        public static string execute()
        {
            try
            {
                Debug.Log("=== 开始创建Panel Settings并配置UI Toolkit ===");
                
                string result = "";
                string panelSettingsPath = "Assets/UI Toolkit/GamePanelSettings.asset";
                string uxmlPath = "Assets/UI Toolkit/MainScreen.uxml";
                
                // 1. 确保目录存在
                string dirPath = Path.GetDirectoryName(panelSettingsPath);
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                    Debug.Log("创建目录: " + dirPath);
                }
                
                // 2. 创建Panel Settings资源
                var panelSettings = ScriptableObject.CreateInstance<UnityEngine.UIElements.PanelSettings>();
                if (panelSettings == null)
                {
                    throw new Exception("无法创建PanelSettings实例");
                }
                
                // 配置PanelSettings参数
                panelSettings.scaleMode = UnityEngine.UIElements.PanelScaleMode.ScaleWithScreenSize;
                panelSettings.referenceResolution = new Vector2Int(1920, 1080);
                panelSettings.match = 0.5f; // Match Width Or Height: 0.5
                panelSettings.sortingOrder = 0;
                
                // 创建asset文件
                AssetDatabase.CreateAsset(panelSettings, panelSettingsPath);
                AssetDatabase.Refresh();
                Debug.Log("成功创建Panel Settings: " + panelSettingsPath);
                
                // 3. 创建UIDocument GameObject
                var uidocumentGO = new GameObject("UIDocument_MainScreen");
                var uidocument = uidocumentGO.AddComponent<UnityEngine.UIElements.UIDocument>();
                Debug.Log("创建UIDocument GameObject: " + uidocumentGO.name);
                
                // 4. 关联Panel Settings资源
                var panelSettingsAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.PanelSettings>(panelSettingsPath);
                if (panelSettingsAsset != null)
                {
                    uidocument.panelSettings = panelSettingsAsset;
                    Debug.Log("成功关联Panel Settings到UIDocument");
                }
                else
                {
                    throw new Exception("无法加载Panel Settings资源: " + panelSettingsPath);
                }
                
                // 5. 关联MainScreen.uxml文件
                string uxmlFullPath = Path.Combine(Application.dataPath, "UI Toolkit", "MainScreen.uxml");
                if (File.Exists(uxmlFullPath))
                {
                    var uxmlAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.VisualTreeAsset>(uxmlPath);
                    if (uxmlAsset != null)
                    {
                        uidocument.visualTreeAsset = uxmlAsset;
                        Debug.Log("成功关联UXML文件: " + uxmlPath);
                    }
                    else
                    {
                        Debug.LogWarning("无法加载UXML资源: " + uxmlPath);
                    }
                }
                else
                {
                    Debug.LogWarning("UXML文件不存在，需要手动创建: " + uxmlPath);
                }
                
                // 6. 保存场景
                var currentScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
                if (!string.IsNullOrEmpty(currentScene.path))
                {
                    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(currentScene);
                    Debug.Log("场景已保存: " + currentScene.path);
                    result = currentScene.path;
                }
                else
                {
                    string newScenePath = "Assets/Scenes/MainScene.unity";
                    
                    dirPath = Path.GetDirectoryName(newScenePath);
                    if (!Directory.Exists(dirPath))
                    {
                        Directory.CreateDirectory(dirPath);
                    }
                    
                    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(currentScene, newScenePath);
                    Debug.Log("新场景已保存: " + newScenePath);
                    result = newScenePath;
                }
                
                Debug.Log("=== 所有任务执行成功 ===");
                return $"{{\"success\":true,\"panelSettingsPath\":\"{panelSettingsPath}\",\"uidocumentObject\":\"UIDocument_MainScreen\",\"scenePath\":\"{result}\",\"configuration\":{{\"scaleMode\":\"Scale With Screen Size\",\"referenceResolution\":{{\"x\":1920,\"y\":1080}},\"matchWidthOrHeight\":0.5,\"sortingOrder\":0}},\"message\":\"Panel Settings创建成功，UI Toolkit配置完成\"}}";
            }
            catch (Exception e)
            {
                Debug.LogError("操作失败: " + e.Message);
                Debug.LogError(e.StackTrace);
                return $"{{\"success\":false,\"message\":\"操作失败: {e.Message}\",\"error\":\"{e.StackTrace}\"}}";
            }
        }
    }
}