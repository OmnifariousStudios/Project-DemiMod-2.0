#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using UnityEditor.AI;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class ProjectDemiModMapExporter : EditorWindow
{
    string exportPath = "";
    string basePath = "";

    bool showExportSettings = true;
    private bool openAfterExport = true;

    public bool FolderSetupComplete = false;

    public Scene currentScene;
    public string currentSceneName;

    public SceneController currentSceneController;
    public GameObject currentPlayerSpawnpoint;
    [FormerlySerializedAs("currentEnemySpawnpoint")] public GameObject currentEnemySpawnpointsHolder;


    public BuildTarget buildTarget = BuildTarget.StandaloneWindows64;

    float vSbarValue;
    public Vector2 scrollPosition = Vector2.zero;

    
    /// <summary>
    /// Where the built mods will be stored on this PC.
    /// </summary>
    //public string modsFolderPath => Application.persistentDataPath + "/mod.io/04747/data/mods";


    [MenuItem("Project Demigod/Map Mod Exporter")]
    public static void ShowMapWindow()
    {
        GetWindow<ProjectDemiModMapExporter>("Map Mod Exporter");
    }

    private void Awake()
    {
        if (buildTarget == BuildTarget.NoTarget)
            buildTarget = BuildTarget.StandaloneWindows64;
    }


    private void OnGUI()
    {
        EditorGUIUtility.labelWidth = 80;
        GUILayout.Label("Project Demigod Mod Exporter", EditorStyles.largeLabel);
        GUILayout.Space(10);


        GUILayoutOption[] options = { GUILayout.MaxWidth(1000), GUILayout.MinWidth(250) };
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, options);




        #region SwitchPlatforms

        EditorGUILayout.HelpBox("Scene: " + currentSceneName, MessageType.Info);
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
        
        if (GUILayout.Button("Add Enemy Spawnpoint"))
        {
            CreateEnemySpawnPoint();
        }

        
        if (GUILayout.Button("Bake Nav Mesh"))
        {
            NavMeshBuilder.BuildNavMesh();
        }
        
        if (GUILayout.Button("Bake Lighting"))
        {
            Lightmapping.Bake();
        }
        
        if (GUILayout.Button("Bake Occlusion Culling"))
        {
            StaticOcclusionCulling.Compute();
        }

        #endregion

        #region Setup Folders

        using (new EditorGUI.DisabledScope(currentSceneController == null))
        {
            if (GUILayout.Button("Setup Folder Structure", GUILayout.Height(20)))
            {
                //CheckForMapModPath();
                DemiModBase.GetOrCreateModPath(DemiModBase.ModType.Map, currentScene.name);
            }
        }

        #endregion

        #region Shield Wall Renderers

        if(GUILayout.Button("Enable Shield Wall Renderers"))
        {
            EnableShieldWallRenderers();
        }
        
        if (GUILayout.Button("Disable Shield Wall Renderers"))
        {
            DisableShieldWallRenderers();
        }

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


        #region Finish Setup

        EditorGUILayout.HelpBox(" Use this button to Finish Setup AFTER building the Addressable.", MessageType.Info);
        if (GUILayout.Button("Finish Setup", GUILayout.Height(20)))
        {

            DisableInterfaceSpawnPoints();
            DisableShieldWallRenderers();
            DisableEnemySpawnpointShapes();
            
            
            /*
            
            string mapModFolderPath = DemiModBase.GetOrCreateModPath(DemiModBase.ModType.Map, currentScene.name);
            //CheckForMapModPath();

            if (mapModFolderPath == "")
                return;
            

            DirectoryInfo dirInfo = new DirectoryInfo(DemiModBase.modsFolderPath);

            foreach (FileInfo fileInfo in dirInfo.GetFiles())
            {
                Debug.Log("File: " + fileInfo.Name);

                if (fileInfo.Name == "StandaloneWindows64.zip" || fileInfo.Name == "Android.zip")
                    continue;

                File.Move(fileInfo.FullName, Path.Combine(mapModFolderPath, fileInfo.Name));

                Debug.Log("Moved File: " + fileInfo.Name + " to: " + Path.Combine(mapModFolderPath, fileInfo.Name));

                //FileUtil.CopyFileOrDirectory(fileInfo.FullName, Path.Combine(avatarModFolderPath, fileInfo.Name));

            }
            
            */
            
        }

        #endregion


        EditorGUILayout.EndScrollView();
        
        
        //if (openAfterExport)
            //EditorUtility.RevealInFinder(DemiModBase.exportPath);
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
        GameObject shieldWalls = GameObject.Find("Shield Walls");

        if (!shieldWalls)
        {
            shieldWalls = GameObject.Find("Shield Walls(Clone)");
        }

        if (shieldWalls == null)
        {
            GameObject newShieldWalls = Instantiate(Resources.Load("Shield Walls", typeof(GameObject))) as GameObject;
            newShieldWalls.name = "Shield Walls";
            newShieldWalls.transform.position = Vector3.zero;
            newShieldWalls.transform.rotation = Quaternion.identity;
        }
    }

    void EnableShieldWallRenderers()
    {
        GameObject shieldWalls = GameObject.Find("Shield Walls");

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
    }

    private void DisableShieldWallRenderers()
    {
        GameObject shieldWalls = GameObject.Find("Shield Walls");

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
    
    
    //[ContextMenu("Check For Map Mod Path")]
    private string CheckForMapModPath()
    {
        if (currentSceneController == null)
        {
            AddSceneController();
        }
        
        if(currentSceneController == null)
        {
            return "";
        }
        
        string builtModsFolderPath = Path.Combine(Application.persistentDataPath, "mod.io/04747/data/mods/");

        Debug.Log("Checking current scene: " + currentSceneName);
        
        Debug.Log("Setting path to StandaloneTarget: " + EditorUserBuildSettings.selectedStandaloneTarget.ToString());
        string mapModStandaloneWindows64FolderPath = Path.Combine(builtModsFolderPath, Path.Combine(BuildTarget.StandaloneWindows64.ToString(), currentSceneName));
        string mapModAndroidFolderPath = Path.Combine(builtModsFolderPath, Path.Combine(BuildTarget.Android.ToString(), currentSceneName));

        if (Directory.Exists(mapModStandaloneWindows64FolderPath))
        {
            Debug.Log("Mod Folder Build Path already exists: " + mapModStandaloneWindows64FolderPath);
        }
        else
        {
            Debug.Log("Creating Map Mod StandaloneWindows64 Folder in Local Build Path: " + mapModStandaloneWindows64FolderPath);
            Directory.CreateDirectory(mapModStandaloneWindows64FolderPath);
        }

        if (Directory.Exists(mapModAndroidFolderPath))
        {
            Debug.Log("Mod Folder Build Path already exists: " + mapModAndroidFolderPath);
        }
        else
        {
            Debug.Log("Creating Map Mod Android Folder in Local Build Path: " + mapModAndroidFolderPath);
            Directory.CreateDirectory(mapModAndroidFolderPath);
        }
        
        
        FolderSetupComplete = true;

        if (buildTarget == BuildTarget.Android)
        {
            return mapModAndroidFolderPath;
        }
        else
        {
            return mapModStandaloneWindows64FolderPath;
        }
        
    }


    public void OpenFolderAfterModsBuild()
    {
        EditorUtility.RevealInFinder(DemiModBase.exportPath);
    }
}

#endif