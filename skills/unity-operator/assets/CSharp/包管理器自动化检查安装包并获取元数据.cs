using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EditorAutomation
{
    public class PackageManagerAutomation
    {
        public static string execute()
        {
            try
            {
                Debug.Log("===== Package Manager Automation Started =====");
                
                var finalResult = new ResultData
                {
                    success = false,
                    packageInstalled = false,
                    packageVersion = null,
                    packageStatus = "disabled",
                    installedPackagesCount = 0,
                    packageList = new List<PackageInfoData>(), 
                    inputSystemMetadata = null,
                    dependenciesChecked = false,
                    errors = new List<string>(),
                    warnings = new List<string>()
                };
                
                string packageId = "com.unity.inputsystem";
                
                Debug.Log("Step 1: Checking package status...");
                var packageStatus = CheckPackageStatus(packageId);
                finalResult.packageInstalled = packageStatus.isInstalled;
                finalResult.packageVersion = packageStatus.version;
                finalResult.packageStatus = packageStatus.status;
                
                Debug.Log($"Package {packageId} exists: {packageStatus.isInstalled}");
                if (!string.IsNullOrEmpty(packageStatus.version))
                    Debug.Log($"Version: {packageStatus.version}");
                
                if (!packageStatus.isInstalled)
                {
                    Debug.Log("Step 2: Package not found, installing...");
                    
                    var versionInfo = GetAvailableVersions(packageId);
                    string targetVersion = "1.7.0";
                    
                    if (versionInfo.success && !string.IsNullOrEmpty(versionInfo.latestVersion))
                    {
                        targetVersion = versionInfo.latestVersion;
                        Debug.Log($"Found latest version: {targetVersion}");
                    }
                    else
                    {
                        finalResult.warnings.Add($"Could not search versions, using fallback: {targetVersion}");
                        Debug.LogWarning($"Using fallback version: {targetVersion}");
                    }
                    
                    var installResult = InstallPackage(packageId, targetVersion);
                    if (!installResult.success)
                    {
                        finalResult.errors.Add(installResult.message);
                        Debug.LogError($"Installation failed: {installResult.message}");
                        return FormatResult(finalResult);
                    }
                    
                    packageStatus = CheckPackageStatus(packageId);
                    finalResult.packageInstalled = packageStatus.isInstalled;
                    finalResult.packageVersion = packageStatus.version;
                }
                else
                {
                    Debug.Log("Step 2: Package already installed, skipping installation");
                }
                
                Debug.Log("Step 3: Verifying installation...");
                var verification = VerifyPackageInstallation(packageId);
                Debug.Log($"Package dir: {verification.inPackagesDir}");
                Debug.Log($"Manifest has ref: {verification.inManifest}");
                Debug.Log($"Files exist: {verification.filesExist}");
                
                Debug.Log("Step 4: Getting all installed packages...");
                var allPackages = GetAllInstalledPackages();
                finalResult.installedPackagesCount = allPackages.count;
                finalResult.packageList = allPackages.packages;
                Debug.Log($"Total packages: {allPackages.count}");
                
                Debug.Log("Step 5: Getting package metadata...");
                var metadata = GetPackageMetadata(packageId);
                if (metadata.metadata != null)
                {
                    finalResult.inputSystemMetadata = metadata.metadata;
                    Debug.Log($"Display name: {metadata.metadata.displayName}");
                }
                else if (metadata.error != null)
                {
                    finalResult.errors.Add(metadata.error);
                }
                
                Debug.Log("Step 6: Checking dependencies...");
                var depsResult = CheckDependencies(packageId);
                finalResult.dependenciesChecked = depsResult.dependenciesChecked;
                if (!depsResult.allDependenciesInstalled)
                {
                    var missingDeps = string.Join(", ", depsResult.missingDependencies);
                    finalResult.warnings.Add($"Missing dependencies: {missingDeps}");
                    Debug.LogWarning($"Missing: {missingDeps}");
                }
                
                finalResult.success = finalResult.packageInstalled && finalResult.errors.Count == 0;
                
                Debug.Log("===== Package Manager Automation Completed =====");
                
                return FormatResult(finalResult);
            }
            catch (Exception e)
            {
                Debug.LogError($"Fatal error: {e.Message}\n{e.StackTrace}");
                return $"{{\"success\":false,\"error\":\"{e.Message}\"}}";
            }
        }
        
        private static PackageStatus CheckPackageStatus(string packageId)
        {
            var result = new PackageStatus { isInstalled = false, version = null, status = "disabled" };
            
            try
            {
                var request = Client.List(true, true);
                while (!request.IsCompleted)
                {
                    System.Threading.Thread.Sleep(100);
                }
                
                if (request.Status == StatusCode.Success)
                {
                    foreach (var package in request.Result)
                    {
                        if (package.name == packageId)
                        {
                            result.isInstalled = true;
                            result.version = package.version;
                            result.status = "enabled";
                            break;
                        }
                    }
                }
                else
                {
                    Debug.LogError($"Failed to list packages: {request.Error.message}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error checking package: {e.Message}");
            }
            
            return result;
        }
        
        private static VersionInfo GetAvailableVersions(string packageId)
        {
            var result = new VersionInfo { success = false, versions = new List<string>(), latestVersion = null };
            
            try
            {
                Debug.Log($"Searching versions for {packageId}...");
                
                var request = Client.Search(packageId);
                while (!request.IsCompleted)
                {
                    System.Threading.Thread.Sleep(100);
                }
                
                if (request.Status == StatusCode.Success && request.Result.Length > 0)
                {
                    var packageInfo = request.Result[0];
                    
                    if (packageInfo.versions != null)
                    {
                        foreach (var version in packageInfo.versions.compatible)
                        {
                            result.versions.Add(version);
                        }
                        
                        result.latestVersion = packageInfo.versions.latestCompatible ?? packageInfo.versions.latest;
                        result.success = true;
                        
                        Debug.Log($"Found {result.versions.Count} compatible versions");
                        Debug.Log($"Latest version: {result.latestVersion}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Search failed or no results for {packageId}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception during search: {e.Message}");
            }
            
            return result;
        }
        
        private static InstallResult InstallPackage(string packageId, string version)
        {
            var result = new InstallResult { success = false, message = "", installedVersion = null };
            
            try
            {
                var installIdentifier = $"{packageId}@{version}";
                Debug.Log($"Installing {installIdentifier}...");
                
                var request = Client.Add(installIdentifier);
                while (!request.IsCompleted)
                {
                    System.Threading.Thread.Sleep(100);
                }
                
                if (request.Status == StatusCode.Success)
                {
                    result.success = true;
                    result.message = $"Package installed successfully (version {version})";
                    result.installedVersion = version;
                    Debug.Log($"Installation completed: {version}");
                }
                else
                {
                    result.message = $"Failed to install: {request.Error.message}";
                    Debug.LogError(result.message);
                }
            }
            catch (Exception e)
            {
                result.message = $"Error installing package: {e.Message}";
                Debug.LogError(result.message);
            }
            
            return result;
        }
        
        private static InstallationVerification VerifyPackageInstallation(string packageId)
        {
            var result = new InstallationVerification { inPackagesDir = false, inManifest = false, filesExist = false };
            
            try
            {
                string projectPath = Path.GetDirectoryName(Application.dataPath);
                string packagesDir = Path.Combine(projectPath, "Packages");
                string packageDir = Path.Combine(packagesDir, packageId);
                string manifestPath = Path.Combine(packagesDir, "manifest.json");
                string packageJsonPath = Path.Combine(packageDir, "package.json");
                
                if (Directory.Exists(packageDir))
                    result.inPackagesDir = true;
                
                if (File.Exists(manifestPath))
                {
                    string manifestContent = File.ReadAllText(manifestPath);
                    if (manifestContent.Contains(packageId))
                        result.inManifest = true;
                }
                
                if (File.Exists(packageJsonPath))
                    result.filesExist = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error verifying installation: {e.Message}");
            }
            
            return result;
        }
        
        private static PackageListResult GetAllInstalledPackages()
        {
            var result = new PackageListResult { packages = new List<PackageInfoData>(), count = 0 };
            
            try
            {
                var request = Client.List(true, true);
                while (!request.IsCompleted)
                {
                    System.Threading.Thread.Sleep(100);
                }
                
                if (request.Status == StatusCode.Success)
                {
                    foreach (var package in request.Result)
                    {
                        var packageInfo = new PackageInfoData
                        {
                            name = package.name,
                            version = package.version,
                            source = package.source.ToString(),
                            displayName = package.displayName ?? "",
                            description = package.description ?? ""
                        };
                        result.packages.Add(packageInfo);
                    }
                    result.count = result.packages.Count;
                }
                else
                {
                    Debug.LogError($"Failed to get package list: {request.Error.message}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error getting package list: {e.Message}");
            }
            
            return result;
        }
        
        private static MetadataResult GetPackageMetadata(string packageId)
        {
            var result = new MetadataResult { metadata = null, error = null };
            
            try
            {
                var request = Client.List(true, true);
                while (!request.IsCompleted)
                {
                    System.Threading.Thread.Sleep(100);
                }
                
                if (request.Status == StatusCode.Success)
                {
                    foreach (var package in request.Result)
                    {
                        if (package.name == packageId)
                        {
                            result.metadata = new PackageMetadataData
                            {
                                name = package.name,
                                version = package.version,
                                displayName = package.displayName ?? "",
                                description = package.description ?? "",
                                documentationUrl = $"https://docs.unity3d.com/Packages/{packageId}@latest",
                                dependencies = new List<DependencyData>()
                            };
                            
                            try
                            {
                                if (package.dependencies != null)
                                {
                                    foreach (var dep in package.dependencies)
                                    {
                                        result.metadata.dependencies.Add(new DependencyData
                                        {
                                            name = dep.name,
                                            version = dep.version
                                        });
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                Debug.LogWarning("Could not read dependencies");
                            }
                            
                            break;
                        }
                    }
                }
                else
                {
                    result.error = $"Failed to get package info: {request.Error.message}";
                }
            }
            catch (Exception e)
            {
                result.error = $"Error getting metadata: {e.Message}";
            }
            
            return result;
        }
        
        private static DependenciesResult CheckDependencies(string packageId)
        {
            var result = new DependenciesResult { 
                dependenciesChecked = true, 
                missingDependencies = new List<string>(), 
                allDependenciesInstalled = true 
            };
            
            try
            {
                var metadata = GetPackageMetadata(packageId);
                if (metadata.metadata != null && metadata.metadata.dependencies.Count > 0)
                {
                    var installedPackages = GetAllInstalledPackages();
                    var installedPackageNames = new HashSet<string>();
                    foreach (var pkg in installedPackages.packages)
                    {
                        installedPackageNames.Add(pkg.name);
                    }
                    
                    foreach (var dep in metadata.metadata.dependencies)
                    {
                        if (!installedPackageNames.Contains(dep.name))
                        {
                            result.missingDependencies.Add(dep.name);
                        }
                    }
                    
                    if (result.missingDependencies.Count > 0)
                        result.allDependenciesInstalled = false;
                }
            }
            catch (Exception e)
            {
                result.dependenciesChecked = false;
                Debug.LogError($"Error checking dependencies: {e.Message}");
            }
            
            return result;
        }
        
        private static string FormatResult(ResultData result)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{\"success\": " + result.success.ToString().ToLower() + ",");
            sb.AppendLine(" \"packageInstalled\": " + result.packageInstalled.ToString().ToLower() + ",");
            sb.AppendLine(" \"packageVersion\": \"" + (result.packageVersion ?? "null") + "\",");
            sb.AppendLine(" \"packageStatus\": \"" + result.packageStatus + "\",");
            sb.AppendLine(" \"installedPackagesCount\": " + result.installedPackagesCount + ",");
            sb.AppendLine(" \"packageList\": [");
            
            for (int i = 0; i < result.packageList.Count; i++)
            {
                var pkg = result.packageList[i];
                sb.AppendLine("   {\"name\": \"" + pkg.name + "\",");
                sb.AppendLine("    \"version\": \"" + pkg.version + "\",");
                sb.AppendLine("    \"source\": \"" + pkg.source + "\",");
                sb.AppendLine("    \"displayName\": \"" + pkg.displayName + "\",");
                sb.AppendLine("    \"description\": \"" + EscapeJson(pkg.description) + "\"}");
                if (i < result.packageList.Count - 1)
                    sb.AppendLine(",");
            }
            
            sb.AppendLine(" ],");
            sb.AppendLine(" \"inputSystemMetadata\": " + (result.inputSystemMetadata != null ? FormatMetadata(result.inputSystemMetadata) : "null") + ",");
            sb.AppendLine(" \"dependenciesChecked\": " + result.dependenciesChecked.ToString().ToLower() + ",");
            sb.AppendLine(" \"errors\": [" + FormatStringList(result.errors) + "],");
            sb.AppendLine(" \"warnings\": [" + FormatStringList(result.warnings) + "]");
            sb.Append("}");
            
            return sb.ToString();
        }
        
        private static string FormatMetadata(PackageMetadataData metadata)
        {
            var sb = new StringBuilder();
            sb.Append("{\"name\": \"" + metadata.name + "\",");
            sb.Append(" \"version\": \"" + metadata.version + "\",");
            sb.Append(" \"displayName\": \"" + metadata.displayName + "\",");
            sb.Append(" \"description\": \"" + EscapeJson(metadata.description) + "\",");
            sb.Append(" \"documentationUrl\": \"" + metadata.documentationUrl + "\",");
            sb.Append(" \"dependencies\": [");
            
            for (int i = 0; i < metadata.dependencies.Count; i++)
            {
                var dep = metadata.dependencies[i];
                sb.Append("{\"name\": \"" + dep.name + "\", \"version\": \"" + dep.version + "\"}");
                if (i < metadata.dependencies.Count - 1) sb.Append(", ");
            }
            
            sb.Append("]}");
            return sb.ToString();
        }
        
        private static string FormatStringList(List<string> list)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append("\"" + EscapeJson(list[i]) + "\"");
            }
            return sb.ToString();
        }
        
        private static string EscapeJson(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", " ").Replace("\r", " ").Trim();
        }
        
        // Data Classes
        [Serializable]
        private class ResultData
        {
            public bool success;
            public bool packageInstalled;
            public string packageVersion;
            public string packageStatus;
            public int installedPackagesCount;
            public List<PackageInfoData> packageList;
            public PackageMetadataData inputSystemMetadata;
            public bool dependenciesChecked;
            public List<string> errors;
            public List<string> warnings;
        }
        
        [Serializable]
        private class PackageInfoData
        {
            public string name;
            public string version;
            public string source;
            public string displayName;
            public string description;
        }
        
        [Serializable]
        private class PackageMetadataData
        {
            public string name;
            public string version;
            public string displayName;
            public string description;
            public string documentationUrl;
            public List<DependencyData> dependencies;
        }
        
        [Serializable]
        private class DependencyData
        {
            public string name;
            public string version;
        }
        
        // Helper Result Classes
        private class PackageStatus
        {
            public bool isInstalled;
            public string version;
            public string status;
        }
        
        private class VersionInfo
        {
            public bool success;
            public List<string> versions;
            public string latestVersion;
        }
        
        private class InstallResult
        {
            public bool success;
            public string message;
            public string installedVersion;
        }
        
        private class InstallationVerification
        {
            public bool inPackagesDir;
            public bool inManifest;
            public bool filesExist;
        }
        
        private class MetadataResult
        {
            public PackageMetadataData metadata;
            public string error;
        }
        
        private class DependenciesResult
        {
            public bool dependenciesChecked;
            public List<string> missingDependencies;
            public bool allDependenciesInstalled;
        }
        
        private class PackageListResult
        {
            public List<PackageInfoData> packages;
            public int count;
        }
    }
}