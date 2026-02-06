using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Collections.Generic;

namespace EditorAutomation
{
    public class CollisionMatrixTest_20250104_143022
    {
        private const string LAYER_A = "CollisionLayerA";
        private const string LAYER_B = "CollisionLayerB";
        private const string LAYER_C = "CollisionLayerC";

        private static bool CheckLayerExists(string layerName)
        {
            try
            {
                string[] allLayers = InternalEditorUtility.layers;
                return Array.Exists(allLayers, layer => layer == layerName);
            }
            catch (Exception e)
            {
                Debug.LogError($"检查Layer存在性失败: {e.Message}");
                return false;
            }
        }

        private static (bool success, string message) AddLayer(string layerName)
        {
            try
            {
                if (CheckLayerExists(layerName))
                {
                    return (true, $"Layer '{layerName}' 已存在");
                }

                string tagManagerPath = "ProjectSettings/TagManager.asset";
                UnityEngine.Object[] tagManagerAssets = AssetDatabase.LoadAllAssetsAtPath(tagManagerPath);

                if (tagManagerAssets == null || tagManagerAssets.Length == 0)
                {
                    return (false, "无法加载TagManager.asset");
                }

                SerializedObject serializedObject = new SerializedObject(tagManagerAssets[0]);
                SerializedProperty layersProp = serializedObject.FindProperty("layers");

                if (layersProp == null || !layersProp.isArray)
                {
                    return (false, "无法找到layers属性");
                }

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

                return (false, "添加Layer失败：所有User Layer槽位（8-31）已满");
            }
            catch (Exception e)
            {
                return (false, $"添加Layer失败: {e.Message}");
            }
        }

        private static (int layerId, string message) GetLayerId(string layerName)
        {
            try
            {
                for (int i = 0; i < 32; i++)
                {
                    string name = LayerMask.LayerToName(i);
                    if (name == layerName)
                    {
                        return (i, $"找到Layer '{layerName}' (ID: {i})");
                    }
                }
                return (-1, $"Layer '{layerName}' 不存在");
            }
            catch (Exception e)
            {
                return (-1, $"获取Layer ID失败: {e.Message}");
            }
        }

        private static (List<List<bool>> matrix, string message) GetCollisionMatrix()
        {
            try
            {
                List<List<bool>> matrix = new List<List<bool>>();
                for (int i = 0; i < 32; i++)
                {
                    List<bool> row = new List<bool>();
                    for (int j = 0; j < 32; j++)
                    {
                        bool ignore = Physics.GetIgnoreLayerCollision(i, j);
                        row.Add(!ignore);
                    }
                    matrix.Add(row);
                }
                return (matrix, "获取碰撞矩阵成功");
            }
            catch (Exception e)
            {
                return (new List<List<bool>>(), $"获取碰撞矩阵失败: {e.Message}");
            }
        }

        private static (bool success, string message) SetLayerCollision(int layer1Id, int layer2Id, bool shouldCollide)
        {
            try
            {
                Physics.IgnoreLayerCollision(layer1Id, layer2Id, !shouldCollide);
                return (true, $"设置Layer碰撞成功: Layer{layer1Id}与Layer{layer2Id} {(shouldCollide ? "可碰撞" : "忽略")}");
            }
            catch (Exception e)
            {
                return (false, $"设置Layer碰撞失败: {e.Message}");
            }
        }

        private static (GameObject obj, string message) CreateTestObject(string objectName, string layerName)
        {
            try
            {
                GameObject testObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                testObj.name = objectName;

                (int layerId, string idMessage) = GetLayerId(layerName);
                if (layerId < 0)
                {
                    UnityEngine.Object.DestroyImmediate(testObj);
                    return (null, idMessage);
                }

                testObj.layer = layerId;

                Rigidbody rigidbody = testObj.AddComponent<Rigidbody>();
                rigidbody.useGravity = false;
                rigidbody.isKinematic = true;

                return (testObj, $"创建测试对象成功: {objectName} (Layer: {layerName})");
            }
            catch (Exception e)
            {
                return (null, $"创建测试对象失败: {e.Message}");
            }
        }

        private static (Dictionary<string, object> result, string message) VerifyCollisionSettings(
            List<List<bool>> matrix, int layerAId, int layerBId, int layerCId)
        {
            try
            {
                bool aBcollide = (layerAId < 32 && layerBId < 32) ? matrix[layerAId][layerBId] : false;
                bool aBcorrect = !aBcollide;

                bool aCcollide = (layerAId < 32 && layerCId < 32) ? matrix[layerAId][layerCId] : false;
                bool aCcorrect = !aCcollide;

                bool bCcollide = (layerBId < 32 && layerCId < 32) ? matrix[layerBId][layerCId] : false;
                bool bCcorrect = bCcollide;

                Dictionary<string, object> verificationResult = new Dictionary<string, object>
                {
                    { "a_b_ignore_correct", aBcorrect },
                    { "a_c_ignore_correct", aCcorrect },
                    { "b_c_collide_correct", bCcorrect },
                    { "a_b_actual_relation", aBcollide ? "Collide" : "Ignore" },
                    { "a_c_actual_relation", aCcollide ? "Collide" : "Ignore" },
                    { "b_c_actual_relation", bCcollide ? "Collide" : "Ignore" }
                };

                return (verificationResult, "验证完成");
            }
            catch (Exception e)
            {
                return (new Dictionary<string, object>(), $"验证碰撞设置失败: {e.Message}");
            }
        }

        public static string execute()
        {
            try
            {
                Debug.Log("=== 开始碰撞矩阵测试 ===");

                List<string> testLayers = new List<string> { LAYER_A, LAYER_B, LAYER_C };
                Dictionary<string, int> layerIds = new Dictionary<string, int>();
                Dictionary<string, GameObject> testObjects = new Dictionary<string, GameObject>();
                List<List<bool>> matrixAfter = new List<List<bool>>();
                Dictionary<string, object> verification = new Dictionary<string, object>();

                bool allSuccess = true;

                for (int step = 1; step <= 7; step++)
                {
                    switch (step)
                    {
                        case 1:
                            Debug.Log("步骤1: 检查并确保测试Layer存在");
                            foreach (string layerName in testLayers)
                            {
                                (bool success, string message) = AddLayer(layerName);
                                Debug.Log($"  - {message}");
                                if (!success) allSuccess = false;
                            }
                            break;

                        case 2:
                            Debug.Log("步骤2: 获取Layer ID");
                            foreach (string layerName in testLayers)
                            {
                                (int layerId, string message) = GetLayerId(layerName);
                                Debug.Log($"  - {message}");
                                if (layerId >= 0)
                                {
                                    layerIds[layerName] = layerId;
                                }
                                else
                                {
                                    allSuccess = false;
                                }
                            }
                            if (layerIds.Count < 3) allSuccess = false;
                            break;

                        case 3:
                            Debug.Log("步骤3: 查询当前碰撞矩阵");
                            (List<List<bool>> matrixBefore, string msgBefore) = GetCollisionMatrix();
                            Debug.Log($"  - {msgBefore} (矩阵大小: {matrixBefore.Count}x{(matrixBefore.Count > 0 ? matrixBefore[0].Count : 0)})");
                            if (matrixBefore.Count != 32) allSuccess = false;
                            break;

                        case 4:
                            Debug.Log("步骤4: 设置Layer碰撞关系");
                            if (layerIds.Count == 3)
                            {
                                int aId = layerIds[LAYER_A];
                                int bId = layerIds[LAYER_B];
                                int cId = layerIds[LAYER_C];

                                (bool successAB, string msgAB) = SetLayerCollision(aId, bId, false);
                                Debug.Log($"  - {msgAB}");
                                if (!successAB) allSuccess = false;

                                (bool successAC, string msgAC) = SetLayerCollision(aId, cId, false);
                                Debug.Log($"  - {msgAC}");
                                if (!successAC) allSuccess = false;

                                (bool successBC, string msgBC) = SetLayerCollision(bId, cId, true);
                                Debug.Log($"  - {msgBC}");
                                if (!successBC) allSuccess = false;
                            }
                            else
                            {
                                Debug.LogError("  - Layer ID不完整，跳过碰撞设置");
                                allSuccess = false;
                            }
                            break;

                        case 5:
                            Debug.Log("步骤5: 创建测试对象");
                            foreach (string layerName in testLayers)
                            {
                                string objName = $"TestObject_{layerName}";
                                (GameObject obj, string message) = CreateTestObject(objName, layerName);
                                Debug.Log($"  - {message}");
                                if (obj != null)
                                {
                                    testObjects[layerName] = obj;
                                }
                                else
                                {
                                    allSuccess = false;
                                }
                            }
                            break;

                        case 6:
                            Debug.Log("步骤6: 查询设置后的碰撞矩阵");
                            var (matrixAfterResult, msgAfter) = GetCollisionMatrix();
                            matrixAfter = matrixAfterResult;
                            Debug.Log($"  - {msgAfter}");
                            if (matrixAfter.Count != 32) allSuccess = false;
                            break;

                        case 7:
                            Debug.Log("步骤7: 验证碰撞矩阵设置");
                            if (matrixAfter.Count == 32 && layerIds.Count == 3)
                            {
                                var (verificationResult, verifyMsg) = VerifyCollisionSettings(
                                    matrixAfter,
                                    layerIds[LAYER_A],
                                    layerIds[LAYER_B],
                                    layerIds[LAYER_C]
                                );
                                verification = verificationResult;
                                Debug.Log($"  - {verifyMsg}");
                                Debug.Log($"  - A-B关系: {verification["a_b_actual_relation"]}");
                                Debug.Log($"  - A-C关系: {verification["a_c_actual_relation"]}");
                                Debug.Log($"  - B-C关系: {verification["b_c_actual_relation"]}");

                                bool verifySuccess = (bool)verification["a_b_ignore_correct"] &&
                                                   (bool)verification["a_c_ignore_correct"] &&
                                                   (bool)verification["b_c_collide_correct"];
                                if (!verifySuccess) allSuccess = false;
                            }
                            else
                            {
                                Debug.LogError("  - 数据不完整，跳过验证");
                                allSuccess = false;
                            }
                            break;
                    }
                }

                Debug.Log(allSuccess ? "✓ 所有测试步骤成功" : "✗ 部分测试步骤失败");
                Debug.Log("=== 测试完成 ===");

                return allSuccess ? "success" : "failed: 部分测试步骤失败";
            }
            catch (Exception e)
            {
                Debug.LogError($"测试执行失败: {e.Message}");
                Debug.LogError(e.StackTrace);
                return $"failed: {e.Message}";
            }
        }
    }
}