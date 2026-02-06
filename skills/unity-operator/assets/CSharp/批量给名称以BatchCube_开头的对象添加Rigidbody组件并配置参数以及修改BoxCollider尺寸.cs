using UnityEngine;
using UnityEditor;
using System;

namespace EditorAutomation
{
    public class AddRigidbodyAndBoxColliderToBatchCube_20260104_133150
    {
        public static string execute()
        {
            int objectsProcessed = 0;
            int rigidbodyAdded = 0;
            int rigidbodyConfigurationSuccess = 0;
            int boxcolliderModified = 0;
            int boxcolliderAdded = 0;
            
            try
            {
                // 查找所有GameObject
                GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
                Debug.Log($"找到 {allObjects.Length} 个GameObject");
                
                // 过滤名称以"BatchCube_"开头的对象
                System.Collections.Generic.List<GameObject> batchCubes = new System.Collections.Generic.List<GameObject>();
                foreach (GameObject obj in allObjects)
                {
                    if (obj.name.StartsWith("BatchCube_"))
                    {
                        batchCubes.Add(obj);
                    }
                }
                
                Debug.Log($"找到 {batchCubes.Count} 个名称以'BatchCube_'开头的对象");
                
                // 处理每个对象
                foreach (GameObject obj in batchCubes)
                {
                    string objName = obj.name;
                    Debug.Log($"处理对象: {objName}");
                    
                    // 检查并添加Rigidbody组件
                    Rigidbody[] rigidbodies = obj.GetComponents<Rigidbody>();
                    Rigidbody rb = null;
                    
                    if (rigidbodies.Length == 0)
                    {
                        Debug.Log($"  - 添加Rigidbody组件...");
                        rb = obj.AddComponent<Rigidbody>();
                        rigidbodyAdded++;
                    }
                    else
                    {
                        Debug.Log($"  - 已存在Rigidbody组件，使用现有组件");
                        rb = rigidbodies[0];
                    }
                    
                    // 配置Rigidbody参数
                    try
                    {
                        rb.mass = 2.0f;
                        rb.useGravity = true;
                        rb.drag = 0.5f; // 设置阻力
                        rb.angularDrag = 0.5f; // 设置角阻力
                        rigidbodyConfigurationSuccess++;
                        Debug.Log($"  - Rigidbody配置成功: mass=2.0, useGravity=True, drag=0.5, angularDrag=0.5");
                    }
                    catch (Exception e)
                    {
                        Debug.Log($"  - Rigidbody配置失败: {e.Message}");
                    }
                    
                    // 检查并配置BoxCollider
                    BoxCollider[] boxColliders = obj.GetComponents<BoxCollider>();
                    if (boxColliders.Length > 0)
                    {
                        // 修改现有BoxCollider
                        Debug.Log($"  - 修改现有BoxCollider尺寸...");
                        BoxCollider bc = boxColliders[0];
                        Vector3 originalSize = bc.size;
                        Vector3 newSize = new Vector3(originalSize.x * 1.2f, originalSize.y * 1.2f, originalSize.z * 1.2f);
                        bc.size = newSize;
                        boxcolliderModified++;
                        Debug.Log($"  - BoxCollider尺寸从 ({originalSize.x:F2}, {originalSize.y:F2}, {originalSize.z:F2}) 修改为 ({newSize.x:F2}, {newSize.y:F2}, {newSize.z:F2})");
                    }
                    else
                    {
                        // 添加新的BoxCollider并配置
                        Debug.Log($"  - 添加BoxCollider组件...");
                        BoxCollider bc = obj.AddComponent<BoxCollider>();
                        // 默认size是(1,1,1)，放大1.2倍
                        Vector3 newSize = new Vector3(1.2f, 1.2f, 1.2f);
                        bc.size = newSize;
                        boxcolliderAdded++;
                        Debug.Log($"  - BoxCollider添加并配置: size=({newSize.x:F2}, {newSize.y:F2}, {newSize.z:F2})");
                    }
                    
                    objectsProcessed++;
                }
                
                string resultMessage;
                if (batchCubes.Count > 0)
                {
                    resultMessage = $"操作成功: 处理 {objectsProcessed} 个对象。Rigidbody添加: {rigidbodyAdded}, 配置成功: {rigidbodyConfigurationSuccess}, BoxCollider修改: {boxcolliderModified}, BoxCollider添加: {boxcolliderAdded}";
                    Debug.Log(resultMessage);
                    return resultMessage;
                }
                else
                {
                    resultMessage = "操作完成: 未找到任何名称以'BatchCube_'开头的对象";
                    Debug.Log(resultMessage);
                    return resultMessage;
                }
            }
            catch (Exception e)
            {
                string errorMessage = $"操作失败: {e.Message}";
                Debug.LogError(errorMessage);
                Debug.LogException(e);
                return errorMessage;
            }
        }
    }
}
