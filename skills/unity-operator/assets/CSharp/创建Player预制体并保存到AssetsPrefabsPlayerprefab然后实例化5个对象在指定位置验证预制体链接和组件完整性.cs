using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace EditorAutomation
{
    public class CreatePlayerPrefabAndInstantiate_20250104
    {
        public static string execute()
        {
            try
            {
                // 步骤1: 创建预制体原型
                Debug.Log("步骤1: 创建预制体原型...");
                
                // 创建空对象
                GameObject prototype = new GameObject("Player_Prototype");
                
                // 添加Cube作为身体
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
                body.name = "Body";
                body.transform.SetParent(prototype.transform);
                body.transform.localPosition = Vector3.zero;
                body.transform.localScale = new Vector3(1f, 1.5f, 0.5f);
                
                // 添加Capsule作为头部
                GameObject head = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                head.name = "Head";
                head.transform.SetParent(prototype.transform);
                head.transform.localPosition = new Vector3(0f, 1.2f, 0f);
                head.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                
                // 添加Rigidbody
                Rigidbody rigidbody = prototype.AddComponent<Rigidbody>();
                rigidbody.mass = 70f;
                rigidbody.drag = 0.5f;
                
                // 添加Capsule Collider
                CapsuleCollider capsuleCollider = prototype.AddComponent<CapsuleCollider>();
                capsuleCollider.height = 2.5f;
                capsuleCollider.radius = 0.5f;
                capsuleCollider.center = new Vector3(0f, 0.5f, 0f);
                
                Debug.Log("预制体原型创建成功！");
                
                // 步骤2: 保存预制体
                Debug.Log("步骤2: 保存预制体...");
                string prefabPath = "Assets/Prefabs/Player.prefab";
                
                // 确保目录存在
                string prefabDir = Path.GetDirectoryName(prefabPath);
                if (!Directory.Exists(prefabDir))
                {
                    Directory.CreateDirectory(prefabDir);
                    Debug.Log($"创建目录: {prefabDir}");
                }
                
                // 保存预制体
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(prototype, prefabPath);
                
                if (prefab == null)
                {
                    Debug.LogError("预制体保存失败！");
                    GameObject.DestroyImmediate(prototype);
                    return "failed: 预制体保存失败";
                }
                
                Debug.Log($"预制体保存成功: {prefabPath}");
                
                // 删除原型对象
                GameObject.DestroyImmediate(prototype);
                Debug.Log("原型对象已删除");
                
                // 步骤3: 从预制体实例化5个对象
                Debug.Log("步骤3: 实例化5个对象...");
                
                // 实例化位置
                Vector3[] positions = new Vector3[]
                {
                    new Vector3(0f, 0f, 0f),
                    new Vector3(5f, 0f, 0f),
                    new Vector3(-5f, 0f, 0f),
                    new Vector3(0f, 0f, 5f),
                    new Vector3(0f, 0f, -5f)
                };
                
                int instantiatedCount = 0;
                
                for (int i = 0; i < positions.Length; i++)
                {
                    // 从预制体实例化
                    GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                    if (instance != null)
                    {
                        instance.name = $"Player_Instance_{i + 1}";
                        instance.transform.position = positions[i];
                        instantiatedCount++;
                        Debug.Log($"实例化对象 {i + 1}: {instance.name} 在位置 {positions[i]}");
                    }
                    else
                    {
                        Debug.LogError($"实例化失败: 位置 {positions[i]}");
                    }
                }
                
                // 检查是否全部实例化成功
                if (instantiatedCount != 5)
                {
                    Debug.LogError($"实例化对象数量不正确: {instantiatedCount}/5");
                    return $"failed: 实例化失败: {instantiatedCount}/5";
                }
                
                Debug.Log("5个对象实例化成功！");
                
                // 步骤4: 验证实例化对象
                Debug.Log("步骤4: 验证实例化对象...");
                
                bool allLinksMaintained = true;
                bool allComponentsIntact = true;
                
                // 重新查找所有实例化对象进行验证
                GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
                foreach (GameObject obj in allObjects)
                {
                    if (obj.name.StartsWith("Player_Instance_"))
                    {
                        // 验证预制体链接
                        if (!PrefabUtility.IsPartOfPrefabInstance(obj))
                        {
                            allLinksMaintained = false;
                            Debug.LogWarning($"预制体链接问题: {obj.name}");
                        }
                        
                        // 验证组件完整性
                        if (obj.GetComponent<Rigidbody>() == null || 
                            obj.GetComponent<CapsuleCollider>() == null ||
                            obj.transform.Find("Body") == null ||
                            obj.transform.Find("Head") == null)
                        {
                            allComponentsIntact = false;
                            Debug.LogWarning($"组件缺失: {obj.name}");
                        }
                    }
                }
                
                // 返回结果
                if (allLinksMaintained && allComponentsIntact)
                {
                    Debug.Log("所有操作成功完成！预制体创建、保存、实例化和验证全部通过。");
                    return "success";
                }
                else
                {
                    Debug.LogWarning("验证发现部分问题");
                    return "failed: 验证发现问题";
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"执行错误: {e.Message}");
                return $"failed: {e.Message}";
            }
        }
    }
}
