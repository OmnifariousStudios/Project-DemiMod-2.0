#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.AI;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class ProjectDemiModMapExporter : EditorWindow
{
    public DataHolder dataHolder;
    
    public bool FolderSetupComplete = false;

    public Scene currentScene;
    public string currentSceneName;

    public SceneController currentSceneController;
    public GameObject currentPlayerSpawnpoint;
    [FormerlySerializedAs("currentEnemySpawnpoint")] public GameObject currentEnemySpawnpointsHolder;

    public GameObject shieldWalls;
    bool shieldsAreVisible = false;

    public BuildTarget buildTarget = BuildTarget.StandaloneWindows64;

    float vSbarValue;
    public Vector2 scrollPosition = Vector2.zero;


    [MenuItem("Project Demigod/Map Mod Exporter")]
    public static void ShowMapWindow()
    {
        GetWindow<ProjectDemiModMapExporter>("Map Mod Exporter");
    }

    private void Awake()
    {
        if (buildTarget == BuildTarget.NoTarget)
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
                // save assets
                EditorUtility.SetDirty(dataHolder);
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

        #region Scene Setup


        if (GUILayout.Button("Setup Current Scene"))
        {
            GetCurrentScene();
            AddSceneController();
            AddPlayerSpawnPoint();
            AddEnemySpawnPointHolder();
            AddInterfaceSpawnPoints();
            AddShieldWalls();
            DestroyCamera();
            GetSceneLights();
        }
        
        if (currentSceneName == "")
        {
            GUI.color = Color.red;
        }
        else
        {
            GUI.color = Color.green;
        }
        
        EditorGUILayout.HelpBox("Scene: " + currentSceneName, MessageType.Info);
        
        GUI.color = Color.white;

        
        DemiModBase.AddLineAndSpace();
        
        
        using (new EditorGUI.DisabledScope(currentSceneController == null))
        {
            if (GUILayout.Button("Add Enemy Spawnpoint"))
            {
                CreateEnemySpawnPoint();
            }


            if (GUILayout.Button("Bake Nav Mesh"))
            {
                NavMeshBuilder.BuildNavMesh();
                Debug.Log("Attempting to bake Nav Mesh");
            }

            if (GUILayout.Button("Bake Lighting"))
            {
                Lightmapping.Bake();
                Debug.Log("Attempting to bake Lighting");
            }

            if (GUILayout.Button("Bake Occlusion Culling"))
            {
                StaticOcclusionCulling.Compute();
                Debug.Log("Attempting to bake Occlusion Culling");
            }
        }

        #endregion

        
        DemiModBase.AddLineAndSpace();
        
        
        
        #region Shield Wall Renderers

        if (shieldsAreVisible)
        {
            GUI.color = Color.red;
        }
        else
        {
            GUI.color = Color.green;
        }
        
        
        using (new EditorGUI.DisabledScope(shieldWalls == null))
        {
            if (GUILayout.Button("Enable Shield Wall Renderers"))
            {
                EnableShieldWallRenderers();
            }

            if (shieldsAreVisible)
            {
                GUI.color = Color.green;
            }
            else
            {
                GUI.color = Color.red;
            }

            if (GUILayout.Button("Disable Shield Wall Renderers"))
            {
                DisableShieldWallRenderers();
            }
        }

        GUI.color = Color.white;
        
        #endregion


        #region Build Addressables

        bool canBuild = currentSceneController != null;


        using (new EditorGUI.DisabledScope(!canBuild))
        {
            GUILayout.BeginHorizontal("Build the Mods", GUI.skin.window);
            if (GUILayout.Button("Build for Windows (PCVR)", GUILayout.Height(20)))
            {
                DisableInterfaceSpawnPoints();
                DisableShieldWallRenderers();
                DisableEnemySpawnpointShapes();
                
                // Find and delete and EventSystem in the scene
                GameObject eventSystem = GameObject.Find("EventSystem");
                if (eventSystem)
                {
                    DestroyImmediate(eventSystem);
                }
                
                RecordFogSettings();

                DemiModBase.ExportWindows(DemiModBase.ModType.Map, null);

                DemiModBase.GetOrCreateModPath(DemiModBase.ModType.Map, currentSceneName);


                EditorApplication.delayCall += () =>
                {
                    OpenFolderAfterModsBuild();
                };
            }

            if (GUILayout.Button("Build for Android (Quest)", GUILayout.Height(20)))
            {
                DisableInterfaceSpawnPoints();
                DisableShieldWallRenderers();
                DisableEnemySpawnpointShapes();
                
                // Find and delete and EventSystem in the scene
                GameObject eventSystem = GameObject.Find("EventSystem");
                if (eventSystem)
                {
                    DestroyImmediate(eventSystem);
                }
                
                RecordFogSettings();

                DemiModBase.ExportAndroid(DemiModBase.ModType.Map, null);

                DemiModBase.GetOrCreateModPath(DemiModBase.ModType.Map, currentScene.name);
                //CheckForMapModPath();

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


    public void GetCurrentScene()
    {
        currentScene = EditorSceneManager.GetActiveScene();
        currentSceneName = currentScene.name;

        Debug.Log("Current Scene: " + currentSceneName);
    }

    void AddSceneController()
    {
        GameObject sceneController = GameObject.Find("Scene Controller");

        if (sceneController == null)
        {
            GameObject newSceneController = new GameObject("Scene Controller");
            currentSceneController = newSceneController.AddComponent<SceneController>();
            currentSceneController.thisSceneUsesUmbraOcclusionCulling = true;
        }
        else
        {
            currentSceneController = sceneController.GetComponent<SceneController>();
        }
    }
    
    public void RecordFogSettings()
    {
        if (currentSceneController)
        {
            currentSceneController.fogColor = RenderSettings.fogColor;
            currentSceneController.fogDensity = RenderSettings.fogDensity;
            currentSceneController.fogStart = RenderSettings.fogStartDistance;
            currentSceneController.fogEnd = RenderSettings.fogEndDistance;
            currentSceneController.fogMode = RenderSettings.fogMode;
        }
    }

    void AddPlayerSpawnPoint()
    {
        GameObject playerSpawnPoint = GameObject.Find("Player Spawnpoint");

        if (playerSpawnPoint == null)
        {
            GameObject newPlayerSpawnPoint = new GameObject("Player Spawnpoint");
            newPlayerSpawnPoint.transform.position = Vector3.zero;
            newPlayerSpawnPoint.transform.rotation = Quaternion.identity;

            currentPlayerSpawnpoint = newPlayerSpawnPoint;

            if (currentSceneController)
            {
                currentSceneController.playerStartPoint = currentPlayerSpawnpoint;
            }
        }
        else
        {
            currentPlayerSpawnpoint = playerSpawnPoint;

            if (currentSceneController)
            {
                currentSceneController.playerStartPoint = currentPlayerSpawnpoint;
            }
        }
    }

    void AddEnemySpawnPointHolder()
    {
        GameObject newEnemySpawnPointsHolder = GameObject.Find("Enemy Spawnpoints Holder");

        if (newEnemySpawnPointsHolder == null)
        {
            newEnemySpawnPointsHolder = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            
            newEnemySpawnPointsHolder.GetComponent<CapsuleCollider>().enabled = false;
            
            newEnemySpawnPointsHolder.name = "Enemy Spawnpoints Holder";
            
            newEnemySpawnPointsHolder.transform.position = Vector3.zero;
            newEnemySpawnPointsHolder.transform.rotation = Quaternion.identity;

            currentEnemySpawnpointsHolder = newEnemySpawnPointsHolder;

            if (currentSceneController)
            {
                currentSceneController.enemySpawnpointsHolder = currentEnemySpawnpointsHolder;
            }
            
            CreateEnemySpawnPoint();
        }
        else
        {
            currentEnemySpawnpointsHolder = newEnemySpawnPointsHolder;

            if (currentSceneController)
            {
                currentSceneController.enemySpawnpointsHolder = currentEnemySpawnpointsHolder;
            }
        }
    }

    void CreateEnemySpawnPoint()
    {
        if(currentSceneController == null || currentSceneController.enemySpawnpointsHolder == null)
        {
            GetCurrentScene();
            AddSceneController();
            AddPlayerSpawnPoint();
            AddEnemySpawnPointHolder();
        }
        
        GameObject newEnemySpawnPoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
        
        newEnemySpawnPoint.GetComponent<BoxCollider>().enabled = false;
        newEnemySpawnPoint.name = "Enemy Spawnpoint";
        newEnemySpawnPoint.transform.parent = currentEnemySpawnpointsHolder.transform;
        newEnemySpawnPoint.transform.localPosition = Vector3.zero;
        newEnemySpawnPoint.transform.localRotation = Quaternion.identity;
    }


    void AddInterfaceSpawnPoints()
    {
        GameObject avatarCalibratorShape = GameObject.Find("Avatar Calibrator Shape");
        
        GameObject levelSelectShape = GameObject.Find("Level Select Shape");
        
        GameObject playerArmoryShape = GameObject.Find("Player Armory Shape");
        
        GameObject enemySpawnerShape = GameObject.Find("Enemy Spawn Controller Shape");
        
        if(!avatarCalibratorShape)
        {
            avatarCalibratorShape = Instantiate(Resources.Load("Avatar Calibrator Shape", typeof(GameObject))) as GameObject;
            avatarCalibratorShape.name = "Avatar Calibrator Shape";
        }
        
        if(!levelSelectShape)
        {
            levelSelectShape = Instantiate(Resources.Load("Level Select Shape", typeof(GameObject))) as GameObject;
            levelSelectShape.name = "Level Select Shape";
        }
    
        if(!playerArmoryShape)
        {
            playerArmoryShape = Instantiate(Resources.Load("Player Armory Shape", typeof(GameObject))) as GameObject;
            playerArmoryShape.name = "Player Armory Shape";
        }
        
        if(!enemySpawnerShape)
        {
            enemySpawnerShape = Instantiate(Resources.Load("Enemy Spawn Controller Shape", typeof(GameObject))) as GameObject;
            enemySpawnerShape.name = "Enemy Spawn Controller Shape";
        }

        if (!currentSceneController)
            AddSceneController();

        if (currentSceneController)
        {
            if (avatarCalibratorShape)
                currentSceneController.avatarCalibratorTransform = avatarCalibratorShape.transform;

            if (levelSelectShape)
                currentSceneController.levelSelectTransform = levelSelectShape.transform;

            if (playerArmoryShape)
                currentSceneController.playerArmoryTransform = playerArmoryShape.transform;

            if (enemySpawnerShape)
                currentSceneController.enemySpawnerTransform = enemySpawnerShape.transform;
        }
    }

    void DisableInterfaceSpawnPoints()
    {
        if (currentSceneController)
        {
            if (currentSceneController.avatarCalibratorTransform)
                currentSceneController.avatarCalibratorTransform.gameObject.SetActive(false);

            if (currentSceneController.levelSelectTransform)
                currentSceneController.levelSelectTransform.gameObject.SetActive(false);

            if (currentSceneController.playerArmoryTransform)
                currentSceneController.playerArmoryTransform.gameObject.SetActive(false);

            if (currentSceneController.enemySpawnerTransform)
                currentSceneController.enemySpawnerTransform.gameObject.SetActive(false);
        }
    }

    void AddShieldWalls()
    {
        shieldWalls = GameObject.Find("Shield Walls");

        if (!shieldWalls)
        {
            shieldWalls = GameObject.Find("Shield Walls(Clone)");
        }

        if (shieldWalls == null)
        {
            shieldWalls = Instantiate(Resources.Load("Shield Walls", typeof(GameObject))) as GameObject;
            shieldWalls.name = "Shield Walls";
            shieldWalls.transform.position = Vector3.zero;
            shieldWalls.transform.rotation = Quaternion.identity;
        }
        
        shieldsAreVisible = true;
    }

    void EnableShieldWallRenderers()
    {
        if(!shieldWalls)
            shieldWalls = GameObject.Find("Shield Walls");

        if (!shieldWalls)
        {
            shieldWalls = GameObject.Find("Shield Walls(Clone)");
        }
        
        if (shieldWalls)
        {
            foreach (var VARIABLE in shieldWalls.GetComponentsInChildren<Renderer>())
            {
                VARIABLE.enabled = true;
            }
        }
        
        shieldsAreVisible = true;
    }

    private void DisableShieldWallRenderers()
    {
        if (!shieldWalls)
            shieldWalls = GameObject.Find("Shield Walls");

        if (!shieldWalls)
        {
            shieldWalls = GameObject.Find("Shield Walls(Clone)");
        }
        
        if (shieldWalls)
        {
            foreach (var VARIABLE in shieldWalls.GetComponentsInChildren<Renderer>())
            {
                VARIABLE.enabled = false;
            }
        }
        
        shieldsAreVisible = false;
    }

    private void DisableEnemySpawnpointShapes()
    {
        if (currentSceneController == null || currentSceneController.enemySpawnpointsHolder == null)
        {
            return;
        }
        else
        {
            currentSceneController.enemySpawnpointsHolder.GetComponent<MeshRenderer>().enabled = false;

            foreach (var childMesh in currentSceneController.enemySpawnpointsHolder.GetComponentsInChildren<MeshRenderer>())
            {
                childMesh.enabled = false;
            }
        }
    }


    void DestroyCamera()
    {
        if (Camera.main)
        {
            DestroyImmediate(Camera.main.gameObject);
        }
        else
        {
            GameObject camera = GameObject.Find("Camera");
            
            if(camera)
                DestroyImmediate(camera);
        }
    }

    void GetSceneLights()
    {
        if (currentSceneController)
        {
            currentSceneController.sceneLights = FindObjectsOfType<Light>().ToList();
        }
    }

    
    private void ResetButtonCompletionStatus()
    {
        FolderSetupComplete = false;
        
        currentSceneName = null;
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

/*
        #region Setup Folders

        using (new EditorGUI.DisabledScope(currentSceneController == null))
        {
            EditorGUILayout.HelpBox("Create folders to store your mod in the project.", MessageType.Info);
            if (GUILayout.Button("Setup Folder Structure", GUILayout.Height(20)))
            {
                DemiModBase.GetOrCreateModPath(DemiModBase.ModType.Map, currentScene.name);
            }
        }

        #endregion
 */

#endif