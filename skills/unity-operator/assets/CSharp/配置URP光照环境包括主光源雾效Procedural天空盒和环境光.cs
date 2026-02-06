using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace EditorAutomation
{
    public class ConfigureLightingEnvironment_20260104_001
    {
        public static string execute()
        {
            try
            {
                Debug.Log("【URP Procedural天空盒设置】开始配置...");
                
                // 1. 配置主方向光源
                Debug.Log("1. 配置主方向光源...");
                Light mainLight = GameObject.FindObjectOfType<Light>();
                
                if (mainLight == null)
                {
                    Debug.Log("  - 未找到现有光源，创建新的方向光源");
                    GameObject lightGo = new GameObject("Auto_CSharp_MainDirectionalLight");
                    mainLight = lightGo.AddComponent<Light>();
                    mainLight.type = LightType.Directional;
                }
                else
                {
                    Debug.Log($"  - 找到现有光源: {mainLight.gameObject.name}");
                }
                
                // 配置光源参数
                mainLight.color = new Color(1.0f, 0.95f, 0.8f);
                mainLight.intensity = 1.2f;
                mainLight.transform.rotation = Quaternion.Euler(50, -30, 0);
                mainLight.shadows = LightShadows.Soft;
                mainLight.shadowStrength = 0.7f;
                
                string lightName = mainLight.gameObject.name;
                Debug.Log($"  ✓ 主光源配置成功: {lightName}");
                
                // 2. 启用雾效
                Debug.Log("2. 配置雾效...");
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.ExponentialSquared;
                RenderSettings.fogDensity = 0.02f;
                RenderSettings.fogColor = new Color(0.7f, 0.8f, 0.9f, 1.0f);
                
                Debug.Log("  ✓ 雾效配置成功");
                
                // 3. 设置Procedural天空盒材质
                Debug.Log("3. 配置Procedural天空盒...");
                try
                {
                    Shader shader = Shader.Find("Skybox/Procedural");
                    
                    if (shader != null)
                    {
                        Debug.Log($"  - 使用着色器: {shader.name}");
                        
                        // 创建材质
                        Material skyboxMat = new Material(shader);
                        skyboxMat.name = "Auto_CSharp_ProceduralSkybox_URP";
                        
                        // 设置程序化天空盒参数
                        skyboxMat.SetColor("_SkyTint", new Color(0.38f, 0.58f, 0.78f));
                        skyboxMat.SetFloat("_AtmosphereThickness", 1.2f);
                        skyboxMat.SetFloat("_Exposure", 1.3f);
                        skyboxMat.SetFloat("_SunSize", 0.04f);
                        skyboxMat.SetFloat("_SunSizeConvergence", 5.0f);
                        
                        // 确保Materials文件夹存在
                        string folderPath = "Assets/Materials";
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                            AssetDatabase.Refresh();
                        }
                        
                        // 保存材质到项目
                        string materialPath = $"{folderPath}/{skyboxMat.name}.mat";
                        
                        // 创建资源前确保没有同名资源
                        if (File.Exists(materialPath))
                        {
                            AssetDatabase.DeleteAsset(materialPath);
                        }
                        
                        AssetDatabase.CreateAsset(skyboxMat, materialPath);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        
                        Debug.Log($"  ✓ 材质创建成功: {materialPath}");
                        
                        // 应用天空盒到渲染设置
                        RenderSettings.skybox = skyboxMat;
                        Debug.Log("  ✓ Procedural天空盒应用成功");
                    }
                    else
                    {
                        string errorMsg = "错误: 找不到Skybox/Procedural着色器，请确保项目包含此着色器";
                        Debug.LogError(errorMsg);
                        return $"{{\"success\": false, \"message\": \"{errorMsg}\", \"details\": {{\"errors\": [\"{errorMsg}\"]}}}}";
                    }
                }
                catch (Exception e)
                {
                    string errorMsg = $"天空盒配置失败: {e.Message}";
                    Debug.LogError(errorMsg);
                    return $"{{\"success\": false, \"message\": \"{errorMsg}\", \"details\": {{\"errors\": [\"{e.Message}\", \"{e.StackTrace}\"]}}}}";
                }
                
                // 4. 配置环境光
                Debug.Log("4. 配置环境光...");
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
                RenderSettings.ambientIntensity = 1.0f;
                Debug.Log("  ✓ 环境光配置成功");
                
                // 标记场景为已修改
                UnityEngine.SceneManagement.Scene activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
                if (activeScene != null)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(activeScene);
                    Debug.Log("  ✓ 场景已标记为已修改");
                }
                
                // 返回JSON格式的成功结果
                string successMessage = "✅ URP Procedural天空盒配置成功！";
                return $"{{\"success\": true, \"message\": \"{successMessage}\", \"details\": {{\"light_configured\": true, \"fog_enabled\": true, \"skybox_set\": true, \"material_created\": true, \"material_path\": \"Assets/Materials/Auto_CSharp_ProceduralSkybox_URP.mat\", \"shader_name\": \"Skybox/Procedural\", \"light_name\": \"{lightName}\", \"errors\": []}}}}";
            }
            catch (Exception e)
            {
                string errorMessage = $"【URP Procedural天空盒设置】配置失败: {e.Message}";
                Debug.LogError(errorMessage);
                return $"{{\"success\": false, \"message\": \"{errorMessage}\", \"details\": {{\"errors\": [\"{e.Message}\", \"{e.StackTrace}\"]}}}}";
            }
        }
    }
}
