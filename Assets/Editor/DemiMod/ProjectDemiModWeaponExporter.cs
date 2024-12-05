#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class ProjectDemiModWeaponExporter : EditorWindow
{
    //public WeaponModType weaponModType = WeaponModType.MeshReplacement;
    
    // GameObject/mesh that the modder places into the export window.
    public GameObject originalWeaponModel;
    [FormerlySerializedAs("lastWeaponModel")] public GameObject lastOriginalWeaponModel;
    
    
    [Space(30)]
    [FormerlySerializedAs("weaponToMod")]  public GameObject weaponModel;
    
    public GameObject infusionMesh;
    public GameObject scaler;
    
    // Root to be created and parented to the weaponToMod. Grabbable and Damage scripts go here.
    public GameObject weaponToModRoot;
    public GameObject lastWeaponToModRoot;

    public WeaponMod weaponModScript;
    public WeaponType weaponType;
    private WeaponType lastWeaponType;
    
    private WeaponAbility weaponAbility;
    private WeaponAbility lastWeaponAbility;
    
    // Meshes from originalWeaponModel that will be used for Auto-Adding Colliders
    private List<GameObject> meshesToAutoAddColliders = new List<GameObject>();
    private List<Collider> collidersToAutoAdd = new List<Collider>();
    
    // The final prefab asset that will be exported.
    private GameObject finalPrefab;
    
    private string weaponModName;
    
    
    private void Awake()
    {
        if (buildTarget == BuildTarget.NoTarget)
            buildTarget = BuildTarget.StandaloneWindows64;
        
        GetDataHolder();
    }
    
    
    private void OnGUI()
    {
        #region Opening GUI
        
        EditorGUIUtility.labelWidth = 160;
        GUILayout.Label("Project Demigod Mod Exporter", EditorStyles.largeLabel);
        GUILayout.Space(10);


        GUILayoutOption[] options = { GUILayout.MaxWidth(1000), GUILayout.MinWidth(250) };
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, options);

        #endregion
        
        #region Mod Storage Locations
        
        if (!dataHolder)
            GetDataHolder();

        EditorGUILayout.HelpBox("Choose where you would like mods to be stored.", MessageType.Info);
        if (GUILayout.Button("Select Mod Location", GUILayout.Height(20)))
        {
            if (dataHolder)
            {
                SetDefaultModLocation();
                // save assets
                EditorUtility.SetDirty(dataHolder);
            }
        }

        if (dataHolder)
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

        #endregion
        
        //weaponModType = (WeaponModType)EditorGUILayout.EnumPopup("Weapon Mod Type", weaponModType);
        
        DemiModBase.AddLineAndSpace();
        
        using (new EditorGUI.DisabledScope(weaponModel != null))
        {
            originalWeaponModel = EditorGUILayout.ObjectField("Original Weapon Model", originalWeaponModel, typeof(GameObject), true, options) as GameObject;
        }
        
        using (new EditorGUI.DisabledScope(originalWeaponModel == null && weaponModel == null))
        {
            weaponModel = EditorGUILayout.ObjectField("Weapon Model", weaponModel, typeof(GameObject), true, options) as GameObject;
        }
        
        using (new EditorGUI.DisabledScope(weaponModel == null))
        {
            weaponType = (WeaponType)EditorGUILayout.EnumPopup("Weapon Type", weaponType);
        }

        using (new EditorGUI.DisabledScope(weaponModel == null))
        {
            weaponAbility = (WeaponAbility)EditorGUILayout.EnumPopup("Weapon Ability", weaponAbility);
        }
        
        DemiModBase.AddLineAndSpace();
        
        #region Mod Process

        using (new EditorGUI.DisabledScope(originalWeaponModel == null && weaponModel == null && weaponToModRoot == null))
        {
            if (GUILayout.Button("Start Mod Process", GUILayout.Height(20)))
            {
                SetupWeapon();
            }
        }

        #endregion
        
        
        DemiModBase.AddLineAndSpace();
        
        
        //weaponToMod = EditorGUILayout.ObjectField("Weapon to Mod", weaponToMod, typeof(GameObject), true) as GameObject;
        weaponToModRoot = EditorGUILayout.ObjectField("Weapon Root", weaponToModRoot, typeof(GameObject), true) as GameObject;
        
        if(weaponType != lastWeaponType)
        {
            //UpdateWeaponType();
        }

        if (originalWeaponModel != null && lastOriginalWeaponModel != null)
        {
            if (originalWeaponModel != lastOriginalWeaponModel)
            {
                Debug.Log("Original Weapon Model has changed. Resetting all data.");
                ResetAllData(false);
            }
        }
        
        
        if (weaponAbility != lastWeaponAbility)
        {
            //Debug.Log("Weapon Ability has changed. Resetting all data.");
           // UpdateWeaponAbility();
        }
        
        
        lastOriginalWeaponModel = originalWeaponModel;
        lastWeaponType = weaponType;
        lastWeaponAbility = weaponAbility;
        
        
        DemiModBase.AddLineAndSpace();
        
        
        /*
        using(new EditorGUI.DisabledScope(weaponToModRoot == null))
        {
            if (GUILayout.Button("Auto-Add Colliders", GUILayout.Height(20)))
            {
                AutoAddColliders();
            }
        }
        */
        
        
        using(new EditorGUI.DisabledScope(weaponToModRoot == null))
        {
            if (GUILayout.Button("Create Weapon Handle", GUILayout.Height(20)))
            {
                AddWeaponHandle();
            }
        }
        
        using(new EditorGUI.DisabledScope(weaponModScript == null))
        {
            if (GUILayout.Button("Create Stabber", GUILayout.Height(20)))
            {
                CreateStabber();
            }
        }
        
        using(new EditorGUI.DisabledScope(weaponModScript == null))
        {
            if (GUILayout.Button("Create Arc Wave Tracker", GUILayout.Height(20)))
            {
                CreateArcWaveTracker();
            }
        }

        using(new EditorGUI.DisabledScope((weaponModScript == null || weaponModScript.damageCollider == null) || weaponType == WeaponType.None))
        {
            if (GUILayout.Button("Copy Premade Stats From Weapon Type", GUILayout.Height(20)))
            {
                // Add confirmation window.
                if(EditorUtility.DisplayDialog("Copy Premade Stats from " + weaponType,
                       "This will overwrite any changes you have made to the WeaponMod stats.", "OK", "Cancel"))
                {
                    CopyWeaponStatsFromWeaponType();
                }
            }
        }
        
        
        DemiModBase.AddLineAndSpace();

        #region SwitchPlatforms

        //currentScene = EditorGUILayout.ObjectField("Current Scene", currentScene, typeof(SceneInstance), true) as SceneInstance;
        DemiModBase.AddLineAndSpace();

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
        
        using (new EditorGUI.DisabledScope(finalPrefab == null))
        {
            finalPrefab = EditorGUILayout.ObjectField("Final Prefab", finalPrefab, typeof(GameObject), true) as GameObject;
        }
        

        #region Build Addressables

        bool canBuild = weaponToModRoot != null;

        using (new EditorGUI.DisabledScope(!canBuild))
        {
            GUILayout.BeginHorizontal("Build the Mods", GUI.skin.window);
            
            if (GUILayout.Button("Build for Windows (PCVR)", GUILayout.Height(20)))
            {
                SaveWeaponModPrefab();
                
                GetDataHolder();

                if (originalWeaponModel)
                {
                    dataHolder.lastWeaponModel = originalWeaponModel;
                    dataHolder.lastWeaponModelName = originalWeaponModel.name;
                }
                
                if(weaponToModRoot)
                {
                    dataHolder.lastWeaponModRoot = weaponToModRoot;
                    dataHolder.lastWeaponModScript = weaponModScript;
                    dataHolder.lastWeaponModRootName = weaponToModRoot.name;
                    
                    Debug.Log("Data Holder Weapon Mod Root: " + dataHolder.lastWeaponModRoot.name);
                }
                
                dataHolder.lastWeaponFinalPrefab = finalPrefab;
                
                AssetDatabase.Refresh();
                EditorUtility.SetDirty(dataHolder);
                AssetDatabase.SaveAssets();
                
                
                DemiModBase.ExportWindows(DemiModBase.ModType.Weapon, weaponToModRoot);

                PostBuildCleanup();

                EditorApplication.delayCall += () =>
                {
                    OpenFolderAfterModsBuild();
                };
            }

            if (GUILayout.Button("Build for Android (Quest)", GUILayout.Height(20)))
            {
                SaveWeaponModPrefab();
                
                GetDataHolder();
                
                if (originalWeaponModel)
                {
                    dataHolder.lastWeaponModel = originalWeaponModel;
                    dataHolder.lastWeaponModelName = originalWeaponModel.name;
                }
                
                if(weaponToModRoot)
                {
                    dataHolder.lastWeaponModRoot = weaponToModRoot;
                    dataHolder.lastWeaponModScript = weaponModScript;
                    dataHolder.lastWeaponModRootName = weaponToModRoot.name;
                    
                    Debug.Log("Data Holder Weapon Mod Root: " + dataHolder.lastWeaponModRoot.name);
                }
                
                dataHolder.lastWeaponFinalPrefab = finalPrefab;
                
                AssetDatabase.Refresh();
                EditorUtility.SetDirty(dataHolder);
                AssetDatabase.SaveAssets();
                
                
                DemiModBase.ExportAndroid(DemiModBase.ModType.Weapon, weaponToModRoot);

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
        
        DemiModBase.AddLineAndSpace();
        
        
        // Save the enemy prefab.
        using (new EditorGUI.DisabledScope(weaponToModRoot == null))
        {
            if (GUILayout.Button("Save Weapon Prefab", GUILayout.Height(20)))
            {
                SaveWeaponModPrefab();
            }
        }
        
        
        using (new EditorGUI.DisabledScope(weaponToModRoot == null))
        {
            if (GUILayout.Button("Enable Debug Shapes", GUILayout.Height(20)))
            {
                EnableDebugShapes();
            }
            
            if (GUILayout.Button("Disable Debug Shapes", GUILayout.Height(20)))
            {
                DisableDebugShapes();
            }
        }
        
        
        
        #region Clear Data
        
        EditorGUILayout.HelpBox("Reset all data for this Mod Exporter Tab.", MessageType.Info);
        GUI.color = Color.red;
        if (GUILayout.Button("Clear Mod Exporter", GUILayout.Height(20)))
        {
            ResetAllData();
        }
        
        GUI.color = Color.white;
        
        #endregion
        
        DemiModBase.AddLineAndSpace();

        // End the scroll view that we began above.
        EditorGUILayout.EndScrollView();
    }


    #region Weapon Setup Process
    
    
    private void SetupWeapon()
    {
        FindExistingComponents();
        
        CreateCopiesFromOriginalModel();
        SetupWeaponModRoot();
        SetupScaler();
        SetupInfusionMesh();
        
        AddWeaponComponents(weaponToModRoot);
        
        UpdateWeaponAbility();
    }

    public void FindExistingComponents()
    {
        if(originalWeaponModel)
        {
            if (originalWeaponModel.TryGetComponent(out WeaponMod weaponMod))
            {
                // This is the root of the weapon but placed into the originalWeaponModel.
                weaponToModRoot = originalWeaponModel;

                if (weaponMod.weaponModel)
                {
                    weaponModel = weaponMod.weaponModel.gameObject;
                }

                originalWeaponModel = null;
            }
        }
        
        
        if (!weaponToModRoot && weaponModel)
        {
            weaponToModRoot = weaponModel.transform.root.gameObject;
        }
        
        if (weaponToModRoot)
        {
            if(!weaponModScript)
            {
                weaponModScript = weaponToModRoot.GetComponent<WeaponMod>();
            }
            
            if(!scaler)
            {
                if(weaponToModRoot.transform.Find("Scaler"))
                {
                    scaler = weaponToModRoot.transform.Find("Scaler").gameObject;
                }
            }
            
            if (!weaponModel)
            {
                if (weaponModScript)
                {
                    if (weaponModScript.weaponModel)
                    {
                        weaponModel = weaponModScript.weaponModel.gameObject;
                    }
                }
            }

            if (!weaponModel)
            {
                // Find weaponModel in children of weaponToModRoot
                WeaponModel weaponModelScript = weaponToModRoot.GetComponentInChildren<WeaponModel>();
                if (weaponModelScript)
                {
                    weaponModel = weaponModelScript.gameObject;
                }
            }
            
            
            Debug.Log("Weapon Model Name: " + weaponModName);
            
            if (!weaponModel)
            {
                if(string.IsNullOrEmpty(weaponModName))
                    return;
                
                if(scaler)
                {
                    foreach (var child in scaler.transform.GetComponentsInChildren<Transform>())
                    {
                        if (child.name.Contains(weaponModName))
                        {
                            Debug.Log("Found WeaponMod: " + child.name);
                            weaponModel = child.gameObject;
                            break;
                        }
                    }
                }
            }
        }
    }

    private void CreateCopiesFromOriginalModel()
    {
        
        if(!weaponModel && originalWeaponModel)
        {
            weaponModel = Instantiate(originalWeaponModel);
            
            if(weaponModel.TryGetComponent(out WeaponModel weaponModelScript) == false)
            {
                weaponModelScript = weaponModel.AddComponent<WeaponModel>();
            }

            
            //meshesToAutoAddColliders = weaponToMod.GetComponentsInChildren<MeshFilter>().Select(mf => mf.gameObject).ToList();

            if (PrefabUtility.IsPartOfPrefabInstance(originalWeaponModel))
            {
                originalWeaponModel.SetActive(false);

                Debug.Log("Original Weapon Model is a prefab instance. Setting to inactive.");
            }
            else
            {
                originalWeaponModel.SetActive(false);
                Debug.Log("Original Weapon Model is not a prefab instance.");
            }
        }
        
        
        if(originalWeaponModel)
        {
            if(originalWeaponModel.name.Contains("(Clone)"))
            {
                originalWeaponModel.name = originalWeaponModel.name.Replace("(Clone)", "");
            }
            
            weaponModName = originalWeaponModel.name;
        }
        else if(weaponModel)
        {
            if(weaponModel.name.Contains("(Clone)"))
            {
                weaponModel.name = weaponModel.name.Replace("(Clone)", "");
            }
            
            weaponModName = weaponModel.name;
        }
        else if(weaponToModRoot)
        {
            weaponModName = weaponToModRoot.name;
        }
        
        if(weaponModName.Contains("(Clone)"))
        {
            weaponModName = weaponModName.Replace("(Clone)", "");
        }
    }

    private void SetupWeaponModRoot()
    {
        if (weaponModel)
        {
            if (weaponModel.TryGetComponent(out WeaponMod weaponMod))
            {
                // This is the root of the weapon.
                weaponToModRoot = weaponModel;
            }
        }

        if (!weaponToModRoot)
        {
            // Search object for name
            weaponToModRoot = GameObject.Find(weaponModName + " Root");
        }
        
        if(!weaponToModRoot)
        {
            // Create a root for the weapon to be parented to.
            weaponToModRoot = new GameObject(weaponModel.name + " Root");
            weaponToModRoot.transform.position = weaponModel.transform.position;
            weaponToModRoot.transform.rotation = weaponModel.transform.rotation;
        }
        
        weaponToModRoot.name = weaponModName + " Root";
    }
    
    private void SetupScaler()
    {
        if(!scaler)
        {
            if(weaponToModRoot.transform.Find("Scaler"))
            {
                scaler = weaponToModRoot.transform.Find("Scaler").gameObject;
            }
        }
        
        if(!scaler)
        {
            scaler = new GameObject("Scaler");
            scaler.transform.SetParent(weaponToModRoot.transform);
            scaler.transform.localPosition = Vector3.zero;
            scaler.transform.localRotation = Quaternion.identity;
            scaler.transform.localScale = Vector3.one;
        }
        
        if (weaponModel && weaponModel.transform.parent != scaler.transform)
        {
            weaponModel.transform.SetParent(scaler.transform);
        }
    }

    private void SetupInfusionMesh()
    {
        if(!infusionMesh)
        {
            // Try to find the infusion mesh in the weaponToModRoot by searching for the name on each object.
            foreach (var child in scaler.transform.GetComponentsInChildren<Transform>(true))
            {
                if(child.name.Contains("Infusion Mesh"))
                {
                    infusionMesh = child.gameObject;
                    Debug.Log("Found Infusion Mesh: " + infusionMesh.name);
                    break;
                }
            }
        }
        
        if(!infusionMesh)
            infusionMesh = Instantiate(originalWeaponModel);
        
        if(infusionMesh && scaler)
        {
            infusionMesh.name = weaponModName + " Infusion Mesh";
            
            infusionMesh.transform.SetParent(scaler.transform);
            
            // Remove all colliders or components that are not needed for the infusion mesh.
            Collider[] colliders = infusionMesh.GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                DestroyImmediate(col);
            }
            
            Rigidbody[] rigidbodies = infusionMesh.GetComponentsInChildren<Rigidbody>();
            foreach (var rb in rigidbodies)
            {
                DestroyImmediate(rb);
            }
        }
        
        infusionMesh.SetActive(false);
    }
    
    
    private void AddWeaponComponents(GameObject objectToAddComponentsTo)
    {
        if (objectToAddComponentsTo)
        {
            Rigidbody rb;
            DamageCollider dc;
            GrabbableMod grabbable;
            
            if (objectToAddComponentsTo.TryGetComponent(out weaponModScript) == false)
            {
                weaponModScript = objectToAddComponentsTo.AddComponent<WeaponMod>();
            }
            
            if(infusionMesh)
            {
                weaponModScript.infusionMesh = infusionMesh;
            }
            
            if(objectToAddComponentsTo.TryGetComponent(out rb) == false)
            {
                rb = objectToAddComponentsTo.AddComponent<Rigidbody>();

            }
            
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            
            
            if(objectToAddComponentsTo.TryGetComponent(out dc) == false)
            {
                dc = objectToAddComponentsTo.AddComponent<DamageCollider>();
            }
            
            if(rb)
                dc.thisRigidbody = rb;
            
            
            
            if(!dc.defaultDamageColliderStats)
            {
                AssetDatabase.CreateFolder("Assets", "Weapon Stats");
                
                
                DamageColliderStats stats = ScriptableObject.CreateInstance<DamageColliderStats>();
                
                
                AssetDatabase.CreateAsset(stats, "Assets/Resources/Weapon Stats/" + weaponModName + ".asset");
                AssetDatabase.SaveAssets();
                
                
                dc.defaultDamageColliderStats = stats;
            }
            
            
            
            if(infusionMesh)
            {
                dc.weaponInfusion = infusionMesh;
            }
            
            if(objectToAddComponentsTo.TryGetComponent(out grabbable) == false)
            {
                grabbable = objectToAddComponentsTo.AddComponent<GrabbableMod>();
            }

            grabbable.Stationary = false;
            
            
            if(weaponModScript.colliders == null)
            {
                weaponModScript.colliders = new List<Collider>();
            }

            if (weaponModScript.colliders.Count <= 0)
            {
                Debug.Log("Finding colliders for " + objectToAddComponentsTo.name);
                weaponModScript.colliders = objectToAddComponentsTo.GetComponentsInChildren<Collider>().ToList();
            }

            if (weaponModScript.colliders.Count > 0)
            {
                // Search for any null colliders and remove them.
                weaponModScript.colliders.RemoveAll(item => item == null);
                
                foreach (var col in weaponModScript.colliders)
                {
                    if (col.gameObject != weaponToModRoot)
                    {
                        if (col.gameObject.TryGetComponent(out GrabbableModChild grabbableModChild) == false)
                        {
                            grabbableModChild = col.gameObject.AddComponent<GrabbableModChild>();
                        }
                        
                        grabbableModChild.ParentGrabbable = grabbable;
                    }
                }
            }

            // Stabbing
            if(weaponType == WeaponType.Sword || weaponType == WeaponType.Kunai 
                || weaponType == WeaponType.CombatKnife || weaponType == WeaponType.ClassicKatana 
                || weaponType == WeaponType.TechKatana || weaponType == WeaponType.Spear)
            {
                weaponModScript.isStabbingWeapon = true;
            }
            else
            {
                //weaponModScript.isStabbingWeapon = false;
            }
            
            if(weaponModScript.isStabbingWeapon)
            {
                CreateStabber();
            }


            if (weaponModScript.arcWaveTracker == null)
            {
                
            }
            
            
            if(weaponModScript.flyingForwardTransform == null)
            {
                GameObject flyingForward = Instantiate(Resources.Load<GameObject>("Pointer Arrow"));
                flyingForward.name = "Flying Forward Transform";
                flyingForward.transform.SetParent(scaler.transform);
                flyingForward.transform.localPosition = Vector3.zero;
                flyingForward.transform.localRotation = Quaternion.identity;
                weaponModScript.flyingForwardTransform = flyingForward.transform;
            }
            
            if(weaponModScript)
            {
                weaponModScript.weaponType = weaponType;
                weaponModScript.Rigidbody = rb;
                weaponModScript.damageCollider = dc;
            }



            // Get all ModPosableGrabPoints and add them to the weaponMod list.
            ModPosableGrabPoint[] posableGrabPoints = objectToAddComponentsTo.GetComponentsInChildren<ModPosableGrabPoint>();
            
            if (weaponModScript.modPosableGrabPoints == null)
            {
                weaponModScript.modPosableGrabPoints = new List<ModPosableGrabPoint>();
            }
            
            weaponModScript.modPosableGrabPoints = posableGrabPoints.ToList();
            
            /*
            if(weaponModScript.modGrabPoints == null)
            {
                GameObject grabPoints = new GameObject("GrabPoints");
                grabPoints.transform.SetParent(scaler.transform);
                grabPoints.transform.localPosition = Vector3.zero;
                grabPoints.transform.localRotation = Quaternion.identity;
                weaponModScript.modGrabPoints = grabPoints.AddComponent<ModGrabPoints>();
            }
            
            
            // Add the grab points to the weapon.
            if(weaponModScript.modPosableGrabPoints.Count == 0)
            {
                GameObject grabPoint = new GameObject("ModPosableGrabPoint");
                grabPoint.transform.SetParent(weaponModScript.modGrabPoints.transform);
                grabPoint.transform.localPosition = Vector3.zero;
                grabPoint.transform.localRotation = Quaternion.identity;
                weaponModScript.modPosableGrabPoints.Add(grabPoint.AddComponent<ModPosableGrabPoint>());
            }
            */
        }
    }
    

    #endregion


    private void AddWeaponHandle()
    {
        GameObject weaponHandle = Instantiate(Resources.Load<GameObject>("Weapon Mod Handles/Weapon Handle"));
        
        if(!scaler)
        {
            SetupScaler();
        }
        
        weaponHandle.transform.SetParent(scaler.transform);
        weaponHandle.transform.localPosition = Vector3.zero;
        weaponHandle.transform.localRotation = Quaternion.identity;
        
        GetAllGrabPoints();
    }
    
    
    // Auto-add colliders
    private void AutoAddColliders()
    {
        for (int i = 0; i < meshesToAutoAddColliders.Count; i++)
        {
            if (meshesToAutoAddColliders[i])
            {
                if(meshesToAutoAddColliders[i].TryGetComponent(out MeshFilter meshFilter))
                {
                    if (meshesToAutoAddColliders[i].TryGetComponent(out Collider collider) == false)
                    {
                        MeshCollider meshCollider = meshesToAutoAddColliders[i].AddComponent<MeshCollider>();
                        
                        Debug.Log("Adding Mesh Collider to: " + meshesToAutoAddColliders[i].name);
                    }
                }
            }
        }
        
        GetAllColliders();
    }

    private void GetAllColliders()
    {
        if(!weaponToModRoot || !weaponModScript)
            return;
        
        // Get all colliders and add them to the weaponMod list.
        Collider[] colliders = weaponToModRoot.GetComponentsInChildren<Collider>();
        

        for (int i = 0; i < weaponModScript.colliders.Count; i++)
        {
            // Check if the collider is null and remove it from the list.
            if (weaponModScript.colliders[i] == null)
            {
                weaponModScript.colliders.RemoveAt(i);
                i--;
            }
        }

        for (int i = 0; i < colliders.Length; i++)
        {
            // Check if the list doesn't already contain the collider.
            if (weaponModScript.colliders.Contains(colliders[i]) == false)
            {
                weaponModScript.colliders.Add(colliders[i]);
                Debug.Log("Adding Collider: " + colliders[i].name);
            }
        }
        
        
        // Check each collider. If it's also in the stabbable colliders, continue. Else add a HVRGrabbableChild component.
        if (weaponModScript.colliders != null)
        {
            for (int i = 0; i < weaponModScript.colliders.Count; i++)
            {
                if (weaponModScript.stabColliders != null)
                {
                    if (weaponModScript.stabColliders.Contains(weaponModScript.colliders[i]))
                    {
                        continue;
                    }
                }
                
                if (weaponModScript.colliders[i].TryGetComponent(out GrabbableModChild GrabbableModChild) == false)
                {
                    GrabbableModChild = weaponModScript.colliders[i].gameObject.AddComponent<GrabbableModChild>();
                }
                
                if(weaponModScript.GetComponent<GrabbableMod>())
                    GrabbableModChild.ParentGrabbable = weaponModScript.GetComponent<GrabbableMod>();
            }
        }
    }

    private void GetAllGrabPoints()
    {
        if(!weaponToModRoot || !weaponModScript)
            return;
        
        // Get all ModPosableGrabPoints and add them to the weaponMod list.
        ModPosableGrabPoint[] posableGrabPoints = weaponToModRoot.GetComponentsInChildren<ModPosableGrabPoint>();
        
        // Add the grab points to the weapon if they don't already exist.
        for (int i = 0; i < weaponModScript.modPosableGrabPoints.Count; i++)
        {
            // Check if the grab point is null and remove it from the list.
            if (weaponModScript.modPosableGrabPoints[i] == null)
            {
                weaponModScript.modPosableGrabPoints.RemoveAt(i);
                i--;
            }
        }

        for (int i = 0; i < posableGrabPoints.Length; i++)
        {
            // Check if the list doesn't already contain the grab point.
            if (weaponModScript.modPosableGrabPoints.Contains(posableGrabPoints[i]) == false)
            {
                weaponModScript.modPosableGrabPoints.Add(posableGrabPoints[i]);
                Debug.Log("Adding Grab Point: " + posableGrabPoints[i].name);
            }
        }
        
        if(weaponModScript.mainGrabPoints == null)
        {
            weaponModScript.mainGrabPoints = new List<ModPosableGrabPoint>();
        }
        
        if(weaponModScript.mainGrabPoints.Count == 0)
        {
            if(weaponModScript.modPosableGrabPoints.Count > 0)
            {
                weaponModScript.mainGrabPoints.Add(weaponModScript.modPosableGrabPoints[0]);
            }
        }
    }
    

    private void CreateStabber()
    {
        if(!weaponModScript)
            return;
        
        weaponModScript.isStabbingWeapon = true;
        
        if(weaponModScript.stabBase == null)
        {
            GameObject stabBase = Instantiate(Resources.Load<GameObject>("Stab Base"));
            stabBase.transform.SetParent(scaler.transform);
            stabBase.transform.localPosition = Vector3.zero;
            stabBase.transform.localRotation = Quaternion.identity;
            weaponModScript.stabBase = stabBase.transform;
        }
                
        if(weaponModScript.stabTip == null)
        {
            GameObject stabTip = Instantiate(Resources.Load<GameObject>("Stab Tip"));
            stabTip.transform.SetParent(scaler.transform);
            stabTip.transform.localPosition = Vector3.zero;
            stabTip.transform.localRotation = Quaternion.identity;
            weaponModScript.stabTip = stabTip.transform;
        }
    }


    private void CreateArcWaveTracker()
    {
        if(!weaponModScript)
            return;
        
        weaponModScript.isArcWaveWeapon = true;
        
        if(weaponModScript.arcWaveTracker == null)
        {
            GameObject arcWaveTracker = Instantiate(Resources.Load<GameObject>("Stab Base"));
            
            arcWaveTracker.name = "Arc Wave Tracker";
            
            arcWaveTracker.transform.SetParent(scaler.transform);
            arcWaveTracker.transform.localPosition = Vector3.zero;
            arcWaveTracker.transform.localRotation = Quaternion.identity;
            weaponModScript.arcWaveTracker = arcWaveTracker.transform;
        }
    }
    
    
    private void UpdateWeaponType()
    {
        if(weaponToModRoot)
        {
            if(weaponToModRoot.TryGetComponent(out WeaponMod weaponMod))
            {
                weaponMod.weaponType = weaponType;
                
                if (weaponMod.damageCollider)
                {
                    weaponMod.damageCollider.weaponType = weaponType;
                }
                
                AddWeaponComponents(weaponToModRoot);
            }
        }
    }


    private void CopyWeaponStatsFromWeaponType()
    {
        if(!weaponModScript || !weaponModScript.damageCollider)
            return;

        switch (weaponType)
        {
            case WeaponType.Sword:
                weaponModScript.damageCollider.defaultDamageColliderStats = Resources.Load<DamageColliderStats>("Weapon Stats/Premade Weapon Stats/Sword");
                break;
            
            case WeaponType.Shield:
                weaponModScript.damageCollider.defaultDamageColliderStats = Resources.Load<DamageColliderStats>("Weapon Stats/Premade Weapon Stats/Shield");
                break;
            
            case WeaponType.Kunai:
                weaponModScript.damageCollider.defaultDamageColliderStats = Resources.Load<DamageColliderStats>("Weapon Stats/Premade Weapon Stats/CombatKnife");
                break;
            
            case WeaponType.CombatKnife:
                weaponModScript.damageCollider.defaultDamageColliderStats = Resources.Load<DamageColliderStats>("Weapon Stats/Premade Weapon Stats/CombatKnife");
                break;
            
            case WeaponType.ClassicKatana:
                weaponModScript.damageCollider.defaultDamageColliderStats = Resources.Load<DamageColliderStats>("Weapon Stats/Premade Weapon Stats/Sword");
                break;
            
            case WeaponType.TechKatana:
                weaponModScript.damageCollider.defaultDamageColliderStats = Resources.Load<DamageColliderStats>("Weapon Stats/Premade Weapon Stats/Sword");
                break;
            
            case WeaponType.WarHammer:
                weaponModScript.damageCollider.defaultDamageColliderStats = Resources.Load<DamageColliderStats>("Weapon Stats/Premade Weapon Stats/WarHammer");
                break;
            
            case WeaponType.LightHammer:
                weaponModScript.damageCollider.defaultDamageColliderStats = Resources.Load<DamageColliderStats>("Weapon Stats/Premade Weapon Stats/LightHammer");
                break;
            
            case WeaponType.HeroBaton:
                weaponModScript.damageCollider.defaultDamageColliderStats = Resources.Load<DamageColliderStats>("Weapon Stats/Premade Weapon Stats/HeroBaton");
                break;
            
            case WeaponType.Pistol:
                return;
                break;
            
            case WeaponType.DemigodPistol:
                return;
                break;
            
            case WeaponType.AssaultRifle:
                return;
            
            case WeaponType.Bow:
                return;
            
            case WeaponType.DemigodBow:
                return;
            
            case WeaponType.Staff:
                weaponModScript.damageCollider.defaultDamageColliderStats = Resources.Load<DamageColliderStats>("Weapon Stats/Premade Weapon Stats/Staff");
                break;
            
            case WeaponType.Spear:
                weaponModScript.damageCollider.defaultDamageColliderStats = Resources.Load<DamageColliderStats>("Weapon Stats/Premade Weapon Stats/Spear");
                break;
            
            case WeaponType.Grenade:
                return;
            
            case WeaponType.Unused:
                return;
        }

        if (weaponModScript.damageCollider.defaultDamageColliderStats)
        {
            weaponModScript.UnpinOnHit = weaponModScript.damageCollider.defaultDamageColliderStats.UnpinOnHit;
            weaponModScript.UnpinPuppetAmount = weaponModScript.damageCollider.defaultDamageColliderStats.UnpinPuppetAmount;
            
            weaponModScript.ForceToApplyToPuppet = weaponModScript.damageCollider.defaultDamageColliderStats.ForceToApplyToPuppet;
            weaponModScript.MaximumForceToApplyToPuppet = weaponModScript.damageCollider.defaultDamageColliderStats.MaximumForceToApplyToPuppet;
            
            weaponModScript.AddForceMultiplier = weaponModScript.damageCollider.defaultDamageColliderStats.AddForceMultiplier;
            weaponModScript.ForceMultiplier = weaponModScript.damageCollider.defaultDamageColliderStats.ForceMultiplier;
            
            weaponModScript.doesAdditionalDamage = weaponModScript.damageCollider.defaultDamageColliderStats.doesAdditionalDamage;
            weaponModScript.additionalDamage = weaponModScript.damageCollider.defaultDamageColliderStats.additionalDamage;
            
            weaponModScript.DestructibleDamageMultiplier = weaponModScript.damageCollider.defaultDamageColliderStats.DestructibleDamageMultiplier;
            
            weaponModScript.ForceEnemyDamageReaction = weaponModScript.damageCollider.defaultDamageColliderStats.ForceEnemyDamageReaction;
            weaponModScript.ForceEnemyDamageReactionInt = weaponModScript.damageCollider.defaultDamageColliderStats.ForceEnemyDamageReactionInt;
        }
    }
    
    

    private void UpdateWeaponAbility()
    {
        // Turn off all abilities and then turn on the selected ability.
        
        if(weaponModScript)
        {
            weaponModScript.weaponAbility = weaponAbility;
        }
        
        
        
        switch (weaponAbility)
        {
            case WeaponAbility.ArcWave:
                CreateArcWaveTracker();
                break;
            
            case WeaponAbility.StabAnything:
                CreateStabber();
                break;
            
            case WeaponAbility.Teleport:
                break;
            
            case WeaponAbility.Recall:
                break;
            
            case WeaponAbility.BounceAndRecall:
                break;
            
            case WeaponAbility.Force:
                break;
        }
    }
    

    // If finalPrefab already exists, save the weaponModRoot changes to finalPrefab.
    // Otherwise, save the weaponModRoot as a new prefab.
    public void SaveWeaponModPrefab()
    {
        DisableDebugShapes();
        GetAllGrabPoints();
        GetAllColliders();
        
        if (finalPrefab)
        {
            Debug.Log("Saving Weapon Mod Prefab: " + finalPrefab.name);
            PrefabUtility.ApplyPrefabInstance(weaponToModRoot, InteractionMode.UserAction);
        }
        else if(weaponToModRoot)
        {
            if(PrefabUtility.IsPartOfRegularPrefab(weaponToModRoot))
            {
                Debug.Log("Saving Weapon Mod Prefab: " + weaponToModRoot.name);
                PrefabUtility.ApplyPrefabInstance(weaponToModRoot, InteractionMode.UserAction);
                
                if(!finalPrefab)
                    finalPrefab = PrefabUtility.GetCorrespondingObjectFromSource(weaponToModRoot);
            }
            else
            {
                if (weaponModName.Contains("(Clone"))
                {
                    weaponModName = weaponModName.Replace("(Clone)", "");
                }
                
                Debug.Log("Saving Weapon Mod Prefab as a Prefab Asset and Connecting: " + weaponModName);
                finalPrefab = PrefabUtility.SaveAsPrefabAssetAndConnect(weaponToModRoot, 
                    DemiModBase.GetOrCreateModPath(DemiModBase.ModType.Weapon, weaponModName) + ".prefab", InteractionMode.UserAction);
            }
        }
        
        DemiModBase.GetOrCreateModPath(DemiModBase.ModType.Weapon, weaponModName);
    }
    
    
    
    private void ResetAllData(bool includeOriginalModel = true)
    {
        FolderSetupComplete = false;
        
        weaponModel = null;
        weaponToModRoot = null;
        finalPrefab = null;
        
        infusionMesh = null;
        scaler = null;
        weaponModScript = null;
        
        weaponModName = "";
        
        if (includeOriginalModel)
            originalWeaponModel = null;
        
        lastOriginalWeaponModel = null;
        lastWeaponType = WeaponType.None;
        lastWeaponToModRoot = null;

        weaponType = WeaponType.None;
        weaponAbility = WeaponAbility.None;
    }


    public void EnableDebugShapes()
    {
        if(!weaponToModRoot)
            return;
        
        // Find all debug shapes in the weapon's hierarchy and enable them.
        DebugShape[] debugShapes = weaponToModRoot.GetComponentsInChildren<DebugShape>();
        
        foreach (var shape in debugShapes)
        {
            if(shape)
                shape.EnableShapes();
        }
    }

    public void DisableDebugShapes()
    {
        if(!weaponToModRoot)
            return;
        
        // Find all debug shapes in the weapon's hierarchy and disable them.
        DebugShape[] debugShapes = weaponToModRoot.GetComponentsInChildren<DebugShape>();
        
        foreach (var shape in debugShapes)
        {
            if(shape)
                shape.DisableShapes();
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

    public void PostBuildCleanup()
    {
        GetDataHolder();
        
        GetCurrentWeapon();
    }
    
    public void GetCurrentWeapon()
    {
        if (!finalPrefab && dataHolder.lastWeaponFinalPrefab)
        {
            finalPrefab = dataHolder.lastWeaponFinalPrefab;
        }

        if (!finalPrefab)
        {
            if (weaponToModRoot)
            {
                finalPrefab = PrefabUtility.GetCorrespondingObjectFromSource(weaponToModRoot);
            }
        }
        
        if(dataHolder.lastWeaponModRoot)
        {
            weaponToModRoot = dataHolder.lastWeaponModRoot;
        }
        else
        {
            // Try to find using the dataHolder lastWeaponModRootName
            if(dataHolder.lastWeaponModRootName != "")
            {
                weaponToModRoot = GameObject.Find(dataHolder.lastWeaponModRootName);
            }
        }
        
        if(dataHolder.lastWeaponModScript)
        {
            weaponModScript = dataHolder.lastWeaponModScript;
        }
        else if(weaponToModRoot)
        {
            weaponModScript = weaponToModRoot.GetComponent<WeaponMod>();
        }
        
        if(dataHolder.lastWeaponModel)
        {
            originalWeaponModel = dataHolder.lastWeaponModel;
        }
        else
        {
            // Try to find using the dataHolder lastWeaponModelName
            if(dataHolder.lastWeaponModelName != "")
            {
                originalWeaponModel = GameObject.Find(dataHolder.lastWeaponModelName);
            }
        }
    }

    public enum WeaponModType
    {
        MeshReplacement,
        CustomWeapon
    }


    #region Base Code Stuff
    
    public DataHolder dataHolder;
    public bool FolderSetupComplete = false;
    public BuildTarget buildTarget = BuildTarget.StandaloneWindows64;
    float vSbarValue;
    public Vector2 scrollPosition = Vector2.zero;
    
    [MenuItem("Project Demigod/Weapon Mod Exporter")]
    public static void ShowMapWindow()
    {
        GetWindow<ProjectDemiModWeaponExporter>("Weapon Mod Exporter");
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
    
    #endregion
}

#endif