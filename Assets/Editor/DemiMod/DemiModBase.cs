#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ModIO;
using ModIO.Implementation;
using Unity.SharpZipLib.Utils;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Base script for all mod exporters.
/// Mods are setup and created in their individual scripts, but this contains the base functions for exporting mods.
/// </summary>
public static class DemiModBase
{
    public static DataHolder DataHolder;
    
    public static BuildTarget buildTarget = BuildTarget.StandaloneWindows64;
    
    /// <summary>
    /// ModsFolderPath is the location where mods are stored on the user's computer, chosen by the user.
    /// </summary>
    public static string modsFolderPath => GetDataHolder().GetUserDefinedModsLocation();
        //Application.persistentDataPath + "/mod.io/04747/data/mods";
    
    // Paths for storing mods prefabs once they've been created.
    public static string unityAssetsModsFolderPath => Application.dataPath + "/MODS";
    public static string unityAssetsAvatarModsFolderPath => unityAssetsModsFolderPath + "/Avatars";
    public static string unityAssetsMapModsFolderPath => unityAssetsModsFolderPath + "/Maps";
    public static string unityAssetsEnemyModsFolderPath => unityAssetsModsFolderPath + "/Enemies";
    
    
    
    public static string windowsbuildPath = "C:/Users/Public/mod.io/4747/mods/{LOCAL_FILE_NAME}/";
    public static string editorBuildPath = "{UnityEngine.Application.persistentDataPath}/mod.io/04747/data/mods/{LOCAL_FILE_NAME}/";
    
    
    /// <summary>
    /// Location where files are stored.
    /// todo: Create a folder here and zip it, then export to mod.io 
    /// </summary>
    public static string exportPath => DemiModBase.FormatForFileExplorer(modsFolderPath);  
    // + "/" + EditorUserBuildSettings.selectedStandaloneTarget);
    
    
    // Scene
    public static Scene currentScene => SceneManager.GetActiveScene();
    
    //public static bool openAfterExport = EditorPrefs.GetBool("OpenAfterExport", false);
    
    
    private static UserProfile user;
    private static bool isAwaitingServerResponse = false;
     
    
    public static void ExportWindows(ModType modType, GameObject target) 
    {
        SwitchToWindows();
        BuildAddressable(BuildTarget.StandaloneWindows64, modType, target);
    }

    public static void ExportAndroid(ModType modType, GameObject target)
    {
        SwitchToAndroid();
        BuildAddressable(BuildTarget.Android, modType, target);
    }
    
    
    private static void BuildAddressable(BuildTarget buildTarget, ModType modType, GameObject target)
    {
        string finalPath = "";
        string targetName = "";
        string label = "";
        
        
        // Get final path of object in database.
        switch (modType)
        {
            case ModType.Avatar:
                finalPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(target);
                targetName = target.name;
                label = "Player Avatar";
                break;
            
            case ModType.Map:
                finalPath = currentScene.path;
                targetName = currentScene.name;
                label = "Map";
                break;
            
            case ModType.Enemy:
                finalPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(target);
                //finalPath = Path.Combine(Application.dataPath + "/" + target.name);
                
                Debug.Log("Final path: " + finalPath);
                
                targetName = target.name;
                
                label = "Enemy Avatar";
                break;
        }


        
        Debug.Log("Prefab/Scene relative to project path: " + finalPath);
        
        var group = AddressableAssetSettingsDefaultObject.Settings.FindGroup("Default Local Group");
        var guid = AssetDatabase.AssetPathToGUID(finalPath);
        
        
        // Set the bundles' naming style to custom, and make the name as unique as possible to avoid cross-mod conflicts.
        AddressableAssetSettingsDefaultObject.Settings.ShaderBundleNaming = ShaderBundleNaming.Custom;
        AddressableAssetSettingsDefaultObject.Settings.ShaderBundleCustomNaming = "_Shaders"; //targetName + "_Shaders" + DateTime.Now.Minute + DateTime.Now.Second;
        
        AddressableAssetSettingsDefaultObject.Settings.MonoScriptBundleNaming = MonoScriptBundleNaming.Custom;
        AddressableAssetSettingsDefaultObject.Settings.MonoScriptBundleCustomNaming = "_Mono"; //targetName + "_Mono" + DateTime.Now.Minute + DateTime.Now.Second;


        if (group == null || guid == null)
            return;

        foreach (AddressableAssetEntry entry in group.entries.ToList())
            group.RemoveAssetEntry(entry);

        var e = AddressableAssetSettingsDefaultObject.Settings.CreateOrMoveEntry(guid, group, false, false);
        var entriesAdded = new List<AddressableAssetEntry> { e };
        
        
        e.SetLabel(label, true, true, false);

        group.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, false, true);
        AddressableAssetSettingsDefaultObject.Settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, true, false);
        

        Debug.Log("Build target: " + buildTarget);
        
        string buildPath = GetDataHolder().userDefinedModsLocation + "/" + targetName;
        
        
        // We can dynamically change the LOAD path here, and replace the LOCAL_FILE_NAME with: MOD-ID/BUILD TARGET/AVATAR NAME
        if (buildTarget == BuildTarget.StandaloneWindows64)
        {
            if (buildPath.Contains(label))
            {
                buildPath = GetDataHolder().userDefinedModsLocation + "/" + targetName + " - PCVR";
            }
            else
            {
                buildPath = GetDataHolder().userDefinedModsLocation + "/" + label + " - " + targetName + " - PCVR";
            }
            
            Debug.Log("Setting load path for windows");
            AddressableAssetSettingsDefaultObject.Settings.profileSettings
                .SetValue(AddressableAssetSettingsDefaultObject.Settings.activeProfileId, "Local.LoadPath", windowsbuildPath); //editorBuildPath);
        }
        else if (buildTarget == BuildTarget.Android)
        {
            if (buildPath.Contains(label))
            {
                buildPath = GetDataHolder().userDefinedModsLocation + "/" + targetName + " - Android";
            }
            else
            {
                buildPath = GetDataHolder().userDefinedModsLocation + "/" + label + " - " + targetName + " - Android";
            }
            
            Debug.Log("Setting load path for android");
            AddressableAssetSettingsDefaultObject.Settings.profileSettings
                .SetValue(AddressableAssetSettingsDefaultObject.Settings.activeProfileId, "Local.LoadPath", "{UnityEngine.Application.persistentDataPath}/mod.io/4747/mods/{LOCAL_FILE_NAME}/");
            
        }

        GetDataHolder().lastAddressableBuildPath = buildPath;
        AssetDatabase.Refresh();
        EditorUtility.SetDirty(GetDataHolder());
        

        // Create two different folders. One for Windows, and one for Android.

        if (Directory.Exists(buildPath))
        {
            Debug.Log("Directory exists: " + buildPath);
            
            DirectoryInfo directoryInfo = new DirectoryInfo(buildPath);
            
            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                Debug.Log("Deleting file: " + file.FullName);
                file.Delete();
            }
        }
        else
        {
            Directory.CreateDirectory(buildPath);
            
            Debug.Log("Creating directory: " + buildPath);
        }

        
        AddressableAssetSettingsDefaultObject.Settings.profileSettings
            .SetValue(AddressableAssetSettingsDefaultObject.Settings.activeProfileId, "Local.BuildPath", buildPath);    //"[UnityEngine.Application.persistentDataPath]" + "/" + "mod.io/04747/data/mods/");

        
        AddressableAssetSettings.CleanPlayerContent(AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);
        AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
    }
    
    
    
    public static void SwitchToWindows()
    {
        buildTarget = BuildTarget.StandaloneWindows64;
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
        EditorUserBuildSettings.selectedStandaloneTarget = BuildTarget.StandaloneWindows64;
    }
    
    public static void SwitchToAndroid()
    {
        buildTarget = BuildTarget.Android;
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        EditorUserBuildSettings.selectedStandaloneTarget = BuildTarget.Android;
    }



    /// <summary>
    /// Location for storing mods in the Unity project once they're created. 
    /// </summary>
    /// <param name="modType"></param>
    /// <param name="modName"></param>
    public static string GetOrCreateModPath(ModType modType, string modName)
    {
        Debug.Log("Checking project folders for current mod target: " + modName);

        string unityAssetModPath = "";
        
        switch (modType)
        {
            case ModType.Avatar:
                unityAssetModPath = Path.Combine(unityAssetsAvatarModsFolderPath, modName);
                break;
            
            // We don't need to create a folder for maps, as they are scenes and don't create new prefabs.
            //case ModType.Map:
                //unityAssetModPath = Path.Combine(unityAssetsMapModsFolderPath, modName);
                //break;
            
            case ModType.Enemy:
                unityAssetModPath = Path.Combine(unityAssetsEnemyModsFolderPath, modName);
                break;
        }
        
        
        if (Directory.Exists(unityAssetModPath))
        {
            Debug.Log("Project folder already exists: " + unityAssetModPath);
        }
        else
        {
            Debug.Log("Project folder does not exist: " + unityAssetModPath + ". Creating folder now.");
            Directory.CreateDirectory(unityAssetModPath);
        }
        
        AssetDatabase.Refresh();
        
        return unityAssetModPath;
    }


    // Move all mods from the child folders to the parent folder before we start new ones.
    [ContextMenu("Move Old Mods To Parent Folder")]
    public static void MoveOldModsToParentFolder(string modName)
    {
        DirectoryInfo dirInfoWindows = new DirectoryInfo(unityAssetsAvatarModsFolderPath + "/" + BuildTarget.StandaloneWindows64.ToString());
        DirectoryInfo dirInfoAndroid = new DirectoryInfo(unityAssetsAvatarModsFolderPath + "/" + BuildTarget.Android.ToString());
        
        string targetPath = unityAssetsAvatarModsFolderPath;
        
        if(!Directory.Exists(dirInfoWindows.FullName))
        {
            //Directory.CreateDirectory(dirInfoWindows.FullName);
        }

        foreach (var dir in dirInfoWindows.GetDirectories())
        {
            Directory.Move(dir.FullName, targetPath + "/" + modName);
            
            Debug.Log("Moving directory: " + dir.FullName + "   to   " + targetPath + "/" + modName);
        }

        foreach (FileInfo fileInfo in dirInfoWindows.GetFiles())
        {
            File.Move(fileInfo.FullName, targetPath);
            
            Debug.Log("Moving file: " + fileInfo.FullName + "   to   " + targetPath + "/" + modName);
        }
        
        
        
        if(!Directory.Exists(dirInfoAndroid.FullName))
        {
            //Directory.CreateDirectory(dirInfoAndroid.FullName);
        }

        foreach (var dir in dirInfoAndroid.GetDirectories())
        {
            if (dir.Name != BuildTarget.StandaloneWindows64.ToString() && dir.Name != BuildTarget.Android.ToString())
            {
                Directory.Move(dir.FullName, targetPath + "/" + modName);
                
                Debug.Log("Moving directory: " + dir.FullName + "   to   " + targetPath + "/" + modName);
            }
        }


        foreach (FileInfo fileInfo in dirInfoAndroid.GetFiles())
        {
            File.Move(fileInfo.FullName, targetPath);
            
            Debug.Log("Moving file: " + fileInfo.FullName + "   to   " + targetPath + "/" + modName);
        }
        
        
        
        
        dirInfoWindows = new DirectoryInfo(unityAssetsMapModsFolderPath + "/" + BuildTarget.StandaloneWindows64.ToString());
        dirInfoAndroid = new DirectoryInfo(unityAssetsMapModsFolderPath + "/" + BuildTarget.Android.ToString());
        
        targetPath = unityAssetsMapModsFolderPath;
        
        if(!Directory.Exists(dirInfoWindows.FullName))
        {
            Directory.CreateDirectory(dirInfoWindows.FullName);
        }
        else
        {
            foreach (var dir in dirInfoWindows.GetDirectories())
            {
                if (dir.Name != BuildTarget.StandaloneWindows64.ToString() && dir.Name != BuildTarget.Android.ToString())
                {
                    Directory.Move(dir.FullName, targetPath + "/" + modName);
                    
                    Debug.Log("Moving directory: " + dir.FullName + "   to   " + targetPath + "/" + modName);
                }
            }

            foreach (FileInfo fileInfo in dirInfoWindows.GetFiles())
            {
                File.Move(fileInfo.FullName, targetPath);
                
                Debug.Log("Moving file: " + fileInfo.FullName + "   to   " + targetPath + "/" + modName);
            }
        }
        
        
        if(!Directory.Exists(dirInfoAndroid.FullName))
        {
            Directory.CreateDirectory(dirInfoAndroid.FullName);
        }
        else
        {
            foreach (var dir in dirInfoAndroid.GetDirectories())
            {
                if (dir.Name != BuildTarget.StandaloneWindows64.ToString() && dir.Name != BuildTarget.Android.ToString())
                {
                    Directory.Move(dir.FullName, targetPath + "/" + modName);
                    
                    Debug.Log("Moving directory: " + dir.FullName + "   to   " + targetPath + "/" + modName);
                }
            }
            
            foreach (FileInfo fileInfo in dirInfoAndroid.GetFiles())
            {
                File.Move(fileInfo.FullName, targetPath);
                
                Debug.Log("Moving file: " + fileInfo.FullName + "   to   " + targetPath + "/" + modName);
            }
        }
        
        
        
        
        
        dirInfoWindows = new DirectoryInfo(unityAssetsEnemyModsFolderPath + "/" + BuildTarget.StandaloneWindows64.ToString());
        dirInfoAndroid = new DirectoryInfo(unityAssetsEnemyModsFolderPath + "/" + BuildTarget.Android.ToString());
        
        targetPath = unityAssetsEnemyModsFolderPath;
        
        
        if(!Directory.Exists(dirInfoWindows.FullName))
        {
            Directory.CreateDirectory(dirInfoWindows.FullName);
        }
        else
        {
            foreach (var dir in dirInfoWindows.GetDirectories())
            {
                if (dir.Name != BuildTarget.StandaloneWindows64.ToString() && dir.Name != BuildTarget.Android.ToString())
                {
                    Directory.Move(dir.FullName, targetPath + "/" + modName);
                    
                    Debug.Log("Moving directory: " + dir.FullName + "   to   " + targetPath + "/" + modName);
                }
            }

            foreach (FileInfo fileInfo in dirInfoWindows.GetFiles())
            {
                File.Move(fileInfo.FullName, targetPath);
                
                Debug.Log("Moving file: " + fileInfo.FullName + "   to   " + targetPath + "/" + modName);
            }
        }
        
        
        if(!Directory.Exists(dirInfoAndroid.FullName))
        {
            Directory.CreateDirectory(dirInfoAndroid.FullName);
        }
        else
        {
            foreach (var dir in dirInfoAndroid.GetDirectories())
            {
                if (dir.Name != BuildTarget.StandaloneWindows64.ToString() && dir.Name != BuildTarget.Android.ToString())
                {
                    Directory.Move(dir.FullName, targetPath + "/" + modName);
                    
                    Debug.Log("Moving directory: " + dir.FullName + "   to   " + targetPath + "/" + modName);
                }
            }
            
            foreach (FileInfo fileInfo in dirInfoAndroid.GetFiles())
            {
                File.Move(fileInfo.FullName, targetPath);
                
                Debug.Log("Moving file: " + fileInfo.FullName + "   to   " + targetPath + "/" + modName);
            }
        }
        
        
        
        AssetDatabase.Refresh();
    }
    
    
    
        
    // Mod Process
    // 1. Add a Humanoid Model to the project.
    
    
    // Upload mods process:
    // 1. User must sign in to mod.io.
    // 2. User must assign a logo, name, and summary for the mod.
    // 3. User must assign a mod type (Avatar, Map, Enemy).
    
    
    static ModId newMod;
    static Texture2D logo;
    static CreationToken token;
    
    static void CreateModProfile()
    {
        token = ModIOUnity.GenerateCreationToken();
    
        ModProfileDetails profile = new ModProfileDetails();
        profile.name = "mod name";
        profile.summary = "a brief summary about this mod being submitted";
        profile.logo = logo;
    
        ModIOUnity.CreateModProfile(token, profile, CreateProfileCallback);
    }
    
    static void CreateProfileCallback(ResultAnd<ModId> response)
    {
        if (response.result.Succeeded())
        {
            newMod = response.value;
            Debug.Log("created new mod profile with id " + response.value.ToString());
        }
        else
        {
            Debug.Log("failed to create new mod profile");
        }
    }
    
    
    
    // Variables for Uploading Mods
    public static ModId modId;
    
    private static void UploadMod(string fileDirectory)
    {
        ModfileDetails modfile = new ModfileDetails();
        modfile.modId = modId;
         
        //modfile.directory = "files/mods/mod_123";
        modfile.directory = fileDirectory;
         
        ModIOUnity.UploadModfile(modfile, UploadModCallback);
    }
    
    static void UploadModCallback(Result result)
    {
        if (result.Succeeded())
        {
            Debug.Log("uploaded mod file");
        }
        else
        {
            Debug.Log("failed to upload mod file");
        }
    }

    
    
    
    public static async Task ZipFolder(string sourceFolder, string targetPath)
    {
        CompressOperationDirectory compressOperation = new CompressOperationDirectory(sourceFolder);

        try
        {
           // await compressOperation.Compress();
        }
        catch (Exception e)
        {
            Debug.Log($"error : {e.Message}");
        }
        
    }
    
    
    private static string Screenshot(string modName) 
    {
        var timestamp = System.DateTime.Now;
        var stampString = string.Format("_{0}-{1:00}-{2:00}_{3:00}-{4:00}-{5:00}", timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, timestamp.Minute, timestamp.Second);
        
        string screenshotFolder = Application.dataPath + "/Thumbnails/" + modName + "/";
        if (Directory.Exists(screenshotFolder))
        {
            //Directory.Delete(screenshotFolder, true);
        }
        else
        {
            Directory.CreateDirectory(screenshotFolder);
        }
        
        string screenshotPath = screenshotFolder + "/Screenshot" + stampString + ".png";

        RenderTexture screenTexture = new RenderTexture(1920, 1080, 16);
        Camera.allCameras[0].targetTexture = screenTexture;
        RenderTexture.active = screenTexture;
        Camera.allCameras[0].Render();
        Texture2D renderedTexture = new Texture2D(1920, 1080);
        renderedTexture.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
        RenderTexture.active = null;
        byte[] byteArray = renderedTexture.EncodeToPNG();
        System.IO.File.WriteAllBytes(screenshotPath, byteArray);

        AssetDatabase.Refresh();

        return screenshotPath;
    }
    
    

    /*
        static void UploadToServer() 
        {
            isAwaitingServerResponse = true;

            string profileFilePath = AssetDatabase.GetAssetPath(profile);

            Action<WebRequestError> onSubmissionFailed = (e) => { 
                EditorUtility.DisplayDialog("Upload Failed", "Failed to update the mod profile on the server.\n" + e.displayMessage, "Close");

                isAwaitingServerResponse = false;
                Repaint();
            };

            if (profile.modId > 0) 
            {
                ModManager.SubmitModChanges(profile.modId, profile.editableModProfile, (m) => ModProfileSubmissionSucceeded(m, profileFilePath), onSubmissionFailed);
            } 
            else 
            {
                ModManager.SubmitNewMod(profile.editableModProfile, (m) => ModProfileSubmissionSucceeded(m, profileFilePath), onSubmissionFailed);
            }
        }

        private static void ModProfileSubmissionSucceeded(ModProfile updatedProfile, string profileFilePath) 
        {
            if (updatedProfile == null) 
            {
                isAwaitingServerResponse = false;
                return;
            }


            // Update ScriptableModProfile
            profile.modId = updatedProfile.id;
            profile.editableModProfile = EditableModProfile.CreateFromProfile(updatedProfile);
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();

            // Upload Build
            if (Directory.Exists(exportPath)) 
            {
                Action<WebRequestError> onSubmissionFailed = (e) => {
                    EditorUtility.DisplayDialog("Upload Failed", "Failed to upload the mod build to the server.\n" + e.displayMessage, "Close");

                    isAwaitingServerResponse = false;
                    //Repaint();

                };

                ModManager.UploadModBinaryDirectory(profile.modId, buildProfile, exportPath, true, 
                    mf => NotifySubmissionSucceeded(updatedProfile.name, updatedProfile.profileURL), onSubmissionFailed);

            } 
            else 
            {
                NotifySubmissionSucceeded(updatedProfile.name, updatedProfile.profileURL);
            }
        }

        private static void NotifySubmissionSucceeded(string modName, string modProfileURL) 
        {
            EditorUtility.DisplayDialog("Submission Successful", modName + " was successfully updated on the server." 
                                                                         + "\nView the changes here: " + modProfileURL, "Close");
            
            isAwaitingServerResponse = false;
        }

        */

    /*
        public static void OpenFolderAfterModsBuild()
        {
            EditorUtility.RevealInFinder(DemiModBase.exportPath);
        }
        
     
        public static void ExportSettings() 
        {
            //if (GUILayout.Button("Open Export Folder"))
                //EditorUtility.RevealInFinder(basePath + "/");

            EditorGUIUtility.labelWidth = 200;
            openAfterExport = EditorGUILayout.Toggle("Open Export Folder On Complete", openAfterExport);
            EditorPrefs.SetBool("OpenAfterExport", openAfterExport);

            GUILayout.Space(10);
            GUILayout.Label("Export path: " + exportPath, EditorStyles.label);
        }
      */

    public static Transform FindChildRecursive(this Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name.Contains(name))
                return child;

            var result = child.FindChildRecursive(name);
            if (result != null)
                return result;
        }
        return null;
    }


    
    
    public static DataHolder GetDataHolder()
    {
        if (!DataHolder)
        {
            DataHolder = Resources.Load<DataHolder>("DataHolder");
        }

        return DataHolder;
    }
    
    
    public static string FormatPath(string path)
    {
        return path.Replace(" ", "").Replace(@"\", "/");
    }
    public static string FormatPathKeepSpaces(string path)
    {
        return path.Replace(@"\", "/");
    }
    public static string FormatForFileExplorer(string path)
    {
        return path.Replace("/", @"\");
    }
    
    
    public static void AddLineAndSpace()
    {
        GUILayout.Space(10);
        DrawUILine(Color.blue);
        GUILayout.Space(10);
    }
    
    public static void DrawUILine(Color color, int thickness = 2, int padding = 10) 
    {
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
        r.height = thickness;
        r.y += padding / 2;
        r.x -= 2;
        r.width += 6;
        EditorGUI.DrawRect(r, color);
    }
    
    
    public static void DisplayError(string title, string error) 
    {
        EditorUtility.DisplayDialog(title, error, "Ok", "");
    }

    public static bool DisplayWarning(string title, string warning) 
    {
        return EditorUtility.DisplayDialog(title, warning, "Continue", "Cancel");
    }
    
    private static void DeleteFolder(string path) 
    {
        if (!Directory.Exists(path))
            return;
        FileUtil.DeleteFileOrDirectory(path);
    }

    private static void CreateFolder(string path) 
    {
        Directory.CreateDirectory(path);
    }
    
    
    public enum ModType
    {
        Avatar,
        Map,
        Enemy
    }
}


#endif