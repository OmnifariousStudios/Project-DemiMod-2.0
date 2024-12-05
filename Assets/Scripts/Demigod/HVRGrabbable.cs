using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HVRGrabbable : MonoBehaviour
{
    public bool Stationary;
    
    public Rigidbody Rigidbody;
}

public enum HandPoseGrip
{
    None,
    Dynamic,
    ClosedFist,
    RelaxedHand,
    WideOpenHand,
    Sphere,
    LargeSphere,
    Sword,
    Shield,
    Knife,
    Kunai,
    ForwardSwordGrip,
    Cylindrical,
    Warhammer,
    Baton,
    PistolGrip,
    SecondaryHandPistolGrip,
    PistolSlideGrip,
    PistolSlideRelease,
    PistolMagazineGrip,
    PistolMagazineRelease,
    HornsHandPose,
    RuntimeCreatedGrabPoint
}