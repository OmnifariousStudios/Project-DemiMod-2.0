#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BzKovSoft.RagdollHelper.Editor;
using RootMotion.Dynamics;

public class ProjectDemiModCustomEnemyExporter : EditorWindow
{
    public DataHolder dataHolder;
    
    public bool FolderSetupComplete = false;

    // The original avatar model that will be used to create the enemy mod.
    private GameObject avatarModel;
    
    private EnemyComponentReference enemyComponentReference;
    private GameObject characterRoot;
    private GameObject enemyModRoot;
    private Animator characterAnimator;
    private CapsuleCollider characterCapsuleCollider;
    private GameObject ragdoll;
    //private PuppetMaster puppetMaster;
    //private VRPuppet vrPuppet;

    private List<Transform> aimIKBones;

    private GameObject finalPrefab;

    float vSbarValue;
    public Vector2 scrollPosition = Vector2.zero;


    [MenuItem("Project Demigod/Enemy Mod Exporter")]
    public static void ShowEnemyModWindow()
    {
        GetWindow<ProjectDemiModCustomEnemyExporter>("Enemy Mod Exporter");
    }

    private void Awake()
    {
        if (DemiModBase.buildTarget == BuildTarget.NoTarget)
            DemiModBase.buildTarget = BuildTarget.StandaloneWindows64;
        
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
            if (dataHolder.userDefinedModsLocation == "")
            {
                GUI.color = Color.red;
                EditorGUILayout.HelpBox("Please choose a location to store built mods.", MessageType.Info);
            }
            else
            {
                GUI.color = Color.green;
                EditorGUILayout.HelpBox("Current Location: " + dataHolder.userDefinedModsLocation, MessageType.Info);
            }
        }
        
        GUI.color = Color.white;
        
        
        if (avatarModel == null) 
        {
            EditorGUILayout.HelpBox("Drag the avatar model here to continue.", MessageType.Info);
        } 
        else if (avatarModel) 
        {
            EditorGUILayout.HelpBox(avatarModel + " will be tested for correct settings.", MessageType.Info);
        } 
        else 
        {
            EditorGUILayout.HelpBox(avatarModel + " is empty.", MessageType.Info);
        }

        
        avatarModel = EditorGUILayout.ObjectField("Avatar Model", avatarModel, typeof(GameObject), true) as GameObject;
        
        if(avatarModel || enemyModRoot)
        {
            enemyModRoot = EditorGUILayout.ObjectField("Enemy Mod Root", enemyModRoot, typeof(GameObject), true) as GameObject;
            characterRoot = EditorGUILayout.ObjectField("Character Root", characterRoot, typeof(GameObject), true) as GameObject;
            ragdoll = EditorGUILayout.ObjectField("Ragdoll", ragdoll, typeof(GameObject), true) as GameObject;
        }

        
        
        DemiModBase.AddLineAndSpace();
        
        #region SwitchPlatforms

        EditorGUILayout.HelpBox("Current Target: " + EditorUserBuildSettings.selectedStandaloneTarget.ToString(), MessageType.Info);

        GUILayout.BeginHorizontal("Switch Platforms", GUI.skin.window);

        using (new EditorGUI.DisabledScope(EditorUserBuildSettings.selectedStandaloneTarget == BuildTarget.StandaloneWindows64))
        {
            //EditorGUILayout.HelpBox("Current Target: Android", MessageType.Info);
            if (GUILayout.Button("Switch to Windows"))
            {
                DemiModBase.SwitchToWindows();
            }
        }

        using (new EditorGUI.DisabledScope(EditorUserBuildSettings.selectedStandaloneTarget == BuildTarget.Android))
        {
            //EditorGUILayout.HelpBox("Current Target: Windows", MessageType.Info);
            if (GUILayout.Button("Switch to Android"))
            {
                DemiModBase.SwitchToAndroid();
            }
        }

        GUILayout.EndHorizontal();

        #endregion
        
        DemiModBase.AddLineAndSpace();

        
        #region Starting Mod Process

        using (new EditorGUI.DisabledScope(avatarModel == null))
        {
            if (GUILayout.Button("Start Mod Process", GUILayout.Height(20)))
            {
                CreateCharacterRootFromModel();
            }
        }

        
        using (new EditorGUI.DisabledScope(characterRoot == null))
        {
            if(GUILayout.Button("Start Ragdoll Creation", GUILayout.Height(20)))
            {
                StartRagdoll();
            }
            
            if(GUILayout.Button("Finish Ragdoll Creation", GUILayout.Height(20)))
            {
                RagdollFinished();
            }

            if (GUILayout.Button("Enemy Mod Setup", GUILayout.Height(20)))
            {
                EnemyModSetup();
            }
        }
        
        #endregion

        DemiModBase.AddLineAndSpace();
        
        using (new EditorGUI.DisabledScope(finalPrefab == null))
        {
            finalPrefab = EditorGUILayout.ObjectField("Final Prefab", finalPrefab, typeof(GameObject), true) as GameObject;
        }
        
        

        #region Build Addressables

        bool canBuild = characterRoot != null;

        // Build the addressables for the enemy mod.
        using (new EditorGUI.DisabledScope(!canBuild))
        {
            GUILayout.BeginHorizontal("Build the Mods", GUI.skin.window);
            if (GUILayout.Button("Build for Windows (PCVR)", GUILayout.Height(20)))
            {
                GetDataHolder();
                dataHolder.lastEnemyAvatarPrefab = finalPrefab;
                dataHolder.lastEnemyAvatarName = enemyModRoot.name;
                
                //string name = enemyModRoot.name;
                DemiModBase.ExportWindows(DemiModBase.ModType.Enemy, enemyModRoot);

                PostBuildCleanup();
                
                EditorApplication.delayCall += () =>
                {
                    OpenFolderAfterModsBuild();
                };
            }

            if (GUILayout.Button("Build for Android (Quest)", GUILayout.Height(20)))
            {
                GetDataHolder();
                
                dataHolder.lastEnemyModel = avatarModel;
                dataHolder.lastEnemyModelName = avatarModel.name;
                
                dataHolder.lastRagdollRoot = ragdoll;
                dataHolder.lastRagdollRootName = ragdoll.name;
                
                dataHolder.lastEnemyAvatarPrefab = finalPrefab;
                dataHolder.lastEnemyAvatarName = enemyModRoot.name;
                
                DemiModBase.ExportAndroid(DemiModBase.ModType.Enemy, avatarModel);

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

/*
        #region Finish Setup

        EditorGUILayout.HelpBox(" Use this button to Finish Setup AFTER building the Addressable.", MessageType.Info);
        if (GUILayout.Button("Finish Setup", GUILayout.Height(20)))
        {
            DemiModBase.GetOrCreateModPath(DemiModBase.ModType.Enemy, avatarModel.name);
        }

        #endregion
*/
        
        DemiModBase.AddLineAndSpace();
        
        
        // Save the enemy prefab.
        using (new EditorGUI.DisabledScope(enemyModRoot == null))
        {
            if (GUILayout.Button("Save Enemy Prefab", GUILayout.Height(20)))
            {
                if (enemyModRoot)
                {
                    Debug.Log("Saving Enemy Prefab");
                    PrefabUtility.ApplyPrefabInstance(enemyModRoot, InteractionMode.UserAction);
                }
            }
        }

        
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
        
        // End the scroll view that we began above.
        EditorGUILayout.EndScrollView();
    }

    
    public void PostBuildCleanup()
    {
        GetDataHolder();
        
        if(dataHolder.lastEnemyModel)
            avatarModel = GameObject.Find(dataHolder.lastEnemyAvatarName);
        else
        {
            avatarModel = GameObject.Find(dataHolder.lastEnemyModelName);
        }
        
        
        if(dataHolder.lastEnemyAvatarPrefab)
            finalPrefab = dataHolder.lastEnemyAvatarPrefab;
        else
        {
            finalPrefab = GameObject.Find(dataHolder.lastEnemyAvatarName);
        }
        
        
        if(dataHolder.lastRagdollRoot)
            ragdoll = GameObject.Find(dataHolder.lastRagdollRootName);
        else
        {
            ragdoll = GameObject.Find(dataHolder.lastRagdollRootName);
        }

        
        if (avatarModel)
        {
            Debug.Log("Avatar Model Found");
        }
    }

    
    void CreateCharacterRootFromModel()
    {
        if(!characterRoot)
        {
            Debug.Log("Creating Character Root");
            
            characterRoot = Instantiate(avatarModel);
            
            if(avatarModel)
                avatarModel.SetActive(false);
        }
        
        characterRoot.name = avatarModel.name + " Character Root";
        
        if(characterRoot && !characterAnimator)
            characterAnimator = characterRoot.GetComponent<Animator>();
    }
    
    

    void EnemyModSetup()
    {
        // If no enemyModRoot, create the root and add the avatar model to it.
        if (!enemyModRoot)
        {
            Debug.Log("Creating Enemy Mod Root");
            
            enemyModRoot = new GameObject();
            enemyModRoot.name = "Enemy Avatar - " + avatarModel.name;
        }
        else
        {
            enemyModRoot.name = "Enemy Avatar - " + avatarModel.name;
        }
        
        
        enemyComponentReference = enemyModRoot.GetComponent<EnemyComponentReference>();
         
        if (!enemyComponentReference)
            enemyComponentReference = enemyModRoot.AddComponent<EnemyComponentReference>();
        
        
        // Create copy of the character root.
        if(!ragdoll)
        {
            Debug.Log("Creating Ragdoll");
            
            ragdoll = Instantiate(characterRoot, characterRoot.transform.position, characterRoot.transform.rotation);
            ragdoll.name = "Ragdoll";
        }
        
        
        /*
        if(ragdoll.GetComponent<PuppetMaster>() == null)
        {
            Debug.Log("Adding PuppetMaster to Ragdoll");
            puppetMaster = ragdoll.AddComponent<PuppetMaster>();
        }
        else
        {
            Debug.Log("PuppetMaster already exists on Ragdoll");
            puppetMaster = ragdoll.GetComponent<PuppetMaster>();
        }

        
        if (ragdoll.GetComponent<VRPuppet>() == null)
        {
            Debug.Log("Adding VRPuppet to Ragdoll");
            vrPuppet = ragdoll.AddComponent<VRPuppet>();
        }
        else
        {
            Debug.Log("VRPuppet already exists on Ragdoll");
            vrPuppet = ragdoll.GetComponent<VRPuppet>();
        }
        
        vrPuppet.puppetMaster = puppetMaster;
        vrPuppet.enemyComponentReference = enemyComponentReference;
        */
        
        // Set both as children.
        if(characterRoot.transform.parent != enemyModRoot.transform)
            characterRoot.transform.SetParent(enemyModRoot.transform);
        
        if(ragdoll.transform.parent != enemyModRoot.transform)
            ragdoll.transform.SetParent(enemyModRoot.transform);
        
        
        if (characterRoot.TryGetComponent(out characterCapsuleCollider) == false)
        {
            characterCapsuleCollider = characterRoot.AddComponent<CapsuleCollider>();
            
            if(characterRoot && !characterAnimator)
                characterAnimator = characterRoot.GetComponent<Animator>();
            
            // Set collider to be the same size as the character.
            float characterHeight = characterAnimator.GetBoneTransform(HumanBodyBones.Head).position.y - characterAnimator.GetBoneTransform(HumanBodyBones.LeftToes).position.y;
            characterCapsuleCollider.center = new Vector3(0, characterHeight / 2, 0);
            characterCapsuleCollider.height = characterHeight;
        }
        
        
        RemoveRagdollComponentsFromCharacterRoot();


        RemoveAvatarComponentsFromRagdoll();

        
        AddAllRagdollComponents();
        
        
        SetEnemyReferences();
        
        
        finalPrefab = PrefabUtility.SaveAsPrefabAssetAndConnect(enemyModRoot, 
            DemiModBase.GetOrCreateModPath(DemiModBase.ModType.Enemy, enemyModRoot.gameObject.name) + ".prefab", InteractionMode.UserAction);
    }


    private void AddAllRagdollComponents()
    {
        Debug.Log("Adding Ragdoll Components");
        
        Rigidbody[] rigidbodies = ragdoll.GetComponentsInChildren<Rigidbody>();
        
        foreach (Rigidbody rigidbody in rigidbodies)
        {
            HVRGrabbablePlaceHolder grabbable = rigidbody.GetComponent<HVRGrabbablePlaceHolder>();
            
            if (grabbable == null)
                grabbable = rigidbody.gameObject.AddComponent<HVRGrabbablePlaceHolder>();
            
            //HVRStabbable stabbable = rigidbody.GetComponent<HVRStabbable>();
            
            //if (stabbable == null)
                //stabbable = rigidbody.gameObject.AddComponent<HVRStabbable>();
            
            GrabberHelper grabberHelper = rigidbody.GetComponent<GrabberHelper>();
            
            if (grabberHelper == null)
                grabberHelper = rigidbody.gameObject.AddComponent<GrabberHelper>();
            
            Collider collider = rigidbody.GetComponent<Collider>();
            
            
            //grabbable.Rigidbody = rigidbody;
            
            grabberHelper.thisCollider = collider;
            grabberHelper.rb = rigidbody;
            //grabberHelper.thisGrabbable = grabbable;
            //grabberHelper.thisStabbable = stabbable;
            
            grabberHelper.enemyComponentReference = enemyComponentReference;
            
            /*
            grabberHelper.vrPuppet = vrPuppet;
            
            if(vrPuppet.puppetRigidbodies == null)
                vrPuppet.puppetRigidbodies = new System.Collections.Generic.List<Rigidbody>();
            
            if(vrPuppet.puppetRigidbodies.Contains(rigidbody) == false)
                vrPuppet.puppetRigidbodies.Add(rigidbody);
            
            if(vrPuppet.puppetColliders == null)
                vrPuppet.puppetColliders = new System.Collections.Generic.List<Collider>();
            
            if(vrPuppet.puppetColliders.Contains(collider) == false)
                vrPuppet.puppetColliders.Add(collider);
            */
            
            if(enemyComponentReference.enemyRigidbodies == null)
                enemyComponentReference.enemyRigidbodies = new System.Collections.Generic.List<Rigidbody>();
            
            if(enemyComponentReference.enemyRigidbodies.Contains(rigidbody) == false)
                enemyComponentReference.enemyRigidbodies.Add(rigidbody);
            
            if(enemyComponentReference.puppetGrabbables == null)
                enemyComponentReference.puppetGrabbables = new System.Collections.Generic.List<HVRGrabbable>();
            
            //if(enemyComponentReference.puppetGrabbables.Contains(grabbable) == false)
                //enemyComponentReference.puppetGrabbables.Add(grabbable);
            
            if(enemyComponentReference.puppetStabbables == null)
                enemyComponentReference.puppetStabbables = new System.Collections.Generic.List<HVRStabbable>();
            
            //if(enemyComponentReference.puppetStabbables.Contains(stabbable) == false)
                //enemyComponentReference.puppetStabbables.Add(stabbable);
            
            if(enemyComponentReference.puppetGrabberHelpers == null)
                enemyComponentReference.puppetGrabberHelpers = new System.Collections.Generic.List<GrabberHelper>();
            
            if(enemyComponentReference.puppetGrabberHelpers.Contains(grabberHelper) == false)
                enemyComponentReference.puppetGrabberHelpers.Add(grabberHelper);
        }
    }
    
    private void RemoveAvatarComponentsFromRagdoll()
    {
        // Remove extra components from the ragdoll, like the animator controller and meshes.
        if (ragdoll.GetComponent<Animator>())
        {
            DestroyImmediate(ragdoll.GetComponent<Animator>());
        }
        
        SkinnedMeshRenderer[] skinnedMeshRenderers = ragdoll.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
        {
            Debug.Log("Removing SkinnedMeshRenderer: " + skinnedMeshRenderer);
            DestroyImmediate(skinnedMeshRenderer);
        }
        
        MeshRenderer[] meshRenderers = ragdoll.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            Debug.Log("Removing MeshRenderer: " + meshRenderer);
            DestroyImmediate(meshRenderer);
        }
        
        DeleteExtra(ragdoll.transform);
    }


    private void RemoveRagdollComponentsFromCharacterRoot()
    {
        // Remove all ragdoll helper components from the avatar model.
        
        Joint[] joints = characterRoot.GetComponentsInChildren<Joint>();
        foreach (Joint joint in joints)
        {
            Debug.Log("Removing Joint: " + joint);
            DestroyImmediate(joint);
        }
        
        Rigidbody[] rigidbodies = characterRoot.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rigidbody in rigidbodies)
        {
            Debug.Log("Removing Rigidbody: " + rigidbody);
            DestroyImmediate(rigidbody);
        }
        
        Collider[] colliders = characterRoot.GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            Debug.Log("Removing Collider: " + collider);
            
            if(characterCapsuleCollider && collider == characterCapsuleCollider)
                continue;
            
            DestroyImmediate(collider);
        }
    }
    
    
    
    void StartRagdoll()
    {
        // Use BZ Ragdoll Helper to create ragdoll. 
        
        //ShowRagdollHelperWindow();

        if (characterRoot && !ragdoll)
        {
            if (characterRoot.TryGetComponent(out BipedRagdollCreator bipedRagdollCreator) == false)
            {
                characterRoot.AddComponent<BipedRagdollCreator>();
            }
            
        }
    }
    
    void RagdollFinished()
    {
        Debug.Log("Ragdoll Finished");
        
        CloseRagdollHelperWindow();
        
        if (characterRoot.TryGetComponent(out BipedRagdollCreator bipedRagdollCreator))
        {
            DestroyImmediate(bipedRagdollCreator);
        }
        
        if (characterRoot.TryGetComponent(out RagdollEditor ragdollEditor))
        {
            DestroyImmediate(ragdollEditor);
        }
    }
    
    
    static void ShowRagdollHelperWindow()
    {
        EditorWindow.GetWindow(typeof(BoneHelper), false, "Ragdoll helper");
    }
    
    static void CloseRagdollHelperWindow()
    {
        EditorWindow.GetWindow(typeof(BoneHelper), false, "Ragdoll helper").Close();
    }


    public void SetEnemyReferences()
    {
        if(!characterAnimator)
            characterAnimator = characterRoot.GetComponent<Animator>();
        
        if(aimIKBones == null)
            aimIKBones = new List<Transform>();
        
        if (characterAnimator.GetBoneTransform(HumanBodyBones.Spine))
        {
            bool hasSpine = false;
            foreach (var bone in aimIKBones)
            {
                if(bone && bone.transform == characterAnimator.GetBoneTransform(HumanBodyBones.Spine))
                { 
                    hasSpine = true;
                    break;
                }
            }
            
            if(hasSpine == false)
                aimIKBones.Add(characterAnimator.GetBoneTransform(HumanBodyBones.Spine));
        }
        
        if (characterAnimator.GetBoneTransform(HumanBodyBones.Chest))
        {
            bool hasChest = false;
            foreach (var bone in aimIKBones)
            {
                if(bone && bone.transform == characterAnimator.GetBoneTransform(HumanBodyBones.Chest))
                { 
                    hasChest = true;
                    break;
                }
            }
            
            if(hasChest == false)
                aimIKBones.Add(characterAnimator.GetBoneTransform(HumanBodyBones.Chest));
        }
        
        if(characterAnimator.GetBoneTransform(HumanBodyBones.UpperChest))
        {
            bool hasUpperChest = false;
            foreach (var bone in aimIKBones)
            {
                if(bone && bone.transform == characterAnimator.GetBoneTransform(HumanBodyBones.UpperChest))
                { 
                    hasUpperChest = true;
                    break;
                }
            }
            
            if(hasUpperChest == false)
                aimIKBones.Add(characterAnimator.GetBoneTransform(HumanBodyBones.UpperChest));
        }
        
        if (characterAnimator.GetBoneTransform(HumanBodyBones.Neck))
        {
            bool hasNeck = false;
            foreach (var bone in aimIKBones)
            {
                if(bone && bone.transform == characterAnimator.GetBoneTransform(HumanBodyBones.Neck))
                { 
                    hasNeck = true;
                    break;
                }
            }
            
            if(hasNeck == false)
                aimIKBones.Add(characterAnimator.GetBoneTransform(HumanBodyBones.Neck));
        }

        Transform head = null;
        if (characterAnimator.GetBoneTransform(HumanBodyBones.Head))
        {
            bool hasHead = false;
            foreach (var bone in aimIKBones)
            {
                if(bone && bone.transform == characterAnimator.GetBoneTransform(HumanBodyBones.Head))
                { 
                    hasHead = true;
                    head = bone.transform;
                    break;
                }
            }
        }
        
        
        Transform eyes = characterRoot.transform.FindChildRecursive("Eyes Forward Transform");
        if (eyes == null)
        {
            eyes = new GameObject("Eyes Forward Transform").transform;
            
            eyes.forward = characterRoot.transform.forward;
            
            if(characterAnimator.GetBoneTransform(HumanBodyBones.Head))
            {
                eyes.SetParent(characterAnimator.GetBoneTransform(HumanBodyBones.Head));
            }
            else if(characterAnimator.GetBoneTransform(HumanBodyBones.Neck))
            {
                eyes.SetParent(characterAnimator.GetBoneTransform(HumanBodyBones.Neck));
            }
            
            eyes.localPosition = Vector3.zero;
        }
        
        
        
        Transform pin = characterRoot.transform.FindChildRecursive("Forward Pin");
        if (pin == null)
        {
            Debug.Log("Creating Pin");
            
            pin = new GameObject("Forward Pin").transform;
            pin.SetParent(characterRoot.transform);
            
            if(characterAnimator.GetBoneTransform(HumanBodyBones.UpperChest))
            {
                pin.position = characterAnimator.GetBoneTransform(HumanBodyBones.UpperChest).position + characterRoot.transform.forward;
            }
            else if(characterAnimator.GetBoneTransform(HumanBodyBones.Chest))
            {
                pin.position = characterAnimator.GetBoneTransform(HumanBodyBones.Chest).position + characterRoot.transform.forward;
            }
        }
        else
        {
            Debug.Log("Forward Pin already exists");
            
            pin.SetParent(characterRoot.transform);
            
            if(characterAnimator.GetBoneTransform(HumanBodyBones.UpperChest))
            {
                pin.position = characterAnimator.GetBoneTransform(HumanBodyBones.UpperChest).position + characterRoot.transform.forward;
            }
            else if(characterAnimator.GetBoneTransform(HumanBodyBones.Chest))
            {
                pin.position = characterAnimator.GetBoneTransform(HumanBodyBones.Chest).position + characterRoot.transform.forward;
            }
        }
        
        Transform aimTransform = characterRoot.transform.FindChildRecursive("Aim Transform");
        if (aimTransform == null)
        {
            aimTransform = new GameObject("Aim Transform").transform;
        }
        
        if (eyes)
        {
            aimTransform.SetParent(eyes);
        }
        else if(characterAnimator.GetBoneTransform(HumanBodyBones.Head))
        {
            aimTransform.SetParent(characterAnimator.GetBoneTransform(HumanBodyBones.Head));
        }
        else if(characterAnimator.GetBoneTransform(HumanBodyBones.Neck))
        {
            aimTransform.SetParent(characterAnimator.GetBoneTransform(HumanBodyBones.Neck));
        }
        else if(characterAnimator.GetBoneTransform(HumanBodyBones.UpperChest))
        {
            aimTransform.SetParent(characterAnimator.GetBoneTransform(HumanBodyBones.UpperChest));
        }
        else if(characterAnimator.GetBoneTransform(HumanBodyBones.Chest))
        {
            aimTransform.SetParent(characterAnimator.GetBoneTransform(HumanBodyBones.Chest));
        }
        else if(characterAnimator.GetBoneTransform(HumanBodyBones.Spine))
        {
            aimTransform.SetParent(characterAnimator.GetBoneTransform(HumanBodyBones.Spine));
        }
        
        aimTransform.localPosition = Vector3.zero;
        aimTransform.localRotation = Quaternion.identity;
        
        
        /*
        Transform waistline = characterRoot.transform.FindChildRecursive("Waistline");
        if (waistline == null)
        {
            waistline = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
            DestroyImmediate(waistline.GetComponent<Collider>());
            waistline.name = "Waistline";
            waistline.localScale = new Vector3(0.6f, 0.01f, 0.6f);
            
            if(characterAnimator.GetBoneTransform(HumanBodyBones.Hips))
            {
                waistline.position = characterAnimator.GetBoneTransform(HumanBodyBones.Hips).position;
            }
            
            waistline.SetParent(characterRoot.transform);
        }
        */
        
        // Palm Forward Transforms
        Transform rightPalmForward = characterRoot.transform.FindChildRecursive("Right Palm Forward");
        if (rightPalmForward == null)
        {
            rightPalmForward = new GameObject("Right Palm Forward").transform;
            rightPalmForward.SetParent(characterAnimator.GetBoneTransform(HumanBodyBones.RightHand));
            
            // Use the cross product of the hand to the pointer finger and the hand to the middle finger to get the palm forward direction.
            Vector3 handToPointer = characterAnimator.GetBoneTransform(HumanBodyBones.RightIndexProximal).position - characterAnimator.GetBoneTransform(HumanBodyBones.RightHand).position;
            Vector3 handToMiddle = characterAnimator.GetBoneTransform(HumanBodyBones.RightMiddleProximal).position - characterAnimator.GetBoneTransform(HumanBodyBones.RightHand).position;
            
            rightPalmForward.forward = Vector3.Cross(handToMiddle, handToPointer);

            Vector3 middleOfPalm = (characterAnimator.GetBoneTransform(HumanBodyBones.RightHand).position + characterAnimator.GetBoneTransform(HumanBodyBones.RightMiddleProximal).position) / 2;
            rightPalmForward.position = middleOfPalm + rightPalmForward.forward * 0.25f;
        }
        
        Transform leftPalmForward = characterRoot.transform.FindChildRecursive("Left Palm Forward");
        if (leftPalmForward == null)
        {
            leftPalmForward = new GameObject("Left Palm Forward").transform;
            leftPalmForward.SetParent(characterAnimator.GetBoneTransform(HumanBodyBones.LeftHand));
            
            // Use the cross product of the hand to the pointer finger and the hand to the middle finger to get the palm forward direction.
            Vector3 handToPointer = characterAnimator.GetBoneTransform(HumanBodyBones.LeftIndexProximal).position - characterAnimator.GetBoneTransform(HumanBodyBones.LeftHand).position;
            Vector3 handToMiddle = characterAnimator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal).position - characterAnimator.GetBoneTransform(HumanBodyBones.LeftHand).position;
            
            // This has to be the opposite of the right hand because of the way the cross product works.
            leftPalmForward.forward = Vector3.Cross(handToPointer, handToMiddle);

            Vector3 middleOfPalm = (characterAnimator.GetBoneTransform(HumanBodyBones.LeftHand).position + characterAnimator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal).position) / 2;
            leftPalmForward.position = middleOfPalm + leftPalmForward.forward * 0.25f;
        }
        
       
        
        Transform leftHandGrip = characterRoot.transform.FindChildRecursive("Left Hand Grip");
        if (leftHandGrip == null)
        {
            leftHandGrip = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
            DestroyImmediate(leftHandGrip.GetComponent<Collider>());
            leftHandGrip.name = "Left Hand Grip";
            leftHandGrip.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            leftHandGrip.SetParent(characterAnimator.GetBoneTransform(HumanBodyBones.LeftHand));
            
            // Place the grip along the first bones of the middle and ring fingers.
            Vector3 middleFinger = characterAnimator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal).position;
            Vector3 ringFinger = characterAnimator.GetBoneTransform(HumanBodyBones.LeftRingProximal).position;
            
            Vector3 adJustPosition = (middleFinger + ringFinger) / 2;
            
            adJustPosition = (adJustPosition + characterAnimator.GetBoneTransform(HumanBodyBones.LeftHand).position) / 2;
            
            leftHandGrip.position = adJustPosition + leftPalmForward.forward * 0.05f;
            
            // Rotate the grip to be along the middle and ring fingers.
            leftHandGrip.up = ringFinger - middleFinger;
        }
        
        Transform rightHandGrip = characterRoot.transform.FindChildRecursive("Right Hand Grip");    
        if (rightHandGrip == null)
        {
            rightHandGrip = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
            DestroyImmediate(rightHandGrip.GetComponent<Collider>());
            rightHandGrip.name = "Right Hand Grip";
            rightHandGrip.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            rightHandGrip.SetParent(characterAnimator.GetBoneTransform(HumanBodyBones.RightHand));
            
            // Place the grip along the first bones of the middle and ring fingers.
            Vector3 middleFinger = characterAnimator.GetBoneTransform(HumanBodyBones.RightMiddleProximal).position;
            Vector3 ringFinger = characterAnimator.GetBoneTransform(HumanBodyBones.RightRingProximal).position;
            
            Vector3 adJustPosition = (middleFinger + ringFinger) / 2;
            
            adJustPosition = (adJustPosition + characterAnimator.GetBoneTransform(HumanBodyBones.RightHand).position) / 2;
            
            rightHandGrip.position = adJustPosition + rightPalmForward.forward * 0.05f;
            
            // Rotate the grip to be along the middle and ring fingers.
            rightHandGrip.up = ringFinger - middleFinger;
        }
            
        
        if(!enemyComponentReference && enemyModRoot)
            enemyComponentReference = enemyModRoot.GetComponent<EnemyComponentReference>();

        
        if (enemyComponentReference)
        {
            enemyComponentReference.spineBones = aimIKBones.ToArray();

            foreach (var bone in enemyComponentReference.spineBones)
            {
                Debug.Log("Testing bone: " + bone);
                // remove any null bones
                if (bone == null)
                {
                    aimIKBones.Remove(bone);
                    
                    Debug.Log("Removing null bone");
                }
            }
            
            if (characterAnimator)
            {
                enemyComponentReference.anim = characterAnimator;
            }
            
            if(characterAnimator.TryGetComponent(out Rigidbody rigidbody))
            {
                enemyComponentReference.mainEnemyRigidbody = rigidbody;
            }
            else
            {
                enemyComponentReference.mainEnemyRigidbody = characterAnimator.gameObject.AddComponent<Rigidbody>();
            }
            
            if(eyes)
                enemyComponentReference.eyes = eyes;
            
            if(head)
                enemyComponentReference.headBone = head;
            
            if(aimTransform)
                enemyComponentReference.AimTransform = aimTransform;
            
            if(pin)
                enemyComponentReference.forwardPin = pin;
            
            if(rightPalmForward)
                enemyComponentReference.rightPalmForward = rightPalmForward;
            
            if(leftPalmForward)
                enemyComponentReference.leftPalmForward = leftPalmForward;
            
            
            //if(puppetMaster)
                //enemyComponentReference.puppetMaster = puppetMaster;
        }
        
    }
    
    
    // Removes all bones that don't have a Rigidbody or a Collider attached because they are not part of the simulation
    private void DeleteExtra(Transform transform) 
    {
        Transform[] children = transform.GetComponentsInChildren<Transform>();
        
        for (int i = 1; i < children.Length; i++) 
        {
            bool keep = false;

            Rigidbody rb = children[i].GetComponent<Rigidbody>();
            Collider collider = children[i].GetComponent<Collider>();
            Joint joint = children[i].GetComponent<Joint>();
            
            if(rb != null || joint != null)
                keep = true;
            
            if(!keep && collider != null && rb == null)
                keep = true;
            

            if (!keep)
            {
                Transform[] save = new Transform[children[i].childCount];
                
                for (int c = 0; c < save.Length; c++) 
                {
                    save[c] = children[i].GetChild(c);
                }
					
                for (int c = 0; c < save.Length; c++) 
                {
                    save[c].parent = children[i].parent;
                }
					
                DestroyImmediate(children[i].gameObject);
            }
        }
    }
    
    
    private void ResetButtonCompletionStatus()
    {
        FolderSetupComplete = false;
        
        avatarModel = null;
        characterRoot = null;
        enemyModRoot = null;
        characterAnimator = null;
        characterCapsuleCollider = null;
        ragdoll = null;
        aimIKBones = null;
        finalPrefab = null;
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