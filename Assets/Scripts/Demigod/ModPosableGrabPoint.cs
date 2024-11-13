using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModPosableGrabPoint : MonoBehaviour
{
    public HVRPosableGrabPoint HvrPosableGrabPoint;

    private void Start()
    {
        HvrPosableGrabPoint = gameObject.AddComponent<HVRPosableGrabPoint>();
    }
}
