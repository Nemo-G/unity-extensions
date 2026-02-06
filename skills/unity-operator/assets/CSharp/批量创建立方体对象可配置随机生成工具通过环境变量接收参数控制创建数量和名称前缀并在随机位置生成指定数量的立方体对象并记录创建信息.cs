using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace EditorAutomation
{
    public class CreateCubes_20250104_143022
    {
        public static string execute()
        {
            try
            {
                // 默认参数
                int count = 5;
                string prefix = "BatchCube_";
                
                // 从环境变量获取参数（如果存在）
                string argsJson = Environment.GetEnvironmentVariable("UNITY_CSHARP_ARGS");
                if (!string.IsNullOrEmpty(argsJson))
                {
                    try
                    {
                        var args = JsonUtility.FromJson<CubeCreationArgs>(argsJson);
                        if (args.count > 0) count = args.count;
                        if (!string.IsNullOrEmpty(args.prefix)) prefix = args.prefix;
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"解析参数失败，使用默认值: {e.Message}");
                    }
                }

                // 验证参数
                if (count <= 0)
                {
                    return "failed: count参数必须是正整数";
                }

                if (string.IsNullOrEmpty(prefix))
                {
                    return "failed: prefix参数不能为null或空";
                }

                // 批量创建立方体
                for (int i = 0; i < count; i++)
                {
                    // 生成随机位置 (-5 到 5)
                    float x = UnityEngine.Random.Range(-5f, 5f);
                    float y = UnityEngine.Random.Range(-5f, 5f);
                    float z = UnityEngine.Random.Range(-5f, 5f);
                    
                    // 创建立方体
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.name = $"{prefix}{i}";
                    cube.transform.position = new Vector3(x, y, z);
                }

                return count > 0 ? "success" : "failed: 未创建任何对象";
            }
            catch (Exception e)
            {
                return $"failed: {e.Message}";
            }
        }

        [Serializable]
        private class CubeCreationArgs
        {
            public int count = 5;
            public string prefix = "BatchCube_";
        }
    }
}