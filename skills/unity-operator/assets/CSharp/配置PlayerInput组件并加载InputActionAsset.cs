using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace EditorAutomation
{
    public class ConfigurePlayerInput_20250104_0002
    {
        public static string execute()
        {
            bool success = false;
            string gameobject_name = "InputSystem_Test_Player";
            bool gameobject_created = false;
            bool playerinput_added = false;
            bool asset_assigned = false;
            string default_action_map = "";
            string current_action_map = "";
            var action_maps = new List<string>();
            var actions = new List<string>();
            string error = "";

            try
            {
                // 1. 删除已存在的对象
                var existingObj = GameObject.Find("InputSystem_Test_Player");
                if (existingObj != null)
                {
                    GameObject.DestroyImmediate(existingObj);
                    Debug.Log("已删除已存在的测试对象");
                }

                // 2. 创建测试对象
                var testObj = new GameObject("InputSystem_Test_Player");
                gameobject_created = true;
                Debug.Log("创建测试对象: InputSystem_Test_Player");

                // 3. 添加PlayerInput组件（使用类型字符串）
                var playerInputType = System.Type.GetType("UnityEngine.InputSystem.PlayerInput, Unity.InputSystem");
                if (playerInputType != null)
                {
                    var playerInput = testObj.AddComponent(playerInputType);
                    playerinput_added = true;
                    Debug.Log("PlayerInput组件添加成功");

                    // 4. 加载Input Action Asset
                    var assetPath = "Assets/Tests/AutoPy/InputActionAssets/TestInputActions.inputactions";
                    var inputAsset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));

                    if (inputAsset != null)
                    {
                        // 设置actions属性
                        var actionsProp = playerInputType.GetProperty("actions");
                        if (actionsProp != null)
                        {
                            actionsProp.SetValue(playerInput, inputAsset, null);
                            asset_assigned = true;
                            Debug.Log("Input Action Asset已赋值");
                        }

                        // 5. 设置Default Action Map
                        var defaultMapProp = playerInputType.GetProperty("defaultActionMap");
                        if (defaultMapProp != null)
                        {
                            defaultMapProp.SetValue(playerInput, "Gameplay", null);
                            default_action_map = "Gameplay";
                            Debug.Log("设置Default Action Map: Gameplay");
                        }

                        // 6. 设置Notification Behavior为SendMessages
                        var behaviorProp = playerInputType.GetProperty("notificationBehavior");
                        if (behaviorProp != null)
                        {
                            var behaviorType = System.Type.GetType("UnityEngine.InputSystem.PlayerNotifications, Unity.InputSystem");
                            if (behaviorType != null)
                            {
                                var sendMessages = Enum.Parse(behaviorType, "SendMessages");
                                behaviorProp.SetValue(playerInput, sendMessages, null);
                                Debug.Log("设置Notification Behavior: SendMessages");
                            }
                        }

                        // 7. 验证配置
                        var currentMapProp = playerInputType.GetProperty("currentActionMap");
                        if (currentMapProp != null)
                        {
                            var currentMap = currentMapProp.GetValue(playerInput, null);
                            if (currentMap != null)
                            {
                                var nameProp = currentMap.GetType().GetProperty("name");
                                if (nameProp != null)
                                {
                                    current_action_map = nameProp.GetValue(currentMap, null) as string;
                                    Debug.Log("当前Action Map: " + current_action_map);
                                }
                            }
                        }

                        // 8. 枚举Action Maps和Actions
                        var actionMapsProp = inputAsset.GetType().GetProperty("actionMaps");
                        if (actionMapsProp != null)
                        {
                            var actionMaps = actionMapsProp.GetValue(inputAsset, null);
                            if (actionMaps != null)
                            {
                                var enumerator = actionMaps.GetType().GetMethod("GetEnumerator").Invoke(actionMaps, null);
                                var moveNext = enumerator.GetType().GetMethod("MoveNext");
                                var currentProp = enumerator.GetType().GetProperty("Current");

                                while ((bool)moveNext.Invoke(enumerator, null))
                                {
                                    var actionMap = currentProp.GetValue(enumerator, null);
                                    var mapNameProp = actionMap.GetType().GetProperty("name");
                                    if (mapNameProp != null)
                                    {
                                        var mapName = mapNameProp.GetValue(actionMap, null) as string;
                                        action_maps.Add(mapName);
                                        Debug.Log("Action Map: " + mapName);

                                        var mapActionsProp = actionMap.GetType().GetProperty("actions");
                                        if (mapActionsProp != null)
                                        {
                                            var mapActions = mapActionsProp.GetValue(actionMap, null);
                                            if (mapActions != null)
                                            {
                                                var actionEnumerator = mapActions.GetType().GetMethod("GetEnumerator").Invoke(mapActions, null);
                                                var actionMoveNext = actionEnumerator.GetType().GetMethod("MoveNext");
                                                var actionCurrentProp = actionEnumerator.GetType().GetProperty("Current");

                                                while ((bool)actionMoveNext.Invoke(actionEnumerator, null))
                                                {
                                                    var action = actionCurrentProp.GetValue(actionEnumerator, null);
                                                    var actionNameProp = action.GetType().GetProperty("name");
                                                    if (actionNameProp != null)
                                                    {
                                                        var actionName = actionNameProp.GetValue(action, null) as string;
                                                        actions.Add(actionName);
                                                        Debug.Log("  Action: " + actionName);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // 9. 激活输入
                        var activateMethod = playerInputType.GetMethod("ActivateInput");
                        if (activateMethod != null)
                        {
                            activateMethod.Invoke(playerInput, null);
                            Debug.Log("已激活输入");
                        }

                        success = true;
                    }
                    else
                    {
                        error = "无法加载Input Action Asset";
                        Debug.LogError(error);
                    }
                }
                else
                {
                    error = "无法获取PlayerInput类型";
                    Debug.LogError(error);
                }

                // 10. 写入测试结果
                var reportPath = Path.Combine(Application.dataPath, "Tests/AutoPy/AutoCSTestResult.json");
                var directory = Path.GetDirectoryName(reportPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var sb = new StringBuilder();
                sb.Append("{");
                sb.AppendFormat("\"success\": {0}, ", success.ToString().ToLower());
                sb.AppendFormat("\"gameobject_name\": \"{0}\", ", gameobject_name);
                sb.AppendFormat("\"gameobject_created\": {0}, ", gameobject_created.ToString().ToLower());
                sb.AppendFormat("\"playerinput_added\": {0}, ", playerinput_added.ToString().ToLower());
                sb.AppendFormat("\"asset_assigned\": {0}, ", asset_assigned.ToString().ToLower());
                sb.AppendFormat("\"default_action_map\": \"{0}\", ", default_action_map);
                sb.AppendFormat("\"current_action_map\": \"{0}\", ", current_action_map);
                
                sb.Append("\"action_maps\": [");
                for (int i = 0; i < action_maps.Count; i++)
                {
                    if (i > 0) sb.Append(", ");
                    sb.AppendFormat("\"{0}\"", action_maps[i]);
                }
                sb.Append("], ");
                
                sb.Append("\"actions\": [");
                for (int i = 0; i < actions.Count; i++)
                {
                    if (i > 0) sb.Append(", ");
                    sb.AppendFormat("\"{0}\"", actions[i]);
                }
                sb.Append("], ");
                
                sb.AppendFormat("\"error\": \"{0}\"", error);
                sb.Append("}");

                File.WriteAllText(reportPath, sb.ToString());
                Debug.Log("测试结果已写入: " + reportPath);

                return "操作成功: " + sb.ToString();
            }
            catch (Exception e)
            {
                return "操作失败: " + e.Message + "\nStackTrace: " + e.StackTrace;
            }
        }
    }
}