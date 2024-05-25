using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "DataHolder", menuName = "Data Holder")]
public class DataHolder : ScriptableObject
{
    public string userDefinedModsLocation;
    
    public string lastAddressableBuildPath;
    
    public GameObject handPoseCopierGameObject;
    public string handPoseBuildLocation;

    public GameObject lastPlayerAvatarPrefab;
    public string lastPlayerAvatarName;

    public GameObject lastEnemyAvatarPrefab;
    public string lastEnemyAvatarName;
}
