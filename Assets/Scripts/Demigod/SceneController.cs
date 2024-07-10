using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    public GameObject playerStartPoint;
    public GameObject enemySpawnpointsHolder;
    
    public Transform avatarCalibratorTransform;
    public Transform levelSelectTransform;
    public Transform playerArmoryTransform;
    public Transform enemySpawnerTransform;
    
    public bool thisSceneUsesUmbraOcclusionCulling = true;
    
    public List<Light> sceneLights;
    
    public Color fogColor = new Color(128, 128, 128);
    public float fogDensity;
    public float fogStart;
    public float fogEnd;
    public FogMode fogMode;
    
    public List<DemigodProp> propsInScene = new List<DemigodProp>();
}
