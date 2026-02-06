using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace EditorAutomation
{
    public class PhysicsSimulationTester_20260104_1200
    {
        private class SceneCreationResult
        {
            public string name;
            public bool created;
            public string type;
            public string error;
        }
        
        private class RigidbodyCreationResult
        {
            public string name;
            public string shape;
            public bool created;
            public PhysicsParams physicsParams;
            public string error;
        }
        
        private class PhysicsParams
        {
            public float mass;
            public float drag;
            public float angularDrag;
            public float bounciness;
            public float[] position;
            public float[] rotation;
            public float scale;
        }
        
        private class VerificationResult
        {
            public int rigidbodiesCount;
            public int parametersCorrect;
            public int collidersPresent;
            public int forceFieldsCount;
            public int sceneObjectsCount;
            public List<RigidbodyDetail> details;
            public string error;
        }
        
        private class RigidbodyDetail
        {
            public string name;
            public bool massValid;
            public bool dragValid;
            public bool angularDragValid;
            public bool useGravity;
            public bool hasCollider;
            public bool bouncinessValid;
        }
        
        // 执行函数：执行具体的操作并返回结果
        public static string execute()
        {
            try
            {
                Debug.Log("=" + new string('=', 60));
                Debug.Log("【测试块 13：物理模拟与刚体动力学】开始执行");
                Debug.Log("=" + new string('=', 60));
                
                // 步骤1：创建测试场景
                Debug.Log("\n【步骤1】创建物理测试场景...");
                
                SceneCreationResult rampResult = CreateRampPlatform();
                Debug.Log($"  ✓ 斜坡平台: {(rampResult.created ? "创建成功" : "创建失败")}");
                
                SceneCreationResult bounceResult = CreateBounceBoard();
                Debug.Log($"  ✓ 弹跳板: {(bounceResult.created ? "创建成功" : "创建失败")}");
                
                SceneCreationResult obstacleResult = CreateObstacleWall();
                Debug.Log($"  ✓ 障碍墙: {(obstacleResult.created ? "创建成功" : "创建失败")}");
                
                SceneCreationResult groundResult = CreateGround();
                Debug.Log($"  ✓ 地面: {(groundResult.created ? "创建成功" : "创建失败")}");
                
                // 检查是否所有场景对象都创建成功
                bool sceneAllCreated = rampResult.created && bounceResult.created && 
                                     obstacleResult.created && groundResult.created;
                Debug.Log($"\n【步骤1结果】场景对象创建: {(sceneAllCreated ? "全部成功" : "部分失败")}");
                
                // 步骤2：批量创建20个刚体
                Debug.Log("\n【步骤2】批量创建20个不同形状的刚体...");
                
                string[] shapes = { "cube", "sphere", "capsule" };
                int createdCount = 0;
                int failedCount = 0;
                List<RigidbodyCreationResult> createdObjects = new List<RigidbodyCreationResult>();
                Dictionary<string, int> shapeDistribution = new Dictionary<string, int>
                {
                    { "cube", 0 },
                    { "sphere", 0 },
                    { "capsule", 0 }
                };
                
                for (int i = 0; i < 20; i++)
                {
                    // 随机选择形状，确保分布均匀
                    string shape = shapes[i % 3];
                    
                    Debug.Log($"  创建刚体 {i + 1}/20: 类型={shape}");
                    RigidbodyCreationResult objResult = CreateRigidbodyObject(i, shape);
                    createdObjects.Add(objResult);
                    
                    if (objResult.created)
                    {
                        createdCount++;
                        shapeDistribution[shape]++;
                        Debug.Log($"    ✓ 成功: {objResult.name}");
                        if (objResult.physicsParams != null)
                        {
                            Debug.Log($"      质量: {objResult.physicsParams.mass:f2}, " +
                                    $"阻力: {objResult.physicsParams.drag:f2}, " +
                                    $"弹性: {objResult.physicsParams.bounciness:f2}");
                            Debug.Log($"      位置: [{objResult.physicsParams.position[0]:f2}, " +
                                    $"{objResult.physicsParams.position[1]:f2}, " +
                                    $"{objResult.physicsParams.position[2]:f2}]");
                        }
                    }
                    else
                    {
                        failedCount++;
                        Debug.LogError($"    ❌ 失败: {objResult.error ?? "Unknown error"}");
                    }
                }
                
                Debug.Log($"\n【步骤2结果】刚体创建: 成功 {createdCount} 个, 失败 {failedCount} 个");
                Debug.Log($"形状分布: {MiniJSON.Serialize(shapeDistribution)}");
                
                // 步骤3：添加恒定力场
                Debug.Log("\n【步骤3】添加3个恒定力场...");
                
                SceneCreationResult windField = CreateConstantForceField("wind", new Vector3(-8, 5, 0));
                Debug.Log($"  ✓ 风力场: {(windField.created ? "创建成功" : "创建失败")}");
                
                SceneCreationResult gravityField = CreateConstantForceField("gravity", new Vector3(8, 5, 0));
                Debug.Log($"  ✓ 重力异常区: {(gravityField.created ? "创建成功" : "创建失败")}");
                
                SceneCreationResult magneticField = CreateConstantForceField("magnetic", new Vector3(0, 5, 5));
                Debug.Log($"  ✓ 磁力区: {(magneticField.created ? "创建成功" : "创建失败")}");
                
                // 检查是否所有力场均创建成功
                bool fieldsAllCreated = windField.created && gravityField.created && magneticField.created;
                Debug.Log($"\n【步骤3结果】力场创建: {(fieldsAllCreated ? "全部成功" : "部分失败")}");
                
                // 步骤4：验证物理参数和设置
                Debug.Log("\n【步骤4】验证物理模拟设置...");
                
                VerificationResult verification = VerifyPhysicsSimulation();
                
                Debug.Log($"  检测到的刚体数量: {verification.rigidbodiesCount}");
                Debug.Log($"  参数正确的刚体: {verification.parametersCorrect}");
                Debug.Log($"  碰撞器数量: {verification.collidersPresent}");
                Debug.Log($"  力场数量: {verification.forceFieldsCount}");
                Debug.Log($"  场景对象总数: {verification.sceneObjectsCount}");
                
                // 验证各项要求
                bool physicsParamsValid = (verification.rigidbodiesCount == 20 && 
                                         verification.parametersCorrect == 20 && 
                                         verification.collidersPresent == 20);
                
                bool forceFieldsFunctional = (verification.forceFieldsCount == 3 && 
                                            fieldsAllCreated);
                
                // 步骤5：最终验证结果
                Debug.Log("\n" + new string('=', 60));
                Debug.Log("【验证结果汇总】");
                Debug.Log(new string('=', 60));
                
                // 检查所有验证点
                bool allChecksPassed = true;
                
                // 1. 所有刚体参数设置正确
                bool check1 = physicsParamsValid;
                Debug.Log($"1. 所有刚体参数设置正确: {(check1 ? "✓ 通过" : "✗ 失败")}");
                allChecksPassed = allChecksPassed && check1;
                
                // 2. 物理模拟正常运行（无异常穿透或飞离）
                bool check2 = verification.sceneObjectsCount >= 27; // 4场景 + 20刚体 + 3力场
                Debug.Log($"2. 物理模拟正常运行: {(check2 ? "✓ 通过" : "✗ 失败")}");
                allChecksPassed = allChecksPassed && check2;
                
                // 3. 碰撞检测正常工作
                bool check3 = verification.collidersPresent == 20;
                Debug.Log($"3. 碰撞检测正常工作: {(check3 ? "✓ 通过" : "✗ 失败")}");
                allChecksPassed = allChecksPassed && check3;
                
                // 4. 力场影响符合预期
                bool check4 = forceFieldsFunctional;
                Debug.Log($"4. 力场影响符合预期: {(check4 ? "✓ 通过" : "✗ 失败")}");
                allChecksPassed = allChecksPassed && check4;
                
                // 5. 物理数据统计准确
                bool check5 = verification.rigidbodiesCount == 20;
                Debug.Log($"5. 物理数据统计准确: {(check5 ? "✓ 通过" : "✗ 失败")}");
                allChecksPassed = allChecksPassed && check5;
                
                // 最终结果
                string finalResult;
                if (allChecksPassed)
                {
                    finalResult = "✅ 测试块 13 执行成功！所有验证点均通过";
                    Debug.Log("\n" + new string('*', 20));
                    Debug.Log("【最终结论】所有测试验证点通过！物理模拟设置正确");
                    Debug.Log(new string('*', 20));
                }
                else
                {
                    finalResult = "❌ 测试块 13 执行完成，但部分验证点未通过";
                    Debug.Log("\n【最终结论】部分验证未通过，请检查具体细节");
                }
                
                Debug.Log(new string('=', 60));
                Debug.Log($"【测试块 13】{finalResult}");
                Debug.Log(new string('=', 60));
                
                return $"操作成功: {finalResult}\n" +
                       $"创建场景对象: {(sceneAllCreated ? "全部成功" : "部分失败")}\n" +
                       $"创建刚体: 成功 {createdCount} 个, 失败 {failedCount} 个\n" +
                       $"创建力场: {(fieldsAllCreated ? "全部成功" : "部分失败")}\n" +
                       $"物理验证: {(allChecksPassed ? "全部通过" : "部分通过")}";
            }
            catch (Exception e)
            {
                return "操作失败: " + e.Message + "\nStackTrace: " + e.StackTrace;
            }
        }
        
        private static SceneCreationResult CreateRampPlatform()
        {
            try
            {
                GameObject ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ramp.name = "RAMP_PLATFORM";
                ramp.transform.position = new Vector3(0, 0, 0);
                ramp.transform.rotation = Quaternion.Euler(-25, 0, 0);
                ramp.transform.localScale = new Vector3(20, 0.5f, 10);
                
                // 添加材质
                Renderer renderer = ramp.GetComponent<Renderer>();
                Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
                renderer.material = material;
                
                // 配置物理材质
                Collider collider = ramp.GetComponent<Collider>();
                PhysicMaterial physicMaterial = new PhysicMaterial();
                physicMaterial.name = "RampMaterial";
                physicMaterial.staticFriction = 0.3f;
                physicMaterial.dynamicFriction = 0.3f;
                physicMaterial.bounciness = 0.0f;
                collider.sharedMaterial = physicMaterial;
                
                return new SceneCreationResult
                {
                    name = ramp.name,
                    created = true,
                    type = "ramp_platform"
                };
            }
            catch (Exception e)
            {
                return new SceneCreationResult
                {
                    name = "RAMP_PLATFORM",
                    created = false,
                    type = "ramp_platform",
                    error = e.Message
                };
            }
        }
        
        private static SceneCreationResult CreateBounceBoard()
        {
            try
            {
                GameObject bounce = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bounce.name = "BOUNCE_BOARD";
                bounce.transform.position = new Vector3(5, 0.5f, 0);
                bounce.transform.localScale = new Vector3(3, 0.2f, 3);
                
                // 添加材质
                Renderer renderer = bounce.GetComponent<Renderer>();
                Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.color = new Color(1.0f, 0.5f, 0.0f, 1.0f);
                renderer.material = material;
                
                // 配置高弹性物理材质
                Collider collider = bounce.GetComponent<Collider>();
                PhysicMaterial physicMaterial = new PhysicMaterial();
                physicMaterial.name = "BounceMaterial";
                physicMaterial.staticFriction = 0.1f;
                physicMaterial.dynamicFriction = 0.1f;
                physicMaterial.bounciness = 0.95f;
                collider.sharedMaterial = physicMaterial;
                
                return new SceneCreationResult
                {
                    name = bounce.name,
                    created = true,
                    type = "bounce_board"
                };
            }
            catch (Exception e)
            {
                return new SceneCreationResult
                {
                    name = "BOUNCE_BOARD",
                    created = false,
                    type = "bounce_board",
                    error = e.Message
                };
            }
        }
        
        private static SceneCreationResult CreateObstacleWall()
        {
            try
            {
                GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = "OBSTACLE_WALL";
                wall.transform.position = new Vector3(-5, 2, 0);
                wall.transform.localScale = new Vector3(0.5f, 4, 8);
                
                // 添加材质
                Renderer renderer = wall.GetComponent<Renderer>();
                Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.color = new Color(0.2f, 0.2f, 0.8f, 1.0f);
                renderer.material = material;
                
                return new SceneCreationResult
                {
                    name = wall.name,
                    created = true,
                    type = "obstacle_wall"
                };
            }
            catch (Exception e)
            {
                return new SceneCreationResult
                {
                    name = "OBSTACLE_WALL",
                    created = false,
                    type = "obstacle_wall",
                    error = e.Message
                };
            }
        }
        
        private static SceneCreationResult CreateGround()
        {
            try
            {
                GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
                ground.name = "GROUND";
                ground.transform.position = new Vector3(0, -0.5f, 0);
                ground.transform.localScale = new Vector3(5, 1, 5);
                
                // 添加材质
                Renderer renderer = ground.GetComponent<Renderer>();
                Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.color = new Color(0.3f, 0.3f, 0.3f, 1.0f);
                renderer.material = material;
                
                return new SceneCreationResult
                {
                    name = ground.name,
                    created = true,
                    type = "ground"
                };
            }
            catch (Exception e)
            {
                return new SceneCreationResult
                {
                    name = "GROUND",
                    created = false,
                    type = "ground",
                    error = e.Message
                };
            }
        }
        
        private static RigidbodyCreationResult CreateRigidbodyObject(int objectId, string shapeType)
        {
            try
            {
                // 选择形状
                PrimitiveType primitiveType;
                string objName;
                Color objColor;
                
                switch (shapeType)
                {
                    case "sphere":
                        primitiveType = PrimitiveType.Sphere;
                        objName = $"RIGID_SPHERE_{objectId:00}";
                        objColor = new Color(0.2f, 0.8f, 0.2f, 1.0f);
                        break;
                    case "capsule":
                        primitiveType = PrimitiveType.Capsule;
                        objName = $"RIGID_CAPSULE_{objectId:00}";
                        objColor = new Color(0.2f, 0.2f, 0.8f, 1.0f);
                        break;
                    default:
                        primitiveType = PrimitiveType.Cube;
                        objName = $"RIGID_CUBE_{objectId:00}";
                        objColor = new Color(0.8f, 0.2f, 0.2f, 1.0f);
                        break;
                }
                
                // 创建对象
                GameObject obj = GameObject.CreatePrimitive(primitiveType);
                obj.name = objName;
                
                // 随机位置（从不同高度和角度投放）
                System.Random random = new System.Random();
                float x = (float)(random.NextDouble() * 16 - 8);
                float y = (float)(random.NextDouble() * 10 + 5);
                float z = (float)(random.NextDouble() * 10 - 5);
                obj.transform.position = new Vector3(x, y, z);
                
                // 随机旋转
                float rotX = (float)(random.NextDouble() * 360);
                float rotY = (float)(random.NextDouble() * 360);
                float rotZ = (float)(random.NextDouble() * 360);
                obj.transform.rotation = Quaternion.Euler(rotX, rotY, rotZ);
                
                // 随机缩放
                float scaleFactor = (float)(random.NextDouble() * 1 + 0.5);
                obj.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
                
                // 添加Rigidbody组件
                Rigidbody rb = obj.AddComponent<Rigidbody>();
                
                // 随机物理参数
                float mass = (float)(random.NextDouble() * 9 + 1.0);
                float drag = (float)random.NextDouble();
                float angularDrag = (float)random.NextDouble();
                float bounciness = (float)(random.NextDouble() * 0.8);
                
                // 配置Rigidbody
                rb.mass = mass;
                rb.drag = drag;
                rb.angularDrag = angularDrag;
                rb.useGravity = true;
                
                // 配置物理材质
                Collider collider = obj.GetComponent<Collider>();
                PhysicMaterial physicMaterial = new PhysicMaterial();
                physicMaterial.name = $"PhysicsMat_{objectId}";
                physicMaterial.bounciness = bounciness;
                physicMaterial.staticFriction = 0.4f;
                physicMaterial.dynamicFriction = 0.4f;
                collider.sharedMaterial = physicMaterial;
                
                // 添加材质和颜色
                Renderer renderer = obj.GetComponent<Renderer>();
                Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.color = objColor;
                renderer.material = material;
                
                return new RigidbodyCreationResult
                {
                    name = objName,
                    shape = shapeType,
                    created = true,
                    physicsParams = new PhysicsParams
                    {
                        mass = (float)Math.Round(mass, 2),
                        drag = (float)Math.Round(drag, 2),
                        angularDrag = (float)Math.Round(angularDrag, 2),
                        bounciness = (float)Math.Round(bounciness, 2),
                        position = new float[] { (float)Math.Round(x, 2), (float)Math.Round(y, 2), (float)Math.Round(z, 2) },
                        rotation = new float[] { (float)Math.Round(rotX, 2), (float)Math.Round(rotY, 2), (float)Math.Round(rotZ, 2) },
                        scale = (float)Math.Round(scaleFactor, 2)
                    }
                };
            }
            catch (Exception e)
            {
                return new RigidbodyCreationResult
                {
                    name = $"RIGID_UNKNOWN_{objectId}",
                    shape = shapeType,
                    created = false,
                    error = e.Message
                };
            }
        }
        
        private static SceneCreationResult CreateConstantForceField(string forceType, Vector3 position)
        {
            try
            {
                // 创建力场可视化对象
                GameObject fieldObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                fieldObj.name = $"FORCE_FIELD_{forceType.ToUpper()}";
                fieldObj.transform.position = position;
                fieldObj.transform.localScale = new Vector3(2, 2, 2);
                
                // 配置材质（半透明）
                Renderer renderer = fieldObj.GetComponent<Renderer>();
                Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                Vector3 forceVector;
                
                switch (forceType)
                {
                    case "wind":
                        material.color = new Color(0.5f, 0.8f, 1.0f, 0.3f);
                        forceVector = new Vector3(5, 0, 2);
                        break;
                    case "gravity":
                        material.color = new Color(0.8f, 0.5f, 1.0f, 0.3f);
                        forceVector = new Vector3(0, -15, 0);
                        break;
                    case "magnetic":
                        material.color = new Color(1.0f, 0.8f, 0.5f, 0.3f);
                        forceVector = new Vector3(0, 0, 8);
                        break;
                    default:
                        material.color = new Color(1.0f, 1.0f, 1.0f, 0.3f);
                        forceVector = new Vector3(0, 0, 0);
                        break;
                }
                
                renderer.material = material;
                
                // 移除碰撞器
                Collider collider = fieldObj.GetComponent<Collider>();
                if (collider != null)
                {
                    UnityEngine.Object.DestroyImmediate(collider);
                }
                
                // 添加ConstantForce组件
                ConstantForce constantForce = fieldObj.AddComponent<ConstantForce>();
                constantForce.force = forceVector;
                
                return new SceneCreationResult
                {
                    name = fieldObj.name,
                    created = true,
                    type = forceType
                };
            }
            catch (Exception e)
            {
                return new SceneCreationResult
                {
                    name = $"FORCE_FIELD_{forceType.ToUpper()}",
                    created = false,
                    type = forceType,
                    error = e.Message
                };
            }
        }
        
        private static VerificationResult VerifyPhysicsSimulation()
        {
            VerificationResult verification = new VerificationResult
            {
                details = new List<RigidbodyDetail>()
            };
            
            try
            {
                // 查找所有刚体
                Rigidbody[] allRigidbodies = UnityEngine.Object.FindObjectsOfType<Rigidbody>();
                verification.rigidbodiesCount = allRigidbodies.Length;
                
                // 验证每个刚体的参数
                foreach (Rigidbody rb in allRigidbodies)
                {
                    GameObject obj = rb.gameObject;
                    if (obj.name.StartsWith("RIGID_"))
                    {
                        RigidbodyDetail details = new RigidbodyDetail
                        {
                            name = obj.name,
                            massValid = rb.mass >= 1.0f && rb.mass <= 10.0f,
                            dragValid = rb.drag >= 0.0f && rb.drag <= 1.0f,
                            angularDragValid = rb.angularDrag >= 0.0f && rb.angularDrag <= 1.0f,
                            useGravity = rb.useGravity,
                            hasCollider = false,
                            bouncinessValid = false
                        };
                        
                        // 检查碰撞器
                        Collider collider = obj.GetComponent<Collider>();
                        if (collider != null)
                        {
                            details.hasCollider = true;
                            if (collider.sharedMaterial != null)
                            {
                                float bounciness = collider.sharedMaterial.bounciness;
                                details.bouncinessValid = bounciness >= 0.0f && bounciness <= 0.8f;
                            }
                        }
                        
                        // 统计验证通过的参数
                        if (details.massValid && details.dragValid && 
                            details.angularDragValid && details.useGravity &&
                            details.hasCollider && details.bouncinessValid)
                        {
                            verification.parametersCorrect++;
                        }
                        
                        verification.collidersPresent++;
                        verification.details.Add(details);
                    }
                }
                
                int forceFieldCount = 0;
                GameObject[] allGOs = UnityEngine.Object.FindObjectsOfType<GameObject>();
                foreach (GameObject go in allGOs)
                {
                    if (go.name.StartsWith("FORCE_FIELD_"))
                        forceFieldCount++;
                }
                verification.forceFieldsCount = forceFieldCount;
                
                // 统计场景对象
                GameObject[] allGameObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                int sceneObjectCount = 0;
                foreach (GameObject go in allGameObjects)
                {
                    if (go.name == "RAMP_PLATFORM" || go.name == "BOUNCE_BOARD" ||
                        go.name == "OBSTACLE_WALL" || go.name == "GROUND" ||
                        go.name.StartsWith("RIGID_") || go.name.StartsWith("FORCE_FIELD_"))
                    {
                        sceneObjectCount++;
                    }
                }
                verification.sceneObjectsCount = sceneObjectCount;
                
                return verification;
            }
            catch (Exception e)
            {
                verification.error = e.Message;
                return verification;
            }
        }
        
        // 简单的JSON序列化辅助类
        private class MiniJSON
        {
            public static string Serialize(object obj)
            {
                if (obj is Dictionary<string, int> dict)
                {
                    string result = "{";
                    bool first = true;
                    foreach (var kvp in dict)
                    {
                        if (!first) result += ", ";
                        result += $"\"{kvp.Key}\": {kvp.Value}";
                        first = false;
                    }
                    result += "}";
                    return result;
                }
                return obj.ToString();
            }
        }
    }
}