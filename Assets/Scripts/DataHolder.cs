using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "DataHolder", menuName = "Data Holder")]
public class DataHolder : ScriptableObject
{
    public string userDefinedModsLocation;
    
    public string lastAddressableBuildPath;
    
    // Avatar Mod Data
    public GameObject handPoseCopierGameObject;
    public string handPoseBuildLocation;

    public GameObject lastPlayerAvatarPrefab;
    public string lastPlayerAvatarName;

    
    // Enemy Mod Data
    public GameObject lastEnemyModel;
    public string lastEnemyModelName;
    
    public GameObject lastRagdollRoot;
    public string lastRagdollRootName;
    
    public GameObject lastEnemyAvatarPrefab;
    public string lastEnemyAvatarName;
}
