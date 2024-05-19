using UnityEngine;

public class GrabberHelper : MonoBehaviour
{
    public float unpin = 500.0f;
    public float force = 500.0f;
    
    public HVRGrabbable thisGrabbable;
    public HVRStabbable thisStabbable;
    
    public VRPuppet vrPuppet;
    
    public Collider thisCollider;
    
    public EnemyBodyPart bodyPart;
    
    public Rigidbody rb;

    public EnemyComponentReference enemyComponentReference;
}

public enum EnemyBodyPart
{
    Head, Neck, Chest, Spine, Hips, 
    RightArm, RightForearm, RightHand, 
    LeftArm, LeftForearm, LeftHand, 
    RightThigh, RightShin, RightFoot, 
    LeftThigh, LeftShin, LeftFoot,
    Tail, Prop,
    RightWeapon, LeftWeapon, AlternateWeapon, 
    LeftWing, RightWing
}