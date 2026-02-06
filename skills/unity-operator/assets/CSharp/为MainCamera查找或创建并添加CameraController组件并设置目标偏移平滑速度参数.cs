using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace EditorAutomation
{
    public class CameraControllerSetup_20250104
    {
        public static string execute()
        {
            try
            {
                // 查找或创建主摄像机
                GameObject cameraObj = FindOrCreateMainCamera();
                
                // 检查CameraController类型是否存在
                Type cameraControllerType = GetCameraControllerType();
                if (cameraControllerType == null)
                {
                    return "failed: 无法找到CameraController类型";
                }
                
                // 检查是否已存在CameraController组件
                Component existingController = cameraObj.GetComponent(cameraControllerType);
                if (existingController != null)
                {
                    return "success";
                }
                
                // 添加CameraController组件并设置参数
                GameObject target = null;
                Vector3 offset = new Vector3(0f, 5f, -10f);
                float smoothSpeed = 0.125f;
                bool lookAtTarget = false;
                
                AddCameraController(cameraObj, target, offset, smoothSpeed, lookAtTarget);
                
                return "success";
            }
            catch (Exception e)
            {
                return $"failed: {e.Message}";
            }
        }

        private static GameObject FindOrCreateMainCamera()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                GameObject cameraObj = new GameObject("Main Camera");
                camera = cameraObj.AddComponent<Camera>();
                cameraObj.tag = "MainCamera";
                return cameraObj;
            }
            else
            {
                return camera.gameObject;
            }
        }

        private static Type GetCameraControllerType()
        {
            Type type = Type.GetType("Gameplay.CameraController, Assembly-CSharp");
            if (type != null) return type;
            
            type = Type.GetType("CameraController, Assembly-CSharp");
            if (type != null) return type;
            
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType("Gameplay.CameraController");
                if (type != null) return type;
                
                type = assembly.GetType("CameraController");
                if (type != null) return type;
            }
            
            return null;
        }

        private static void AddCameraController(GameObject cameraObj, GameObject target, Vector3 offset, float smoothSpeed, bool lookAtTarget)
        {
            Type cameraControllerType = GetCameraControllerType();
            if (cameraControllerType == null)
            {
                throw new Exception("无法找到CameraController类型");
            }

            Component controller = cameraObj.AddComponent(cameraControllerType);
            
            SetProperty(controller, "Target", target);
            SetProperty(controller, "Offset", offset);
            SetProperty(controller, "SmoothSpeed", smoothSpeed);
            SetProperty(controller, "LookAtTarget", lookAtTarget);
        }

        private static void SetProperty(Component component, string propertyName, object value)
        {
            PropertyInfo prop = component.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(component, value);
            }
        }
    }
}