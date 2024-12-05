using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModPosableGrabPoint : MonoBehaviour
{
    public HVRPosableGrabPoint HvrPosableGrabPoint;

    public HandPoseGrip handPoseGripToSet = HandPoseGrip.None;
    
    public bool IsForceGrabbable = true;

    [Tooltip("If true only one hand can grab this grabpoint")]
    public bool OneHandOnly;

    [Tooltip("Can the Left hand grab this")]
    public bool LeftHand = true;

    [Tooltip("Can the right hand grab this")]
    public bool RightHand = true;

    [Tooltip("Grab Points in the same group will have pose rotation considered")]
    public int Group = -1;

    [Header("Controller Tracking Offsets")]
    public Vector3 HandRotationOffset;
    public Vector3 HandPositionOffset;
}
