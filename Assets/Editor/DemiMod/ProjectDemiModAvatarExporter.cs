#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;


public class ProjectDemiModAvatarExporter : EditorWindow
{
    public DataHolder dataHolder;
    
    public HandPoseCopier handPoseCopierScript;
    public WeblineRenderer weblineRendererScript;
    
    // Demi-Mod Variables
    private GameObject avatarModel;
    public Animator animator;
    public PlayerAvatar playerAvatarScript;
    public string avatarNameString = "";
    
    
    public bool FolderSetupComplete = false;
    public bool AvatarSetupComplete = false;
    public bool CustomMaterialSettingsComplete = false;
    public bool CustomHandPoseSettingsComplete = false;
    
    
    public BuildTarget buildTarget = BuildTarget.StandaloneWindows64;
    
    
    float vSbarValue;
    public Vector2 scrollPosition = Vector2.zero;
    
    private GameObject finalPrefab;
    
    
    [MenuItem("Project Demigod/Avatar Mod Exporter")]
    public static void ShowMapWindow() 
    {
        GetWindow<ProjectDemiModAvatarExporter>("Avatar Mod Exporter");
    }
    
    private void Awake() 
    {
        if(buildTarget == BuildTarget.NoTarget)
            buildTarget = BuildTarget.StandaloneWindows64;

        GetDataHolder();
    }

    public void GetDataHolder()
    {
        if(!dataHolder)
            dataHolder = Resources.Load<DataHolder>("DataHolder");
    }
    
    public void SetDefaultModLocation()
    {
        if (dataHolder)
        {
            dataHolder.userDefinedModsLocation = EditorUtility.OpenFolderPanel("Select Directory", "", "");
        }
    }
    
    
    private void OnGUI()
    {
        EditorGUIUtility.labelWidth = 80;
        GUILayout.Label("Project Demigod Mod Exporter", EditorStyles.largeLabel);
        GUILayout.Space(10);
        
        
        GUILayoutOption[] options = { GUILayout.MaxWidth(1000), GUILayout.MinWidth(250) };
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, options);

        if(!dataHolder)
            GetDataHolder();
        
        EditorGUILayout.HelpBox("Choose where you would like mods to be stored.", MessageType.Info);
        if(GUILayout.Button("Select Mod Location", GUILayout.Height(20)))
        {
            if (dataHolder)
            {
                SetDefaultModLocation();
            }
        }
        
        if(dataHolder)
        {
            if (string.IsNullOrEmpty(dataHolder.userDefinedModsLocation))
            {
                GUI.color = Color.red;
                EditorGUILayout.HelpBox("No location chosen.", MessageType.Info);
            }
            else
            {
                GUI.color = Color.green;
                EditorGUILayout.HelpBox("Current Location: " + dataHolder.userDefinedModsLocation, MessageType.Info);
            }
        }
        
        GUI.color = Color.white;
        
        
        avatarModel = EditorGUILayout.ObjectField("Avatar Model", avatarModel, typeof(GameObject), true) as GameObject;
        //playerAvatarScript = EditorGUILayout.ObjectField("Player Avatar Script", playerAvatarScript, typeof(PlayerAvatar), true) as PlayerAvatar;

        if (avatarModel == null) 
        {
            EditorGUILayout.HelpBox("Drag the avatar model here to continue.", MessageType.Info);
        } 
        else if (avatarModel) 
        {
            EditorGUILayout.HelpBox(avatarModel.name + " will be tested for correct settings.", MessageType.Info);
        } 
        
        
        DemiModBase.AddLineAndSpace();
        
        
        #region SwitchPlatforms
        
        EditorGUILayout.HelpBox("Current Target: " + EditorUserBuildSettings.selectedStandaloneTarget.ToString(), MessageType.Info);
        
        GUILayout.BeginHorizontal("Switch Platforms", GUI.skin.window);

        using (new EditorGUI.DisabledScope(EditorUserBuildSettings.selectedStandaloneTarget == BuildTarget.StandaloneWindows64))
        {
            //EditorGUILayout.HelpBox("Current Target: Android", MessageType.Info);
            if(GUILayout.Button("Switch to Windows"))
            {
                DemiModBase.SwitchToWindows();
            }
        }

        using (new EditorGUI.DisabledScope(EditorUserBuildSettings.selectedStandaloneTarget == BuildTarget.Android))
        {
            //EditorGUILayout.HelpBox("Current Target: Windows", MessageType.Info);
            if(GUILayout.Button("Switch to Android"))
            {
                DemiModBase.SwitchToAndroid();
            }
        }

        GUILayout.EndHorizontal();

        #endregion
        
        
        DemiModBase.AddLineAndSpace();


        using (new EditorGUI.DisabledScope(avatarModel == null))
        {
            GUI.color = Color.white;
            
            DemiModBase.AddLineAndSpace();
            
            
            EditorGUILayout.HelpBox("Use this button first to get all references and add necessary scripts.", MessageType.Info);
            
            AvatarSetupComplete = playerAvatarScript != null && animator != null && animator.avatar != null && animator.avatar.isHuman;
            
            
            // Colors
            if (AvatarSetupComplete)
            {
                GUI.color = Color.green;
            }
            else
            {
                GUI.color = Color.white;
            }
            
            if (GUILayout.Button("Setup Avatar", GUILayout.Height(20)))
            {
                Debug.Log("Checking model: " + avatarModel.name);
                
                GetDataHolder();
                dataHolder.lastPlayerAvatarName = avatarModel.name;
                
                if (!avatarModel.GetComponentInChildren<PlayerAvatar>())
                {
                    playerAvatarScript = avatarModel.AddComponent<PlayerAvatar>();
                }
                
                playerAvatarScript = avatarModel.GetComponentInChildren<PlayerAvatar>();
                
                avatarNameString = avatarModel.name;

                if (avatarNameString.Contains("Player Avatar - ") == false)
                {
                    avatarNameString = "Avatar Mod - " + avatarNameString;
                }
                
                animator = avatarModel.GetComponent<Animator>();

                if(animator == null)
                {
                    Debug.LogError("Animator not found. Adding Animator component.");
                    avatarModel.AddComponent<Animator>();
                    return;
                }
                else
                {
                    Debug.Log("Animator found");
                }
                
                if(animator.avatar == null)
                {
                    Debug.LogError("Avatar not found");
                    return;
                }
                else
                {
                    Debug.Log("Animator's Avatar found");
                }
                
                if (!animator.avatar.isHuman)
                {
                    Debug.Log("Avatar Model is not humanoid! Please convert rig to humanoid in the inspector first.");
                    EditorGUILayout.HelpBox("Avatar Model is not humanoid. Please convert rig to humanoid in the inspector first.", MessageType.Warning);
                    return;
                }
                
                if (avatarModel)
                {
                    animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Avatar Hand Animator");
                }
                
                if (!handPoseCopierScript)
                {
                    handPoseCopierScript = GameObject.Find("Hand Pose Copier").GetComponent<HandPoseCopier>();
                }
                
                if(!weblineRendererScript)
                {
                    weblineRendererScript = GameObject.Find("Hand Pose Copier").GetComponent<WeblineRenderer>();
                }

                if(handPoseCopierScript)
                {
                    GetDataHolder();
                    dataHolder.handPoseCopierGameObject = handPoseCopierScript.gameObject;
                    
                    if (animator)
                        handPoseCopierScript.avatarAnimator = animator;

                    if (playerAvatarScript)
                        handPoseCopierScript.playerAvatarScript = playerAvatarScript;
                    
                    //Debug.Log("Avatar's mod folder path in project is: " + Path.Combine(DemiModBase.unityAssetsAvatarModsFolderPath, playerAvatarScript.gameObject.name));
                    
                    handPoseCopierScript.avatarModFolderPath = Path.Combine(Path.Combine(DemiModBase.unityAssetsAvatarModsFolderPath, playerAvatarScript.gameObject.name), "AvatarModHandPoses.json");
                    
                    dataHolder.handPoseBuildLocation = handPoseCopierScript.avatarModFolderPath;
                    
                    Debug.Log("Hand Pose Script should now be in Path: " + handPoseCopierScript.avatarModFolderPath);
                }

                if (weblineRendererScript)
                {
                    weblineRendererScript.playerAvatar = playerAvatarScript;
                }
                
                playerAvatarScript.animator = animator;
                
                MapAvatarBody();
                
                SetupHealthEnergyReadout();
                
                
                if(playerAvatarScript && playerAvatarScript.gameObject)
                {
                    DemiModBase.GetOrCreateModPath(DemiModBase.ModType.Avatar, playerAvatarScript.gameObject.name);
                    
                    finalPrefab = PrefabUtility.SaveAsPrefabAssetAndConnect(playerAvatarScript.gameObject,
                        DemiModBase.GetOrCreateModPath(DemiModBase.ModType.Avatar, avatarNameString) + ".prefab", InteractionMode.UserAction);

                    if (finalPrefab)
                    {
                        dataHolder.lastPlayerAvatarPrefab = finalPrefab;
                    }
                }
                
                AvatarSetupComplete = true;
            }
            
            
            // Colors
            if (FolderSetupComplete)
            {
                GUI.color = Color.green;
            }
            else
            {
                GUI.color = Color.white;
            }
            
            
            DemiModBase.AddLineAndSpace();
            
            
            GUI.color = Color.white;
            //EditorGUILayout.HelpBox("Warning. These buttons will clear all material settings and any changes you made.", MessageType.Warning);
            EditorGUILayout.HelpBox("Collect all data about the Avatar's renderers and materials, so users can customize them in game. Warning. " +
                                    "These buttons will clear all material settings and any changes you made.", MessageType.Info);
            
            GUILayout.BeginHorizontal("Material Settings", GUI.skin.window);

            // Colors
            if (CustomMaterialSettingsComplete)
            {
                GUI.color = Color.green;
            }
            else
            {
                GUI.color = Color.white;
            }
            
            if (GUILayout.Button("Create Custom Material Settings", GUILayout.Height(20)))
            {
                GenerateCustomMaterialSettings();
            }

            GUI.color = Color.red;
            if (GUILayout.Button("Clear Custom Material Settings", GUILayout.Height(20)))
            {
                ClearCustomMaterialSettings();
            }
            
            GUILayout.EndHorizontal();
            
            GUI.color = Color.white;
            
            
            DemiModBase.AddLineAndSpace();
            
            
            EditorGUILayout.HelpBox("Create Hand Poses in Play Mode.", MessageType.Info);
            
            // Start Hand Pose Process in Play Mode
            if (CustomHandPoseSettingsComplete)
            {
                GUI.color = Color.green;
            }
            else
            {
                GUI.color = Color.white;
            }

            if (Application.isPlaying == false)
            {
                if (GUILayout.Button("Create Hand Poses", GUILayout.Height(20)))
                {
                    EditorApplication.EnterPlaymode();
                }
            }
        }
        
        
        DemiModBase.AddLineAndSpace();
        
        if(!finalPrefab && dataHolder.lastPlayerAvatarPrefab)
        {
            finalPrefab = dataHolder.lastPlayerAvatarPrefab;
        }
        
            
        using (new EditorGUI.DisabledScope(finalPrefab == null))
        {
            finalPrefab = EditorGUILayout.ObjectField("Final Prefab", finalPrefab, typeof(GameObject), true) as GameObject;
        }
        
        
        DemiModBase.AddLineAndSpace();
        
        
        #region Build Addressables
        
        bool canBuild = avatarModel != null;
        
        using (new EditorGUI.DisabledScope(!canBuild)) 
        {
            GUILayout.BeginHorizontal("Build the Mods", GUI.skin.window);
            if (GUILayout.Button("Build for Windows (PCVR)", GUILayout.Height(20)))
            {
                DisableDebugRenderers();
                
                // Save avatar before building addressable.
                Debug.Log("Saving Avatar Prefab: " + avatarModel.name);
                PrefabUtility.ApplyPrefabInstance(avatarModel, InteractionMode.UserAction);
                
                GetDataHolder();
                //dataHolder.lastAddressableBuildPath = DemiModBase.modsFolderPath + "/" + avatarModel.name + " - PCVR";
                dataHolder.lastPlayerAvatarPrefab = finalPrefab;
                dataHolder.lastPlayerAvatarName = playerAvatarScript.gameObject.name;
                
                AssetDatabase.Refresh();
                EditorUtility.SetDirty(dataHolder);
                AssetDatabase.SaveAssets();
                
                DemiModBase.ExportWindows(DemiModBase.ModType.Avatar, avatarModel);
                
                PostBuildCleanup();
                
                EditorApplication.delayCall += () =>
                {
                    OpenFolderAfterModsBuild();
                };
            }

            if (GUILayout.Button("Build for Android (Quest)", GUILayout.Height(20)))
            {
                DisableDebugRenderers();
                
                // Save avatar before building addressable.
                Debug.Log("Saving Avatar Prefab");
                PrefabUtility.ApplyPrefabInstance(avatarModel, InteractionMode.UserAction);
                
                GetDataHolder();
                //dataHolder.lastAddressableBuildPath = DemiModBase.modsFolderPath + "/" + avatarModel.name + " - Android";
                dataHolder.lastPlayerAvatarPrefab = finalPrefab;
                dataHolder.lastPlayerAvatarName = playerAvatarScript.gameObject.name;
                
                AssetDatabase.Refresh();
                EditorUtility.SetDirty(dataHolder);
                AssetDatabase.SaveAssets();
                
                DemiModBase.ExportAndroid(DemiModBase.ModType.Avatar, avatarModel);
                
                PostBuildCleanup();
                
                EditorApplication.delayCall += () =>
                {
                    OpenFolderAfterModsBuild();
                };
            }
            
            GUILayout.EndHorizontal();
        }
        
        
        DemiModBase.AddLineAndSpace();

        
        #endregion
        
        
        #region Finish Setup
        
        using(new EditorGUI.DisabledScope(avatarModel == null))
        {
            EditorGUILayout.HelpBox(" Use this button to Finish Setup for current Avatar AFTER building the Addressable. Adds the Hand Pose JSON to the folder before compression.", MessageType.Info);
            if (GUILayout.Button("Finish Setup", GUILayout.Height(20)))
            {
                GetDataHolder();
                
                if (!playerAvatarScript)
                {
                    if(avatarModel)
                        playerAvatarScript = avatarModel.GetComponentInChildren<PlayerAvatar>();
                }
                
                if(string.IsNullOrEmpty(dataHolder.handPoseBuildLocation) == false)
                {
                    if(true)// File.Exists(dataHolder.handPoseBuildLocation) && Directory.Exists(dataHolder.lastAddressableBuildPath))
                    {
                        Debug.Log("Attempting to Copy file: " + dataHolder.handPoseBuildLocation + " to " 
                                  + dataHolder.lastAddressableBuildPath + "/AvatarModHandPoses.json");
                        
                        //File.Copy(dataHolder.handPoseBuildLocation, dataHolder.lastAddressableBuildPath);
                        
                        FileUtil.CopyFileOrDirectory(dataHolder.handPoseBuildLocation,
                            Path.Combine(dataHolder.lastAddressableBuildPath + "/AvatarModHandPoses.json"));
                        
                        //File.Move(dataHolder.handPoseBuildLocation, Path.Combine(dataHolder.lastAddressableBuildPath, "AvatarModHandPoses.json"));
                    }
                    else
                    {
                        Debug.LogError("Hand Pose JSON file not found. Please create Hand Poses first.");
                    }
                }
                else
                {
                    Debug.LogError("Hand Pose JSON file not found. Please create Hand Poses first.");
                }
            }
        }
        
        #endregion
        
        
        DemiModBase.AddLineAndSpace();
        
        
        // Save Avatar Prefab button
        using (new EditorGUI.DisabledScope(avatarModel == null))
        {
            if (GUILayout.Button("Save Avatar Prefab", GUILayout.Height(20)))
            {
                if (avatarModel)
                {
                    if(PrefabUtility.IsPartOfRegularPrefab(avatarModel))
                    {
                        Debug.Log("Saving Avatar Prefab");
                        PrefabUtility.ApplyPrefabInstance(avatarModel, InteractionMode.UserAction);
                    }
                    else
                    {
                        Debug.Log("Saving Avatar Prefab");
                        PrefabUtility.SaveAsPrefabAssetAndConnect(avatarModel, DemiModBase.GetOrCreateModPath(DemiModBase.ModType.Avatar, avatarModel.name) + ".prefab", InteractionMode.UserAction);
                    }
                }
            }
        }

        
        DemiModBase.AddLineAndSpace();

        
        #region Debug Shapes
        
        GUI.color = Color.white;
        using (new EditorGUI.DisabledScope(avatarModel == null))
        {
            GUILayout.BeginHorizontal("Debug Shapes", GUI.skin.window);
            
            if (GUILayout.Button("Enable All Debug Shapes", GUILayout.Height(20)))
            {
                // Turn on FingerTip and Palm Mesh Renderers.
                if (playerAvatarScript)
                {
                    EnableDebugRenderers();
                }
            }
            
            if (GUILayout.Button("Disable All Debug Shapes", GUILayout.Height(20)))
            {
                // Turn off FingerTip and Palm Mesh Renderers.
                if (playerAvatarScript)
                {
                    DisableDebugRenderers();
                }
            }
            
            GUILayout.EndHorizontal();
            
            
            GUILayout.BeginHorizontal( GUI.skin.window);
            

            if (GUILayout.Button("Enable All Hand Shapes", GUILayout.Height(20)))
            {
                // Turn on FingerTip and Palm Mesh Renderers.
                if (playerAvatarScript)
                {
                    EnableHandShapes();
                }
            }


            if (GUILayout.Button("Disable All Hand Shapes", GUILayout.Height(20)))
            {
                // Turn off FingerTip and Palm Mesh Renderers.
                if (playerAvatarScript)
                {
                    DisableHandShapes();
                }
            }
            
            GUILayout.EndHorizontal();
            
            
            GUILayout.BeginHorizontal(GUI.skin.window);
            

            if (GUILayout.Button("Enable Web Shapes", GUILayout.Height(20)))
            {
                // Turn on FingerTip and Palm Mesh Renderers.
                if (playerAvatarScript)
                {
                    EnableWebShapes();
                }
            }


            if (GUILayout.Button("Disable Web Shapes", GUILayout.Height(20)))
            {
                // Turn off FingerTip and Palm Mesh Renderers.
                if (playerAvatarScript)
                {
                    DisableWebShapes();
                }
            }
            
            GUILayout.EndHorizontal();
        }
        
        
        
        #endregion


        #region Clear Data
        
        EditorGUILayout.HelpBox("Reset all data for this Mod Exporter Tab.", MessageType.Info);
        GUI.color = Color.red;
        if (GUILayout.Button("Clear Mod Exporter", GUILayout.Height(20)))
        {
            ResetButtonCompletionStatus();
        }
        
        GUI.color = Color.white;
        
        #endregion
        
        
        DemiModBase.AddLineAndSpace();

        
        // Open Addressable Export Folder button
        if (GUILayout.Button("Open Addressable Export Folder", GUILayout.Height(20)))
        {
            EditorApplication.delayCall += () =>
            {
                OpenFolderAfterModsBuild();
            };
        }
        
        // End the scroll view that we began above.
        EditorGUILayout.EndScrollView();
    }
    
    
    
    public void PostBuildCleanup()
    {
        GetDataHolder();
        
        if(!finalPrefab && dataHolder.lastPlayerAvatarPrefab)
        {
            finalPrefab = dataHolder.lastPlayerAvatarPrefab;
        }
        
        
        if(!avatarModel)
        {
            if (string.IsNullOrEmpty(dataHolder.lastPlayerAvatarName) == false)
            {
                if (GameObject.Find(dataHolder.lastPlayerAvatarName))
                {
                    avatarModel = GameObject.Find(dataHolder.lastPlayerAvatarName);
                }
            }
        }

        
        if (avatarModel)
        {
            Debug.Log("Avatar Model Found: " + avatarModel.name);
        }
    }
    

    private void ResetButtonCompletionStatus()
    {
        AvatarSetupComplete = false;
        CustomMaterialSettingsComplete = false;
        FolderSetupComplete = false;
        CustomHandPoseSettingsComplete = false;
        
        avatarModel = null;
        animator = null;
        playerAvatarScript = null;
        avatarNameString = "";
        
        if(!dataHolder)
            GetDataHolder();
        
        dataHolder.lastPlayerAvatarPrefab = null;
        dataHolder.lastPlayerAvatarName = "";
        
    }

    
    
    private void MapAvatarBody()
    {
        GetAllHumanBoneReferences();

        SetupPalmsAndSpawnPoints();
        
        SetFingerReferences();
    }


    private void GetAllHumanBoneReferences()
    {
        if (playerAvatarScript.avatarHead == null)
        {
            playerAvatarScript.avatarHead = animator.GetBoneTransform(HumanBodyBones.Head);
        }
        
        if(playerAvatarScript.avatarEyes == null)
        {
            if (animator.GetBoneTransform(HumanBodyBones.Head).FindChildRecursive("Eyes Debug Capsule"))
            {
                playerAvatarScript.avatarEyes = animator.GetBoneTransform(HumanBodyBones.Head).FindChildRecursive("Eyes Debug Capsule");
            }
            else
            {
                
                Debug.Log("No Eye Bones Found. Eyes Debug Shape created. Please move the Eyes Debug Shape to the eyes position.");
                
                GameObject eyes = Instantiate(Resources.Load("Eyes Debug Capsule", typeof(GameObject))) as GameObject;
                eyes.transform.SetParent(playerAvatarScript.avatarHead, true);
                
                playerAvatarScript.avatarEyes = eyes.transform;
                
                /*
                if (animator.GetBoneTransform(HumanBodyBones.LeftEye) != null && animator.GetBoneTransform(HumanBodyBones.RightEye) != null)
                {
                    // Create a new gameobject to hold the eyes (so we can rotate them
                    GameObject eyes = new GameObject("Eyes");
                    eyes.transform.parent = playerAvatarScript.avatarHead;

                    Transform leftEye = animator.GetBoneTransform(HumanBodyBones.LeftEye);
                    Transform rightEye = animator.GetBoneTransform(HumanBodyBones.RightEye);

                    eyes.transform.position = (leftEye.position + rightEye.position) / 2;
                    eyes.transform.rotation = Quaternion.LookRotation(Vector3.forward);
                
                    playerAvatarScript.avatarEyes = eyes.transform;
                }
                else
                {
                    Debug.Log("No Eye Bones Found. Eyes Debug Shape created. Please the Eyes Debug Shape to the eyes position.");
                
                    GameObject eyes = Instantiate(Resources.Load("Eyes Debug Capsule", typeof(GameObject))) as GameObject;
                    eyes.transform.SetParent(playerAvatarScript.avatarHead, true);
                
                    playerAvatarScript.avatarEyes = eyes.transform;
                }
                */
            }
        }
                
        if(playerAvatarScript.leftHand == null)
        {
            playerAvatarScript.leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        }
                
        if(playerAvatarScript.rightHand == null)
        {
            playerAvatarScript.rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        }
                
        if(playerAvatarScript.leftForearm == null)
        {
            playerAvatarScript.leftForearm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        }
                
        if(playerAvatarScript.rightForearm == null)
        {
            playerAvatarScript.rightForearm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
        }
        
        if(playerAvatarScript.leftForearmTwist == null)
        {
            if (playerAvatarScript.leftForearm.parent)
            {
                foreach (Transform forearmChild in playerAvatarScript.leftForearm.parent)
                {
                    if(forearmChild.name.Contains("Twist") || forearmChild.name.Contains("twist"))
                        playerAvatarScript.leftForearmTwist = forearmChild;
                }

                if (playerAvatarScript.leftForearmTwist == null)
                {
                    foreach (Transform forearmChild in playerAvatarScript.leftForearm)
                    {
                        if(forearmChild.name.Contains("Twist") || forearmChild.name.Contains("twist"))
                            playerAvatarScript.leftForearmTwist = forearmChild;
                    }
                }
            }
        }
        
        if(playerAvatarScript.rightForearmTwist == null)
        {
            if (playerAvatarScript.rightForearm.parent)
            {
                foreach (Transform forearmChild in playerAvatarScript.rightForearm.parent)
                {
                    if(forearmChild.name.Contains("Twist") || forearmChild.name.Contains("twist"))
                        playerAvatarScript.rightForearmTwist = forearmChild;
                }
                
                if(playerAvatarScript.rightForearmTwist == null)
                {
                    foreach (Transform forearmChild in playerAvatarScript.rightForearm)
                    {
                        if(forearmChild.name.Contains("Twist") || forearmChild.name.Contains("twist"))
                            playerAvatarScript.rightForearmTwist = forearmChild;
                    }
                }
            }
        }
        
        
        // Fingers
        playerAvatarScript.leftIndexRoot = animator.GetBoneTransform(HumanBodyBones.LeftIndexProximal);
        playerAvatarScript.leftIndexIntermediate = animator.GetBoneTransform(HumanBodyBones.LeftIndexIntermediate);
        playerAvatarScript.leftIndexEnd = animator.GetBoneTransform(HumanBodyBones.LeftIndexDistal);
        
        playerAvatarScript.leftMiddleRoot = animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
        playerAvatarScript.leftMiddleIntermediate = animator.GetBoneTransform(HumanBodyBones.LeftMiddleIntermediate);
        playerAvatarScript.leftMiddleEnd = animator.GetBoneTransform(HumanBodyBones.LeftMiddleDistal);
        
        playerAvatarScript.leftRingRoot = animator.GetBoneTransform(HumanBodyBones.LeftRingProximal);
        playerAvatarScript.leftRingIntermediate = animator.GetBoneTransform(HumanBodyBones.LeftRingIntermediate);
        playerAvatarScript.leftRingEnd = animator.GetBoneTransform(HumanBodyBones.LeftRingDistal);
        
        playerAvatarScript.leftPinkyRoot = animator.GetBoneTransform(HumanBodyBones.LeftLittleProximal);
        playerAvatarScript.leftPinkyIntermediate = animator.GetBoneTransform(HumanBodyBones.LeftLittleIntermediate);
        playerAvatarScript.leftPinkyEnd = animator.GetBoneTransform(HumanBodyBones.LeftLittleDistal);
        
        playerAvatarScript.leftThumbRoot = animator.GetBoneTransform(HumanBodyBones.LeftThumbProximal);
        playerAvatarScript.leftThumbIntermediate = animator.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate);
        playerAvatarScript.leftThumbEnd = animator.GetBoneTransform(HumanBodyBones.LeftThumbDistal);
        
        
        playerAvatarScript.rightIndexRoot = animator.GetBoneTransform(HumanBodyBones.RightIndexProximal);
        playerAvatarScript.rightIndexIntermediate = animator.GetBoneTransform(HumanBodyBones.RightIndexIntermediate);
        playerAvatarScript.rightIndexEnd = animator.GetBoneTransform(HumanBodyBones.RightIndexDistal);
        
        playerAvatarScript.rightMiddleRoot = animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
        playerAvatarScript.rightMiddleIntermediate = animator.GetBoneTransform(HumanBodyBones.RightMiddleIntermediate);
        playerAvatarScript.rightMiddleEnd = animator.GetBoneTransform(HumanBodyBones.RightMiddleDistal);
        
        playerAvatarScript.rightRingRoot = animator.GetBoneTransform(HumanBodyBones.RightRingProximal);
        playerAvatarScript.rightRingIntermediate = animator.GetBoneTransform(HumanBodyBones.RightRingIntermediate);
        playerAvatarScript.rightRingEnd = animator.GetBoneTransform(HumanBodyBones.RightRingDistal);
        
        playerAvatarScript.rightPinkyRoot = animator.GetBoneTransform(HumanBodyBones.RightLittleProximal);
        playerAvatarScript.rightPinkyIntermediate = animator.GetBoneTransform(HumanBodyBones.RightLittleIntermediate);
        playerAvatarScript.rightPinkyEnd = animator.GetBoneTransform(HumanBodyBones.RightLittleDistal);
        
        playerAvatarScript.rightThumbRoot = animator.GetBoneTransform(HumanBodyBones.RightThumbProximal);
        playerAvatarScript.rightThumbIntermediate = animator.GetBoneTransform(HumanBodyBones.RightThumbIntermediate);
        playerAvatarScript.rightThumbEnd = animator.GetBoneTransform(HumanBodyBones.RightThumbDistal);
    }
    

    public float webGrabDistanceAdjustment = -0.04f;
    private void SetupPalmsAndSpawnPoints()
    {
        // Left Hand
        // Add Palm Transform if not already created.
        GameObject existingPalmLeft = null;
        Transform leftPalmTransform;
        bool foundPalmLeft = false;

        Vector3 leftHandToPointer = Vector3.zero;
        Vector3 leftHandToMiddle = Vector3.zero;
        Vector3 leftHandToRing = Vector3.zero;
        Vector3 leftHandToPinky = Vector3.zero;

        Vector3 leftPalmForward;
        Vector3 leftPalmUpward;
        
        foreach (Transform handChild in playerAvatarScript.leftHand)
        {
            if (handChild.name.Contains("Avatar Left Palm Spawnpoints Prefab"))
            {
                foundPalmLeft = true;
                existingPalmLeft = handChild.gameObject;
                break;
            }
            else
            {
                existingPalmLeft = null;
            }
        }
        
        // Find the approximate position & rotation of the avatar palm to make it easier for modders.
        // Use the cross product of the hand to the pointer finger and the hand to the middle finger to get the palm forward direction.
        leftHandToPointer = animator.GetBoneTransform(HumanBodyBones.LeftIndexProximal).position - animator.GetBoneTransform(HumanBodyBones.LeftHand).position;
        leftHandToMiddle = animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal).position - animator.GetBoneTransform(HumanBodyBones.LeftHand).position;
        leftHandToRing = animator.GetBoneTransform(HumanBodyBones.LeftRingProximal).position - animator.GetBoneTransform(HumanBodyBones.LeftHand).position;
        leftHandToPinky = animator.GetBoneTransform(HumanBodyBones.LeftLittleProximal).position - animator.GetBoneTransform(HumanBodyBones.LeftHand).position;
        
        if (!foundPalmLeft)
        {
            //var tip = new GameObject("Tip");

            //var Palm = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube));
            //Palm.name = "Palm";
            
            GameObject Palm = Instantiate(Resources.Load("Avatar Left Palm Spawnpoints Prefab", typeof(GameObject))) as GameObject;
            
            leftPalmTransform = Palm.transform;
            leftPalmTransform.parent = playerAvatarScript.leftHand;
            leftPalmTransform.localPosition = Vector3.zero;
            leftPalmTransform.localRotation = Quaternion.identity;
 
            
            
            // Rotate the forward direction to the cross of the hand to the pointer and hand to the middle,
            // and rotate the upward direction to
            leftPalmForward = Vector3.Cross(leftHandToPointer, leftHandToMiddle);
            leftPalmUpward = Vector3.Cross(leftHandToMiddle, leftPalmForward);
            
            leftPalmTransform.rotation = Quaternion.LookRotation(leftPalmForward, leftPalmUpward);

            Vector3 middleOfPalm = (animator.GetBoneTransform(HumanBodyBones.LeftHand).position + animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal).position) / 2;
            leftPalmTransform.position = middleOfPalm + leftPalmTransform.forward * 0.03f;
            
            leftPalmTransform.position -= leftPalmTransform.right * 0.02f;
            
            
            if (handPoseCopierScript)
            {
                handPoseCopierScript.leftHandWeaponShapes.Clear();
                
                foreach (Transform childTransform in Palm.transform)
                {
                    if(childTransform.name == "Sphere" || childTransform.name.Contains("Palm Shape"))
                        continue;
                    
                    handPoseCopierScript.leftHandWeaponShapes.Add(childTransform.gameObject);
                }
            }
        }
        else
        {
            
            leftPalmTransform = existingPalmLeft.transform;
            //leftPalmTransform.localScale = Vector3.one * 0.02f;
        }
        
        playerAvatarScript.leftPalm = leftPalmTransform;
        playerAvatarScript.leftHandSpawnPointParent = leftPalmTransform;

        
        // Web Grab Hand Positions
        if (playerAvatarScript.leftWebGrabHandPositionLower)
        {
            
        }
        else
        {
            var webPositionLower = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            webPositionLower.name = "Left Web Position Lower";
            webPositionLower.GetComponent<SphereCollider>().enabled = false;
            webPositionLower.transform.localScale = Vector3.one * 0.02f;
            
            webPositionLower.transform.SetParent(animator.GetBoneTransform(HumanBodyBones.LeftHand));
            
            playerAvatarScript.leftWebGrabHandPositionLower = webPositionLower.transform;
            
            playerAvatarScript.leftWebGrabHandPositionLower.position = animator.GetBoneTransform(HumanBodyBones.LeftLittleProximal).position + (leftPalmTransform.forward * 0.03f);
            playerAvatarScript.leftWebGrabHandPositionLower.position += leftHandToPinky.normalized * -0.04f;;
        }
        
        if (playerAvatarScript.leftWebGrabHandPositionUpper)
        {
            
        }
        else
        {
            var webPositionUpper = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            webPositionUpper.name = "Left Web Position Upper";
            webPositionUpper.GetComponent<SphereCollider>().enabled = false;
            webPositionUpper.transform.localScale = Vector3.one * 0.02f;
            
            webPositionUpper.transform.SetParent(animator.GetBoneTransform(HumanBodyBones.LeftHand));
            
            playerAvatarScript.leftWebGrabHandPositionUpper = webPositionUpper.transform;
            
            playerAvatarScript.leftWebGrabHandPositionUpper.position = animator.GetBoneTransform(HumanBodyBones.LeftIndexProximal).position + (leftPalmTransform.forward * 0.03f);
            playerAvatarScript.leftWebGrabHandPositionUpper.position += leftHandToPointer.normalized * -0.04f;;
        }
        
        
        
        // Right Hand
        // Add Palm Transform if not already created.
        GameObject existingPalmRight = null;
        Transform rightPalmTransform;
        bool foundPalmRight = false;

        foreach (Transform handChild in playerAvatarScript.rightHand)
        {
            if (handChild.name.Contains("Avatar Right Palm Spawnpoints Prefab"))
            {
                foundPalmRight = true;
                existingPalmRight = handChild.gameObject;
                break;
            }
            else
            {
                existingPalmRight = null;
            }
        }

        Vector3 rightHandToPointer = Vector3.zero;
        Vector3 rightHandToMiddle = Vector3.zero;
        Vector3 rightHandToRing = Vector3.zero;
        Vector3 rightHandToPinky = Vector3.zero;

        Vector3 rightPalmForward;
        Vector3 rightPalmUpward;
        
        // Find the approximate position & rotation of the avatar palm to make it easier for modders.
        // Use the cross product of the hand to the pointer finger and the hand to the middle finger to get the palm forward direction.
        rightHandToPointer = animator.GetBoneTransform(HumanBodyBones.RightIndexProximal).position - animator.GetBoneTransform(HumanBodyBones.RightHand).position;
        rightHandToMiddle = animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal).position - animator.GetBoneTransform(HumanBodyBones.RightHand).position;
        rightHandToRing = animator.GetBoneTransform(HumanBodyBones.RightRingProximal).position - animator.GetBoneTransform(HumanBodyBones.RightHand).position;
        rightHandToPinky = animator.GetBoneTransform(HumanBodyBones.RightLittleProximal).position - animator.GetBoneTransform(HumanBodyBones.RightHand).position;
        
        if (!foundPalmRight)
        {
            //var tip = new GameObject("Tip");

            //var Palm = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube));
            //Palm.name = "Palm";
            
            GameObject Palm = Instantiate(Resources.Load("Avatar Right Palm Spawnpoints Prefab", typeof(GameObject))) as GameObject;
            
            rightPalmTransform = Palm.transform;
            rightPalmTransform.parent = playerAvatarScript.rightHand;
            rightPalmTransform.localPosition = Vector3.zero;
            rightPalmTransform.localRotation = Quaternion.identity;
            
            
            // Rotate the forward direction to the cross of the hand to the pointer and hand to the middle,
            // and rotate the upward direction to 
            rightPalmForward = Vector3.Cross(rightHandToMiddle, rightHandToPointer);
            rightPalmUpward = Vector3.Cross(rightPalmForward, rightHandToMiddle);
            
            rightPalmTransform.rotation = Quaternion.LookRotation(rightPalmForward, rightPalmUpward);

            Vector3 rightMiddleOfPalm = (animator.GetBoneTransform(HumanBodyBones.RightHand).position + animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal).position) / 2;
            rightPalmTransform.position = rightMiddleOfPalm + rightPalmTransform.forward * 0.03f;
            
            rightPalmTransform.position += rightPalmTransform.right * 0.02f;
            

            if (handPoseCopierScript)
            {
                handPoseCopierScript.rightHandWeaponShapes.Clear();
                
                foreach (Transform childTransform in Palm.transform)
                {
                    if(childTransform.name == "Sphere" || childTransform.name.Contains("Palm Shape"))
                        continue;
                    
                    handPoseCopierScript.rightHandWeaponShapes.Add(childTransform.gameObject);
                }
            }
        }
        else
        {
            rightPalmTransform = existingPalmRight.transform;
            //rightPalmTransform.localScale = Vector3.one * 0.02f;
        }
        
        playerAvatarScript.rightPalm = rightPalmTransform;
        playerAvatarScript.rightHandSpawnPointParent = rightPalmTransform;
        
        // Web Grab Hand Positions
        if (playerAvatarScript.rightWebGrabHandPositionLower)
        {
            
        }
        else
        {
            var webPositionLower = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            webPositionLower.name = "Right Web Position Lower";
            webPositionLower.GetComponent<SphereCollider>().enabled = false;
            webPositionLower.transform.localScale = Vector3.one * 0.02f;
            
            webPositionLower.transform.SetParent(animator.GetBoneTransform(HumanBodyBones.RightHand));
            
            playerAvatarScript.rightWebGrabHandPositionLower = webPositionLower.transform;
            
            playerAvatarScript.rightWebGrabHandPositionLower.position = animator.GetBoneTransform(HumanBodyBones.RightLittleProximal).position + (rightPalmTransform.forward * 0.03f);
            playerAvatarScript.rightWebGrabHandPositionLower.position += rightHandToPinky.normalized * -0.04f;;
        }
        
        if (playerAvatarScript.rightWebGrabHandPositionUpper)
        {
            
        }
        else
        {
            var webPositionUpper = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            webPositionUpper.name = "Right Web Position Upper";
            webPositionUpper.GetComponent<SphereCollider>().enabled = false;
            webPositionUpper.transform.localScale = Vector3.one * 0.02f;
            
            webPositionUpper.transform.SetParent(animator.GetBoneTransform(HumanBodyBones.RightHand));
            
            playerAvatarScript.rightWebGrabHandPositionUpper = webPositionUpper.transform;
            
            playerAvatarScript.rightWebGrabHandPositionUpper.position = animator.GetBoneTransform(HumanBodyBones.RightIndexProximal).position + (rightPalmTransform.forward * 0.03f);
            playerAvatarScript.rightWebGrabHandPositionUpper.position += rightHandToPointer.normalized * -0.04f;
        }
        
        
        if(playerAvatarScript.leftWeblineOriginPoint == null)
        {
            var weblineOrigin = Instantiate(Resources.Load<GameObject>("Webline Origin Point"));
            
            weblineOrigin.transform.SetParent(playerAvatarScript.leftHand);
            
            weblineOrigin.transform.localPosition = Vector3.zero;
            
            Vector3 handToMiddle = animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal).position - animator.GetBoneTransform(HumanBodyBones.LeftHand).position;
            
            // Rotate the weblineOrigin so it points towards handToMiddle.
            weblineOrigin.transform.rotation = Quaternion.LookRotation(handToMiddle);
            
            playerAvatarScript.leftWeblineOriginPoint = weblineOrigin.transform;
        }
        
        if(playerAvatarScript.rightWeblineOriginPoint == null)
        {
            var weblineOrigin = Instantiate(Resources.Load<GameObject>("Webline Origin Point"));
            
            weblineOrigin.transform.SetParent(playerAvatarScript.rightHand);
            
            weblineOrigin.transform.localPosition = Vector3.zero;
            
            Vector3 handToMiddle = animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal).position - animator.GetBoneTransform(HumanBodyBones.RightHand).position;
            
            // Rotate the weblineOrigin so it points towards handToMiddle.
            weblineOrigin.transform.rotation = Quaternion.LookRotation(handToMiddle);
            
            playerAvatarScript.rightWeblineOriginPoint = weblineOrigin.transform;
        }
        
        
        if (weblineRendererScript)
        {
            weblineRendererScript.leftWebGrabHandPositionLower = playerAvatarScript.leftWebGrabHandPositionLower;
            weblineRendererScript.leftWebGrabHandPositionUpper = playerAvatarScript.leftWebGrabHandPositionUpper;
            
            weblineRendererScript.rightWebGrabHandPositionLower = playerAvatarScript.rightWebGrabHandPositionLower;
            weblineRendererScript.rightWebGrabHandPositionUpper = playerAvatarScript.rightWebGrabHandPositionUpper;
            
            weblineRendererScript.leftLineRenderer.enabled = true;
            weblineRendererScript.rightLineRenderer.enabled = true;
            
            weblineRendererScript.leftWeblineOrigin = playerAvatarScript.leftWeblineOriginPoint;
            weblineRendererScript.rightWeblineOrigin = playerAvatarScript.rightWeblineOriginPoint;
            
            weblineRendererScript.SetPoints();
        }
    }
    
    
    private void SetFingerReferences()
    {
        // Need to add FingerTip transforms if not already created.
        for (int i = 0; i < 10; i++)
        {
            var last = playerAvatarScript.leftIndexEnd;
            
            switch (i)
            {
                case 0:
                    if(!playerAvatarScript.leftIndexEnd)
                        continue;
                    
                    last = playerAvatarScript.leftIndexEnd;
                    break;
                
                case 1:
                    if(!playerAvatarScript.leftMiddleEnd)
                        continue;
                    
                    last = playerAvatarScript.leftMiddleEnd;
                    break;
                
                case 2:
                    if(!playerAvatarScript.leftPinkyEnd)
                        continue;
                    
                    last = playerAvatarScript.leftPinkyEnd;
                    break;
                
                case 3:
                    if(!playerAvatarScript.leftRingEnd)
                        continue;
                    
                    last = playerAvatarScript.leftRingEnd;
                    break;
                
                case 4:
                    if(!playerAvatarScript.leftThumbEnd)
                        continue;
                    
                    last = playerAvatarScript.leftThumbEnd;
                    break;
                
                case 5:
                    if(!playerAvatarScript.rightIndexEnd)
                        continue;
                    
                    last = playerAvatarScript.rightIndexEnd;
                    break;
                
                case 6:
                    if(!playerAvatarScript.rightMiddleEnd)
                        continue;
                    
                    last = playerAvatarScript.rightMiddleEnd;
                    break;
                
                case 7:
                    if(!playerAvatarScript.rightPinkyEnd)
                        continue;
                    
                    last = playerAvatarScript.rightPinkyEnd;
                    break;
                
                case 8:
                    if(!playerAvatarScript.rightRingEnd)
                        continue;
                    
                    last = playerAvatarScript.rightRingEnd;
                    break;
                
                case 9:
                    if(!playerAvatarScript.rightThumbEnd)
                        continue;
                    
                    last = playerAvatarScript.rightThumbEnd;
                    break;
            }
            
            var existing = last.Find("FingerTip");
            Transform tipTransform;
            if (!existing)
            {
                //var tip = new GameObject("Tip");

                var tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                tip.name = "FingerTip";
                tip.GetComponent<SphereCollider>().enabled = false;
                tipTransform = tip.transform;
                
                //tipTransform.localPosition = Vector3.zero;
                //tipTransform.localRotation = Quaternion.identity;
                tipTransform.localScale = Vector3.one * 0.02f;

                tip.transform.position = last.position;
                
                tipTransform.SetParent(last);
            }
            else
            {
                tipTransform = existing;
                //tipTransform.localScale = Vector3.one * 0.02f;
            }


            if (playerAvatarScript.fingerTips == null)
                playerAvatarScript.fingerTips = new List<Transform>();
            
            if(playerAvatarScript.fingerTips.Contains(tipTransform) == false)
                playerAvatarScript.fingerTips.Add(tipTransform);

            switch (i)
            {
                case 0:
                    playerAvatarScript.leftIndexTip = tipTransform;
                    break;
                
                case 1:
                    playerAvatarScript.leftMiddleTip = tipTransform;
                    break;
                
                case 2:
                    playerAvatarScript.leftPinkyTip = tipTransform;
                    break;
                
                case 3:
                    playerAvatarScript.leftRingTip = tipTransform;
                    break;
                
                case 4:
                    playerAvatarScript.leftThumbTip = tipTransform;
                    break;
                
                case 5:
                    playerAvatarScript.rightIndexTip = tipTransform;
                    break;
                
                case 6:
                    playerAvatarScript.rightMiddleTip = tipTransform;
                    break;
                
                case 7:
                    playerAvatarScript.rightPinkyTip = tipTransform;
                    break;
                
                case 8:
                    playerAvatarScript.rightRingTip = tipTransform;
                    break;
                
                case 9:
                    playerAvatarScript.rightThumbTip = tipTransform;
                    break;
            }
        }


        // Clear list and re-populate with all finger transforms.
        if(playerAvatarScript.fingerBoneTransforms != null)
            playerAvatarScript.fingerBoneTransforms.Clear();
        else
        {
            playerAvatarScript.fingerBoneTransforms = new List<Transform>();
        }

        if(playerAvatarScript.leftIndexRoot)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.leftIndexRoot);
        
        if(playerAvatarScript.leftIndexIntermediate)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.leftIndexIntermediate);
        
        if(playerAvatarScript.leftIndexEnd)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.leftIndexEnd);
        
        if(playerAvatarScript.leftMiddleRoot)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.leftMiddleRoot);
        
        if(playerAvatarScript.leftMiddleIntermediate)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.leftMiddleIntermediate);
        
        if(playerAvatarScript.leftMiddleEnd)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.leftMiddleEnd);
        
        if(playerAvatarScript.leftPinkyRoot)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.leftPinkyRoot);
        
        if(playerAvatarScript.leftPinkyIntermediate)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.leftPinkyIntermediate);
        
        if(playerAvatarScript.leftPinkyEnd)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.leftPinkyEnd);

        if(playerAvatarScript.leftRingRoot)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.leftRingRoot);
        
        if(playerAvatarScript.leftRingIntermediate)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.leftRingIntermediate);
        
        if(playerAvatarScript.leftRingEnd)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.leftRingEnd);
        
        if(playerAvatarScript.leftThumbRoot)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.leftThumbRoot);
        
        if(playerAvatarScript.leftThumbIntermediate)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.leftThumbIntermediate);
        
        if(playerAvatarScript.leftThumbEnd)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.leftThumbEnd);


        if(playerAvatarScript.rightIndexRoot)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.rightIndexRoot);
        
        if(playerAvatarScript.rightIndexIntermediate)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.rightIndexIntermediate);
        
        if(playerAvatarScript.rightIndexEnd)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.rightIndexEnd);

        if(playerAvatarScript.rightMiddleRoot)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.rightMiddleRoot);
        
        if(playerAvatarScript.rightMiddleIntermediate)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.rightMiddleIntermediate);
        
        if(playerAvatarScript.rightMiddleEnd)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.rightMiddleEnd);
        
        if(playerAvatarScript.rightPinkyRoot)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.rightPinkyRoot);
        
        if(playerAvatarScript.rightPinkyIntermediate)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.rightPinkyIntermediate);
        
        if(playerAvatarScript.rightPinkyEnd)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.rightPinkyEnd);
        
        if(playerAvatarScript.rightRingRoot)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.rightRingRoot);
        
        if(playerAvatarScript.rightRingIntermediate)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.rightRingIntermediate);
        
        if(playerAvatarScript.rightRingEnd)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.rightRingEnd);
        
        if(playerAvatarScript.rightThumbRoot)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.rightThumbRoot);
        
        if(playerAvatarScript.rightThumbIntermediate)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.rightThumbIntermediate);
        
        if(playerAvatarScript.rightThumbEnd)
            playerAvatarScript.fingerBoneTransforms.Add(playerAvatarScript.rightThumbEnd);

    }


    private void SetupHealthEnergyReadout()
    {
        EnergyHealthBar energyHealthBar = playerAvatarScript.GetComponentInChildren<EnergyHealthBar>();

        GameObject healthBar;
        
        if (energyHealthBar)
        {
            healthBar = energyHealthBar.gameObject;
        }
        else
        {
            healthBar = Instantiate(Resources.Load<GameObject>("Energy and Health Bars"));
            
            if (playerAvatarScript.rightForearmTwist)
            {
                healthBar.transform.SetParent(playerAvatarScript.rightForearmTwist);
            }
            else
            {
                healthBar.transform.SetParent(playerAvatarScript.rightForearm);
            }

            healthBar.transform.localPosition = Vector3.zero;
            
            energyHealthBar = healthBar.GetComponent<EnergyHealthBar>();
        }
        
        if(!healthBar)
            return;


        playerAvatarScript.healthAndEnergyBar = healthBar;
        

        if (energyHealthBar)
        {
            playerAvatarScript.healthParentObject = energyHealthBar.healthParentObject;
            playerAvatarScript.healthText = energyHealthBar.healthText;
            playerAvatarScript.healthSlider = energyHealthBar.healthSlider;
            
            playerAvatarScript.energyParentObject = energyHealthBar.energyParentObject;
            playerAvatarScript.energyText = energyHealthBar.energyText;
            playerAvatarScript.energySlider = energyHealthBar.energySlider;
        }
    }
    

    private void GenerateCustomMaterialSettings()
    {
        if(!avatarModel || !playerAvatarScript)
            return;
                
        List<Renderer> avatarRenderers = avatarModel.GetComponentsInChildren<Renderer>().ToList();

        for (int i = 0; i < avatarRenderers.Count; i++)
        {
            if(avatarRenderers[i].name.Contains("FingerTip") || avatarRenderers[i].name.Contains("Palm") || avatarRenderers[i].name == "Cube" 
               || avatarRenderers[i].name == "Capsule" || avatarRenderers[i].transform.parent.name.Contains("Palm Shape") || avatarRenderers[i].name.Contains("Don't Move"))
            {
                avatarRenderers.RemoveAt(i);
                i--;
            }
        }
        
        CustomMaterialSetting[] customSettingsArray = new CustomMaterialSetting[avatarRenderers.Count];
                
        for (int i = 0; i < avatarRenderers.Count; i++)
        {
            if (avatarRenderers[i])
            {
                CustomMaterialSetting newSetting = new CustomMaterialSetting();
                
                newSetting.renderer = avatarRenderers[i];
                
                if(newSetting.renderer.enabled)
                {
                    newSetting.rendererIsEnabled = true;
                }
                else
                {
                    newSetting.rendererIsEnabled = false;
                }

                newSetting.rendererNameForUserInterface = newSetting.renderer.name;
                        
                if(newSetting.renderer && newSetting.renderer.sharedMaterial)
                {
                    newSetting.originalMaterial = newSetting.renderer.sharedMaterial;
                
                    newSetting.originalMaterialMainTexture = newSetting.originalMaterial.mainTexture;
                    
                    newSetting.color = newSetting.originalMaterial.color;
                    
                    newSetting.originalShader = newSetting.originalMaterial.shader;
                    newSetting.originalShaderName = newSetting.originalMaterial.shader.name;
                }
                
                
                newSetting.originalMaterialUsingTexture = true;

                newSetting.activeMaterial = newSetting.originalMaterial;
                newSetting.activeMaterialMainTexture = newSetting.originalMaterialMainTexture;
                newSetting.activeMaterialUsingTexture = newSetting.originalMaterialUsingTexture;
                        
                
                customSettingsArray[i] = newSetting;
            }
        }


        
        for (int i = 0; i < customSettingsArray.Length; i++)
        {
            bool alreadyExists = false;
            
            for (int j = 0; j < playerAvatarScript.customMaterialSettings.Count; j++)
            {
                if (customSettingsArray[i].renderer == playerAvatarScript.customMaterialSettings[j].renderer)
                {
                    alreadyExists = true;
                }
            }
            
            if(alreadyExists == false)
                playerAvatarScript.customMaterialSettings.Add(customSettingsArray[i]);
        }
        
        CustomMaterialSettingsComplete = true;
    }


    private void ClearCustomMaterialSettings()
    {
        if(playerAvatarScript)
            playerAvatarScript.customMaterialSettings.Clear();
        
        CustomMaterialSettingsComplete = false;
    }


    private void EnableDebugRenderers()
    {
        //PostBuildCleanup();
        
        if (avatarModel)
            playerAvatarScript = avatarModel.GetComponent<PlayerAvatar>();
        
        EnableHandShapes();

        EnableEyeShape();
        
        EnableWebShapes();
    }

    private void DisableDebugRenderers()
    {
        //PostBuildCleanup();
        
        if (avatarModel)
            playerAvatarScript = avatarModel.GetComponent<PlayerAvatar>();
        
        DisableEyeShape();
        
        DisableHandShapes();

        DisableWebShapes();
    }

    private void EnableEyeShape()
    {
        if (playerAvatarScript.avatarEyes)
        {
            playerAvatarScript.avatarEyes.gameObject.SetActive(true);
            
            foreach (Transform childTransform in playerAvatarScript.avatarEyes)
            {
                childTransform.gameObject.SetActive(true);
                
                if (childTransform.GetComponent<MeshRenderer>())
                {
                    childTransform.GetComponent<MeshRenderer>().enabled = true;
                }
            }
        }
    }
    
    private void DisableEyeShape()
    {
        if (playerAvatarScript.avatarEyes)
        {
            playerAvatarScript.avatarEyes.gameObject.SetActive(true);
            
            foreach (Transform childTransform in playerAvatarScript.avatarEyes)
            {
                childTransform.gameObject.SetActive(false);
                
                if (childTransform.GetComponent<MeshRenderer>())
                {
                    childTransform.GetComponent<MeshRenderer>().enabled = false;
                }
            }
        }
    }
    

    private void EnableHandShapes()
    {
        if (avatarModel)
            playerAvatarScript = avatarModel.GetComponent<PlayerAvatar>();
        
        if(playerAvatarScript.fingerTips != null)
        {
            foreach (var tip in playerAvatarScript.fingerTips)
            {
                tip.gameObject.SetActive(true);
                if (tip.GetComponent<MeshRenderer>())
                {
                    tip.GetComponent<MeshRenderer>().enabled = true;
                }
            }
        }

        if (playerAvatarScript.leftPalm)
        {
            foreach (Transform childTransform in playerAvatarScript.leftPalm)
            {
                childTransform.gameObject.SetActive(true);
                
                if (childTransform.GetComponent<MeshRenderer>())
                {
                    childTransform.GetComponent<MeshRenderer>().enabled = true;
                }
            }

        }
        
        if (playerAvatarScript.rightPalm)
        {
            foreach (Transform childTransform in playerAvatarScript.rightPalm)
            {
                childTransform.gameObject.SetActive(true);
                
                if (childTransform.GetComponent<MeshRenderer>())
                {
                    childTransform.GetComponent<MeshRenderer>().enabled = true;
                }
            }
        }
    }

    private void DisableHandShapes()
    {
        if(playerAvatarScript.fingerTips != null)
        {
            foreach (var tip in playerAvatarScript.fingerTips)
            {
                tip.gameObject.SetActive(false);
                
                if (tip.GetComponent<MeshRenderer>())
                {
                    tip.GetComponent<MeshRenderer>().enabled = false;
                }
            }
        }

        if (playerAvatarScript.leftPalm)
        {
            foreach (Transform childTransform in playerAvatarScript.leftPalm)
            {
                childTransform.gameObject.SetActive(false);

                if (childTransform.GetComponent<MeshRenderer>())
                {
                    childTransform.GetComponent<MeshRenderer>().enabled = false;
                }
            }

        }
        
        if (playerAvatarScript.rightPalm)
        {
            foreach (Transform childTransform in playerAvatarScript.rightPalm)
            {
                childTransform.gameObject.SetActive(false);
                
                if (childTransform.GetComponent<MeshRenderer>())
                {
                    childTransform.GetComponent<MeshRenderer>().enabled = false;
                }
            }
        }
        
        if (playerAvatarScript.avatarEyes)
        {
            playerAvatarScript.avatarEyes.gameObject.SetActive(true);
            
            foreach (Transform childTransform in playerAvatarScript.avatarEyes)
            {
                childTransform.gameObject.SetActive(false);
                
                if (childTransform.GetComponent<MeshRenderer>())
                {
                    childTransform.GetComponent<MeshRenderer>().enabled = false;
                }
            }
        }

        if (handPoseCopierScript)
        {
            if(handPoseCopierScript.leftHandWeaponShapes != null)
            {
                foreach (var shape in handPoseCopierScript.leftHandWeaponShapes)
                {
                    if(shape)
                    {
                        shape.SetActive(false);

                        if (shape.GetComponent<MeshRenderer>())
                        {
                            shape.GetComponent<MeshRenderer>().enabled = false;
                        }
                    }
                }
            }
            
            if(handPoseCopierScript.rightHandWeaponShapes != null)
            {
                foreach (var shape in handPoseCopierScript.rightHandWeaponShapes)
                {
                    if(shape)
                    {
                        shape.SetActive(false);
                        
                        if (shape.GetComponent<MeshRenderer>())
                        {
                            shape.GetComponent<MeshRenderer>().enabled = false;
                        }
                    }
                }
            }
        }
    }
    
    private void EnableWebShapes()
    {
        if (weblineRendererScript)
        {
            weblineRendererScript.leftLineRenderer.enabled = true;
            weblineRendererScript.rightLineRenderer.enabled = true;

            if (weblineRendererScript.leftWebGrabHandPositionLower)
            {
                weblineRendererScript.leftWebGrabHandPositionLower.GetComponent<MeshRenderer>().enabled = true;
            }
            
            if (weblineRendererScript.leftWebGrabHandPositionUpper)
            {
                weblineRendererScript.leftWebGrabHandPositionUpper.GetComponent<MeshRenderer>().enabled = true;
            }
            
            if (weblineRendererScript.rightWebGrabHandPositionLower)
            {
                weblineRendererScript.rightWebGrabHandPositionLower.GetComponent<MeshRenderer>().enabled = true;
            }
            
            if (weblineRendererScript.rightWebGrabHandPositionUpper)
            {
                weblineRendererScript.rightWebGrabHandPositionUpper.GetComponent<MeshRenderer>().enabled = true;
            }
            
            if (playerAvatarScript.leftWeblineOriginPoint)
            {
                foreach (var renderer in playerAvatarScript.leftWeblineOriginPoint.GetComponentsInChildren<MeshRenderer>())
                {
                    renderer.enabled = true;
                }
            }
            
            if (playerAvatarScript.rightWeblineOriginPoint)
            {
                foreach (var renderer in playerAvatarScript.rightWeblineOriginPoint.GetComponentsInChildren<MeshRenderer>())
                {
                    renderer.enabled = true;
                }
            }
            
            
            if (weblineRendererScript.leftLineRenderer)
            {
                weblineRendererScript.leftLineRenderer.enabled = true;
            }
            
            if (weblineRendererScript.rightLineRenderer)
            {
                weblineRendererScript.rightLineRenderer.enabled = true;
            }
            
            if (weblineRendererScript.leftWeblineOriginLineRenderer)
            {
                weblineRendererScript.leftWeblineOriginLineRenderer.enabled = true;
            }
            
            if (weblineRendererScript.rightWeblineOriginLineRenderer)
            {
                weblineRendererScript.rightWeblineOriginLineRenderer.enabled = true;
            }
        }
    }

    private void DisableWebShapes()
    {
        if (weblineRendererScript)
        {
            weblineRendererScript.leftLineRenderer.enabled = false;
            weblineRendererScript.rightLineRenderer.enabled = false;

            if (weblineRendererScript.leftWebGrabHandPositionLower)
            {
                weblineRendererScript.leftWebGrabHandPositionLower.GetComponent<MeshRenderer>().enabled = false;
            }
            
            if (weblineRendererScript.leftWebGrabHandPositionUpper)
            {
                weblineRendererScript.leftWebGrabHandPositionUpper.GetComponent<MeshRenderer>().enabled = false;
            }
            
            if (weblineRendererScript.rightWebGrabHandPositionLower)
            {
                weblineRendererScript.rightWebGrabHandPositionLower.GetComponent<MeshRenderer>().enabled = false;
            }
            
            if (weblineRendererScript.rightWebGrabHandPositionUpper)
            {
                weblineRendererScript.rightWebGrabHandPositionUpper.GetComponent<MeshRenderer>().enabled = false;
            }
            
            if (playerAvatarScript.leftWeblineOriginPoint)
            {
                foreach (var renderer in playerAvatarScript.leftWeblineOriginPoint.GetComponentsInChildren<MeshRenderer>())
                {
                    renderer.enabled = false;
                }
            }
            
            if (playerAvatarScript.rightWeblineOriginPoint)
            {
                foreach (var renderer in playerAvatarScript.rightWeblineOriginPoint.GetComponentsInChildren<MeshRenderer>())
                {
                    renderer.enabled = false;
                }
            }

            if (weblineRendererScript.leftLineRenderer)
            {
                weblineRendererScript.leftLineRenderer.enabled = false;
            }
            
            if (weblineRendererScript.rightLineRenderer)
            {
                weblineRendererScript.rightLineRenderer.enabled = false;
            }
            
            if (weblineRendererScript.leftWeblineOriginLineRenderer)
            {
                weblineRendererScript.leftWeblineOriginLineRenderer.enabled = false;
            }
            
            if (weblineRendererScript.rightWeblineOriginLineRenderer)
            {
                weblineRendererScript.rightWeblineOriginLineRenderer.enabled = false;
            }
        }
    }
    
    
    
    
    public void OpenFolderAfterModsBuild()
    {
        GetDataHolder();
        
        if(dataHolder.lastAddressableBuildPath != "")
        {
            EditorUtility.RevealInFinder(dataHolder.lastAddressableBuildPath);
        }
        else
        {
            EditorUtility.RevealInFinder(DemiModBase.exportPath);
        }
    }
    
}
#endif