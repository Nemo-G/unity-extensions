using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace EditorAutomation
{
    public class Create3Materials_20250104
    {
        public static string execute()
        {
            try
            {
                var materialsInfo = new[]
                {
                    new { name = "RedMaterial", color = new Color(1, 0, 0, 1), path = "Assets/Materials/RedMaterial.mat" },
                    new { name = "BlueMaterial", color = new Color(0, 0, 1, 1), path = "Assets/Materials/BlueMaterial.mat" },
                    new { name = "GreenMaterial", color = new Color(0, 1, 0, 1), path = "Assets/Materials/GreenMaterial.mat" }
                };

                string folderPath = "Assets/Materials";
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                int createdCount = 0;

                foreach (var matInfo in materialsInfo)
                {
                    Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
                    if (shader == null)
                    {
                        return "failed: URP/Unlit shader not found";
                    }

                    Material material = new Material(shader);
                    material.color = matInfo.color;
                    AssetDatabase.CreateAsset(material, matInfo.path);
                    createdCount++;
                }

                AssetDatabase.Refresh();
                
                return createdCount == 3 ? "success" : $"failed: Only created {createdCount}/3 materials";
            }
            catch (Exception e)
            {
                return $"failed: {e.Message}";
            }
        }
    }
}
