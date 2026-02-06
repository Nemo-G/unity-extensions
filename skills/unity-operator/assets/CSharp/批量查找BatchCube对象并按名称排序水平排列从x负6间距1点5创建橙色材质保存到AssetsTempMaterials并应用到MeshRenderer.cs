using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace EditorAutomation
{
    public class BatchCubeArrange_20250104
    {
        public static string execute()
        {
            try
            {
                // 查找所有BatchCube对象
                GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                List<GameObject> batchCubes = allObjects
                    .Where(obj => obj.name.StartsWith("BatchCube_"))
                    .ToList();
                
                int objectsFound = batchCubes.Count;
                
                if (objectsFound == 0)
                {
                    return "failed: 未找到BatchCube对象";
                }

                // 按名称排序
                batchCubes.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
                
                // 排列参数
                float xStart = -6.0f;
                float spacing = 1.5f;
                
                // 确保材质目录存在
                string materialsPath = "Assets/Temp/Materials";
                if (!Directory.Exists(materialsPath))
                {
                    Directory.CreateDirectory(materialsPath);
                }

                // 处理每个对象
                for (int i = 0; i < batchCubes.Count; i++)
                {
                    GameObject obj = batchCubes[i];
                    
                    // 计算新位置
                    float newX = xStart + i * spacing;
                    obj.transform.position = new Vector3(newX, 0.0f, 0.0f);
                    
                    // 创建橙色材质
                    string materialName = $"OrangeMaterial_{obj.name}";
                    Material orangeMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    orangeMaterial.name = materialName;
                    orangeMaterial.color = new Color(1.0f, 0.5f, 0.0f, 1.0f);
                    
                    // 保存材质
                    string materialPath = $"{materialsPath}/{materialName}.mat";
                    AssetDatabase.CreateAsset(orangeMaterial, materialPath);
                    
                    // 获取或添加MeshRenderer并应用材质
                    MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
                    if (renderer == null)
                    {
                        renderer = obj.AddComponent<MeshRenderer>();
                    }
                    renderer.material = orangeMaterial;
                }

                AssetDatabase.Refresh();
                return "success";
            }
            catch (Exception e)
            {
                return $"failed: {e.Message}";
            }
        }
    }
}