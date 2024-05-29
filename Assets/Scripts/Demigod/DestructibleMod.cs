using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleMod : MonoBehaviour
{
    public float maxHitPoints = 100.0f;
    
    public float velocityReduction = .5f;
    public float ignoreCollisionsUnder = 2f;
    
    public GameObject destroyedPrefab;
}
