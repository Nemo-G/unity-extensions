using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Collections.Generic;

namespace EditorAutomation
{
    public class LayerOperation_CheckAddSetQueryVerify_CreateTestObjectReturnJSON_20250104
    {
        // Helper class for JSON serialization
        [Serializable]
        private class OperationResult
        {
            public bool step1_layer_exists_check;
            public bool step2_layer_added;
            public bool step3_test_object_created;
            public bool step4_layer_set_to_default;
            public bool step5_layer_query_verified_default;
            public bool step6_layer_set_to_custom;
            public bool step7_layer_query_verified_custom;
            public bool step8_all_layers_queried;
            public List<string> messages = new List<string>();
            public string final_result;
            public long timestamp;
        }

        // 检查Layer是否存在
        public static bool CheckLayerExists(string layerName)
        {
            try
            {
                string[] allLayers = InternalEditorUtility.layers;
                foreach (string layer in allLayers)
                {
                    if (layer == layerName)
                        return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"检查Layer失败: {e.Message}");
                return false;
            }
        }

        // 添加Layer到项目
        public static (bool success, string message) AddLayer(string layerName)
        {
            try
            {
                if (CheckLayerExists(layerName))
                    return (true, $"Layer '{layerName}' 已存在，跳过添加");

                // 获取TagManager资源路径
                string tagManagerPath = "ProjectSettings/TagManager.asset";
                UnityEngine.Object[] tagManagerAssets = AssetDatabase.LoadAllAssetsAtPath(tagManagerPath);
                
                if (tagManagerAssets != null && tagManagerAssets.Length > 0)
                {
                    SerializedObject serializedObject = new SerializedObject(tagManagerAssets[0]);
                    SerializedProperty layersProp = serializedObject.FindProperty("layers");
                    
                    if (layersProp != null && layersProp.isArray)
                    {
                        // 查找第一个空的User Layer (从索引8开始，前8个是内置的)
                        for (int i = 8; i < 32; i++)
                        {
                            if (i < layersProp.arraySize)
                            {
                                SerializedProperty layerProp = layersProp.GetArrayElementAtIndex(i);
                                if (string.IsNullOrEmpty(layerProp.stringValue))
                                {
                                    layerProp.stringValue = layerName;
                                    serializedObject.ApplyModifiedProperties();
                                    AssetDatabase.SaveAssets();
                                    return (true, $"Layer '{layerName}' 添加成功 (索引: {i})");
                                }
                            }
                        }
                        
                        return (false, $"添加Layer失败：所有User Layer槽位（8-31）已满");
                    }
                    else
                    {
                        return (false, $"添加Layer失败：无法找到layers属性");
                    }
                }
                else
                {
                    return (false, $"添加Layer失败：无法加载TagManager.asset");
                }
            }
            catch (Exception e)
            {
                return (false, $"添加Layer失败: {e.Message}");
            }
        }

        // 获取Layer ID
        public static int GetLayerId(string layerName)
        {
            try
            {
                // 使用LayerMask.NameToLayer
                int layerId = LayerMask.NameToLayer(layerName);
                return layerId;
            }
            catch (Exception e)
            {
                Debug.LogError($"获取Layer ID失败: {e.Message}");
                return -1;
            }
        }

        // 设置对象的Layer
        public static (bool success, string message) SetObjectLayer(GameObject gameObj, string layerName)
        {
            try
            {
                int layerId = GetLayerId(layerName);
                if (layerId < 0)
                    return (false, $"设置Layer失败: Layer '{layerName}' 不存在");
                
                gameObj.layer = layerId;
                return (true, $"对象Layer设置为 '{layerName}' (ID: {layerId}) 成功");
            }
            catch (Exception e)
            {
                return (false, $"设置Layer失败: {e.Message}");
            }
        }

        // 获取对象的Layer名称
        public static (string layerName, string message) GetObjectLayer(GameObject gameObj)
        {
            try
            {
                int layerId = gameObj.layer;
                string layerName = LayerMask.LayerToName(layerId);
                return (layerName, "获取Layer成功");
            }
            catch (Exception e)
            {
                return ("", $"获取Layer失败: {e.Message}");
            }
        }

        // 获取项目所有可用的Layers
        public static (string[] layers, string message) GetAllLayers()
        {
            try
            {
                string[] allLayers = InternalEditorUtility.layers;
                return (allLayers, "获取所有Layers成功");
            }
            catch (Exception e)
            {
                return (new string[0], $"获取所有Layers失败: {e.Message}");
            }
        }

        // 创建测试对象（Cube）
        public static (GameObject testObj, string message) CreateTestObject(string objectName = "LayerTestObject")
        {
            try
            {
                GameObject testObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                testObj.name = objectName;
                return (testObj, $"测试对象 '{objectName}' 创建成功");
            }
            catch (Exception e)
            {
                return (null, $"创建测试对象失败: {e.Message}");
            }
        }

        // 主执行函数
        public static string execute()
        {
            OperationResult result = new OperationResult();
            result.timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            
            string testLayerName = "TestLayer";
            GameObject testObject = null;
            
            try
            {
                Debug.Log("=== 开始Layer操作测试 ===");
                result.messages.Add("=== 开始Layer操作测试 ===");
                
                // 步骤1: 检查Layer是否存在
                Debug.Log("步骤1：检查 'TestLayer' 是否存在");
                result.messages.Add("步骤1：检查 'TestLayer' 是否存在");
                
                bool layerExists = CheckLayerExists(testLayerName);
                result.step1_layer_exists_check = true;
                result.messages.Add($"  - Layer存在性：{layerExists}");
                Debug.Log($"  - Layer存在性：{layerExists}");
                
                // 步骤2: 添加Layer（如需要）
                Debug.Log("步骤2：添加 'TestLayer' 到项目（如需要）");
                result.messages.Add("步骤2：添加 'TestLayer' 到项目（如需要）");
                
                var addResult = AddLayer(testLayerName);
                bool addSuccess = addResult.success;
                string addMessage = addResult.message;
                result.step2_layer_added = addSuccess;
                result.messages.Add($"  - {addMessage}");
                Debug.Log($"  - {addMessage}");
                
                if (!addSuccess && !layerExists)
                {
                    throw new Exception($"Layer添加失败：{addMessage}");
                }
                
                // 步骤3: 创建测试对象
                Debug.Log("步骤3：创建测试对象（Cube）");
                result.messages.Add("步骤3：创建测试对象（Cube）");
                
                var createResult = CreateTestObject("LayerTestCube");
                testObject = createResult.testObj;
                string createMessage = createResult.message;
                result.step3_test_object_created = testObject != null;
                result.messages.Add($"  - {createMessage}");
                Debug.Log($"  - {createMessage}");
                
                if (testObject == null)
                {
                    throw new Exception($"测试对象创建失败：{createMessage}");
                }
                
                // 步骤4: 设置Layer为Default
                Debug.Log("步骤4：设置对象Layer为 'Default'");
                result.messages.Add("步骤4：设置对象Layer为 'Default'");
                
                var setResult1 = SetObjectLayer(testObject, "Default");
                bool setSuccess1 = setResult1.success;
                string setMessage1 = setResult1.message;
                result.step4_layer_set_to_default = setSuccess1;
                result.messages.Add($"  - {setMessage1}");
                Debug.Log($"  - {setMessage1}");
                
                if (!setSuccess1)
                {
                    throw new Exception($"设置Default Layer失败：{setMessage1}");
                }
                
                // 步骤5: 查询验证Default Layer
                Debug.Log("步骤5：查询并验证对象Layer为 'Default'");
                result.messages.Add("步骤5：查询并验证对象Layer为 'Default'");
                
                var queryResult1 = GetObjectLayer(testObject);
                string actualLayer1 = queryResult1.layerName;
                string queryMessage1 = queryResult1.message;
                bool layerCorrect1 = actualLayer1 == "Default";
                result.step5_layer_query_verified_default = layerCorrect1;
                result.messages.Add($"  - 期望: Default, 实际: {actualLayer1}");
                result.messages.Add($"  - 验证结果: {(layerCorrect1 ? "✓ 正确" : "✗ 错误")}");
                Debug.Log($"  - 期望: Default, 实际: {actualLayer1}");
                Debug.Log($"  - 验证结果: {(layerCorrect1 ? "✓ 正确" : "✗ 错误")}");
                
                if (!layerCorrect1)
                {
                    throw new Exception($"Default Layer验证失败：期望 'Default'，实际 '{actualLayer1}'");
                }
                
                // 步骤6: 设置Layer为自定义层
                Debug.Log($"步骤6：设置对象Layer为自定义层 '{testLayerName}'");
                result.messages.Add($"步骤6：设置对象Layer为自定义层 '{testLayerName}'");
                
                var setResult2 = SetObjectLayer(testObject, testLayerName);
                bool setSuccess2 = setResult2.success;
                string setMessage2 = setResult2.message;
                result.step6_layer_set_to_custom = setSuccess2;
                result.messages.Add($"  - {setMessage2}");
                Debug.Log($"  - {setMessage2}");
                
                if (!setSuccess2)
                {
                    throw new Exception($"设置自定义Layer失败：{setMessage2}");
                }
                
                // 步骤7: 查询验证自定义Layer
                Debug.Log($"步骤7：查询验证自定义Layer '{testLayerName}'");
                result.messages.Add($"步骤7：查询验证自定义Layer '{testLayerName}'");
                
                var queryResult2 = GetObjectLayer(testObject);
                string actualLayer2 = queryResult2.layerName;
                string queryMessage2 = queryResult2.message;
                bool layerCorrect2 = actualLayer2 == testLayerName;
                result.step7_layer_query_verified_custom = layerCorrect2;
                result.messages.Add($"  - 期望: {testLayerName}, 实际: {actualLayer2}");
                result.messages.Add($"  - 验证结果: {(layerCorrect2 ? "✓ 正确" : "✗ 错误")}");
                Debug.Log($"  - 期望: {testLayerName}, 实际: {actualLayer2}");
                Debug.Log($"  - 验证结果: {(layerCorrect2 ? "✓ 正确" : "✗ 错误")}");
                
                if (!layerCorrect2)
                {
                    throw new Exception($"自定义Layer验证失败：期望 '{testLayerName}'，实际 '{actualLayer2}'");
                }
                
                // 步骤8: 查询所有Layer
                Debug.Log("步骤8：查询项目所有Layer信息");
                result.messages.Add("步骤8：查询项目所有Layer信息");
                
                var allLayersResult = GetAllLayers();
                string[] allLayers = allLayersResult.layers;
                string allLayersMessage = allLayersResult.message;
                result.step8_all_layers_queried = true;
                result.messages.Add($"  - {allLayersMessage}");
                result.messages.Add($"  - Layer总数: {allLayers.Length}");
                result.messages.Add($"  - 所有Layers: {string.Join(", ", allLayers)}");
                Debug.Log($"  - {allLayersMessage}");
                Debug.Log($"  - Layer总数: {allLayers.Length}");
                Debug.Log($"  - 所有Layers: {string.Join(", ", allLayers)}");
                
                // 验证所有步骤
                bool allStepsSuccess = result.step1_layer_exists_check &&
                                     result.step2_layer_added &&
                                     result.step3_test_object_created &&
                                     result.step4_layer_set_to_default &&
                                     result.step5_layer_query_verified_default &&
                                     result.step6_layer_set_to_custom &&
                                     result.step7_layer_query_verified_custom &&
                                     result.step8_all_layers_queried;
                
                result.final_result = allStepsSuccess ? "SUCCESS" : "FAILED";
                
                if (allStepsSuccess)
                {
                    result.messages.Add("✓ 所有测试步骤成功");
                    Debug.Log("✓ 所有测试步骤成功");
                }
                else
                {
                    result.messages.Add("✗ 部分测试步骤失败");
                    Debug.Log("✗ 部分测试步骤失败");
                }
                
                Debug.Log("=== 测试完成 ===");
                result.messages.Add("=== 测试完成 ===");
                
                // 清理测试对象
                if (testObject != null)
                {
                    GameObject.DestroyImmediate(testObject);
                    Debug.Log("已清理测试对象");
                }
                
                // 返回JSON格式的结果
                string jsonResult = JsonUtility.ToJson(result, true);
                return jsonResult;
            }
            catch (Exception e)
            {
                Debug.LogError($"测试执行失败: {e.Message}");
                Debug.LogError(e.StackTrace);
                
                result.final_result = "FAILED";
                result.messages.Add($"测试执行失败: {e.Message}");
                result.messages.Add(e.StackTrace);
                
                // 清理测试对象
                if (testObject != null)
                {
                    GameObject.DestroyImmediate(testObject);
                }
                
                string jsonResult = JsonUtility.ToJson(result, true);
                return jsonResult;
            }
        }
    }
}