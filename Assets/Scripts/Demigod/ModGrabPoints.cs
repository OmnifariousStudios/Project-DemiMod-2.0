using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModGrabPoints : MonoBehaviour
{
    public HVRGrabPoints grabPoints;
    
    public void Awake()
    {
        grabPoints = gameObject.AddComponent<HVRGrabPoints>();
    }
}
