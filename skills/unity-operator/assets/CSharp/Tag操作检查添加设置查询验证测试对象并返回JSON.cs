using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EditorAutomation
{
    [Serializable]
    public class TagOperations_20250104_143022
    {
        [Serializable]
        public class OperationResult
        {
            public int step;
            public string action;
            public string tag_name;
            public bool success;
            public string message;
            public bool exists;
            public string object_name;
            public bool tag_correct;
            public string expected_tag;
            public string actual_tag;
            public bool exception_thrown;
            public int tag_count;
        }

        [Serializable]
        public class VerificationResult
        {
            public bool tag_exists_check_correct;
            public bool tag_added_successfully;
            public bool tag_set_successful;
            public bool tag_query_consistent;
            public bool error_handling_working;
            public bool all_tags_query_successful;
            public bool overall_success;
        }

        [Serializable]
        public class ResultData
        {
            public bool success;
            public string test_tag_name;
            public string nonexistent_tag;
            public List<OperationResult> operations;
            public VerificationResult verification;
            public List<string> all_tags;
            public string message;

            public ResultData()
            {
                operations = new List<OperationResult>();
                all_tags = new List<string>();
                verification = new VerificationResult();
            }
        }

        private static bool CheckTagExists(string tagName)
        {
            var allTags = UnityEditorInternal.InternalEditorUtility.tags;
            return allTags.Contains(tagName);
        }

        private static bool AddTag(string tagName, out string message)
        {
            try
            {
                if (!CheckTagExists(tagName))
                {
                    UnityEditorInternal.InternalEditorUtility.AddTag(tagName);
                    message = $"Tag '{tagName}' 添加成功";
                    return true;
                }
                else
                {
                    message = $"Tag '{tagName}' 已存在";
                    return true;
                }
            }
            catch (Exception e)
            {
                message = $"添加Tag失败: {e.Message}";
                return false;
            }
        }

        private static GameObject CreateTestObject(string objectName, out string message)
        {
            try
            {
                GameObject testObj = new GameObject(objectName);
                message = $"测试对象 '{objectName}' 创建成功";
                return testObj;
            }
            catch (Exception e)
            {
                message = $"创建测试对象失败: {e.Message}";
                return null;
            }
        }

        private static bool SetObjectTag(GameObject gameObj, string tagName, out string message)
        {
            try
            {
                gameObj.tag = tagName;
                message = $"对象Tag设置为 '{tagName}' 成功";
                return true;
            }
            catch (Exception e)
            {
                message = $"设置Tag失败: {e.Message}";
                return false;
            }
        }

        private static string GetObjectTag(GameObject gameObj, out string message)
        {
            try
            {
                message = "获取Tag成功";
                return gameObj.tag;
            }
            catch (Exception e)
            {
                message = $"获取Tag失败: {e.Message}";
                return string.Empty;
            }
        }

        private static List<string> GetAllTags(out string message)
        {
            try
            {
                var allTags = UnityEditorInternal.InternalEditorUtility.tags;
                message = "获取所有Tags成功";
                return new List<string>(allTags);
            }
            catch (Exception e)
            {
                message = $"获取所有Tags失败: {e.Message}";
                return new List<string>();
            }
        }

        public static string execute()
        {
            ResultData result = new ResultData
            {
                test_tag_name = "TestTag",
                nonexistent_tag = "NonExistentTag_12345",
                success = false,
                message = ""
            };

            try
            {
                Debug.Log("=== 开始Tag操作测试 ===");

                // 步骤1: 检查"TestTag"是否存在
                Debug.Log("步骤1: 检查 'TestTag' 是否存在");
                bool tagExists = CheckTagExists(result.test_tag_name);
                result.operations.Add(new OperationResult
                {
                    step = 1,
                    action = "检查Tag存在性",
                    tag_name = result.test_tag_name,
                    exists = tagExists,
                    success = true,
                    message = $"Tag '{result.test_tag_name}' 存在: {tagExists}"
                });
                Debug.Log($"  - Tag存在性: {tagExists}");

                // 步骤2: 如不存在，添加"TestTag"到项目Tags列表
                Debug.Log("步骤2: 添加 'TestTag' 到项目（如需要）");
                if (!tagExists)
                {
                    bool addSuccess;
                    string addMessage;
                    addSuccess = AddTag(result.test_tag_name, out addMessage);
                    result.operations.Add(new OperationResult
                    {
                        step = 2,
                        action = "添加Tag",
                        tag_name = result.test_tag_name,
                        success = addSuccess,
                        message = addMessage
                    });
                    Debug.Log($"  - {addMessage}");
                }
                else
                {
                    result.operations.Add(new OperationResult
                    {
                        step = 2,
                        action = "添加Tag",
                        tag_name = result.test_tag_name,
                        success = true,
                        message = $"Tag '{result.test_tag_name}' 已存在，跳过添加"
                    });
                    Debug.Log($"  - Tag已存在，跳过添加");
                }

                // 步骤3: 创建测试对象
                Debug.Log("步骤3: 创建测试对象");
                GameObject testObj;
                string createMessage;
                testObj = CreateTestObject("TagTestObject", out createMessage);
                result.operations.Add(new OperationResult
                {
                    step = 3,
                    action = "创建测试对象",
                    object_name = testObj != null ? testObj.name : "",
                    success = testObj != null,
                    message = createMessage
                });
                Debug.Log($"  - {createMessage}");

                if (testObj == null)
                {
                    throw new Exception("测试对象创建失败，无法继续后续测试");
                }

                // 步骤4: 将对象的Tag设置为"TestTag"
                Debug.Log("步骤4: 设置对象Tag为 'TestTag'");
                bool setSuccess;
                string setMessage;
                setSuccess = SetObjectTag(testObj, result.test_tag_name, out setMessage);
                result.operations.Add(new OperationResult
                {
                    step = 4,
                    action = "设置Tag",
                    tag_name = result.test_tag_name,
                    object_name = testObj.name,
                    success = setSuccess,
                    message = setMessage
                });
                Debug.Log($"  - {setMessage}");

                if (!setSuccess)
                {
                    throw new Exception($"设置Tag失败: {setMessage}");
                }

                // 步骤5: 查询并验证对象的Tag已正确设置
                Debug.Log("步骤5: 查询并验证对象Tag");
                string actualTag;
                string queryMessage;
                actualTag = GetObjectTag(testObj, out queryMessage);
                bool tagCorrect = actualTag == result.test_tag_name;
                result.operations.Add(new OperationResult
                {
                    step = 5,
                    action = "查询并验证Tag",
                    expected_tag = result.test_tag_name,
                    actual_tag = actualTag,
                    tag_correct = tagCorrect,
                    success = tagCorrect,
                    message = $"期望: {result.test_tag_name}, 实际: {actualTag}"
                });
                Debug.Log($"  - 期望Tag: '{result.test_tag_name}'");
                Debug.Log($"  - 实际Tag: '{actualTag}'");
                Debug.Log($"  - 验证结果: {(tagCorrect ? "✓ 正确" : "✗ 错误")}");

                // 步骤6: 尝试设置不存在的Tag（测试错误处理）
                Debug.Log("步骤6: 测试设置不存在的Tag（错误处理）");
                string nonexistentTag = result.nonexistent_tag;
                bool exceptionThrown = false;
                bool tagActuallySet = false;

                try
                {
                    // 先确认这个Tag不存在
                    bool nonexistentExists = CheckTagExists(nonexistentTag);
                    if (nonexistentExists)
                    {
                        // 如果意外存在，生成一个新的不存在的Tag
                        nonexistentTag = $"NonExistentTag_{DateTime.Now.Ticks}";
                        result.nonexistent_tag = nonexistentTag;
                    }

                    // 尝试设置不存在的Tag - Unity应该会抛出异常或警告
                    testObj.tag = nonexistentTag;
                    // 如果到这里没有异常，检查是否真的设置成功了
                    string actualNonexistentTag = testObj.tag;
                    tagActuallySet = actualNonexistentTag == nonexistentTag;

                    result.operations.Add(new OperationResult
                    {
                        step = 6,
                        action = "测试错误处理（设置不存在的Tag）",
                        tag_name = nonexistentTag,
                        exception_thrown = false,
                        success = false,  // 警告情况
                        message = "警告: 不存在的Tag被接受（Unity可能自动创建了Tag或允许设置）"
                    });
                    Debug.LogWarning($"  - 警告: 不存在的Tag '{nonexistentTag}' 被接受");
                    Debug.LogWarning($"  - Unity可能允许设置任意Tag字符串");
                }
                catch (Exception e)
                {
                    exceptionThrown = true;
                    result.operations.Add(new OperationResult
                    {
                        step = 6,
                        action = "测试错误处理（设置不存在的Tag）",
                        tag_name = nonexistentTag,
                        exception_thrown = true,
                        success = true,  // 成功捕获错误
                        message = $"正确捕获错误: {e.Message}"
                    });
                    Debug.Log($"  - ✓ 正确捕获错误: {e.Message}");
                }

                // 步骤7: 查询项目的所有可用Tags
                Debug.Log("步骤7: 查询项目所有可用Tags");
                List<string> allTags;
                string allTagsMessage;
                allTags = GetAllTags(out allTagsMessage);
                result.all_tags = allTags;
                result.operations.Add(new OperationResult
                {
                    step = 7,
                    action = "查询所有Tags",
                    tag_count = allTags.Count,
                    success = true,
                    message = allTagsMessage
                });
                Debug.Log($"  - {allTagsMessage}");
                Debug.Log($"  - Tag总数: {allTags.Count}");
                Debug.Log($"  - 所有Tags: {string.Join(", ", allTags.Take(10))}{(allTags.Count > 10 ? "..." : "")}");

                // 验证结果总结
                Debug.Log("=== 验证结果总结 ===");

                result.verification = new VerificationResult
                {
                    tag_exists_check_correct = result.operations[0].success,
                    tag_added_successfully = result.operations[1].success,
                    tag_set_successful = result.operations[3].success,
                    tag_query_consistent = result.operations[4].tag_correct,
                    error_handling_working = result.operations[5].success,
                    all_tags_query_successful = result.operations[6].success,
                    overall_success = result.operations.All(op => op.success)
                };

                if (result.verification.overall_success)
                {
                    result.success = true;
                    result.message = "所有Tag操作测试成功完成";
                    Debug.Log("✓ 所有测试步骤成功");
                }
                else
                {
                    result.success = false;
                    result.message = "部分测试步骤失败";
                    Debug.Log("✗ 部分测试步骤失败");
                }

                Debug.Log("=== 测试完成 ===");
            }
            catch (Exception e)
            {
                result.success = false;
                result.message = $"测试执行失败: {e.Message}";
                Debug.LogError($"测试执行失败: {e.Message}");
                Debug.LogError(e.StackTrace);
            }

            // 返回Json字符串
            return JsonUtility.ToJson(result, true);
        }
    }
}
