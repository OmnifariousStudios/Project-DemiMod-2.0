using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu (fileName = "DataHolder", menuName = "Data Holder")]
public class DataHolder : ScriptableObject
{
    public string userDefinedModsLocation;
    
    public string lastAddressableBuildPath;
    
    
    [Header("Avatar Mod Data")]
    
    // Avatar Mod Data
    public GameObject handPoseCopierGameObject;
    public string handPoseBuildLocation;

    public GameObject lastPlayerAvatarPrefab;
    public string lastPlayerAvatarName;

    
    
    [Header("Enemy Mod Data")]
    
    // Enemy Mod Data
    public GameObject lastEnemyModel;
    public string lastEnemyModelName;
    
    public GameObject lastRagdollRoot;
    public string lastRagdollRootName;
    
    [FormerlySerializedAs("lastEnemyAvatarPrefab")] public GameObject lastEnemyAvatarFinalPrefab;
    [FormerlySerializedAs("lastEnemyAvatarName")] public string lastEnemyAvatarRootName;
    
    
    
    [Header("Weapon Mod Data")]
    // Weapon Mod Data
    public GameObject lastWeaponModel;
    public string lastWeaponModelName;
    
    
    public WeaponMod lastWeaponModScript;
    public GameObject lastWeaponModRoot;
    public string lastWeaponModRootName;
    
    public GameObject lastWeaponFinalPrefab;
    public string lastWeaponPrefabName;
    
    
    
    
    public string GetUserDefinedModsLocation()
    {
        if (string.IsNullOrEmpty(userDefinedModsLocation))
        {
            Debug.Log("User Defined Mods Location is empty. Setting to default location: ");

            userDefinedModsLocation = Application.persistentDataPath;
        }

        return userDefinedModsLocation;
    }
}
