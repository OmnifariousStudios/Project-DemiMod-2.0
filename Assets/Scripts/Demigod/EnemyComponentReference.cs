using System.Collections;
using System.Collections.Generic;
using RootMotion.Dynamics;
using UnityEngine;

public class EnemyComponentReference : MonoBehaviour
{
    public EnemyType enemyType;
    
    public Animator anim;
    public PuppetMaster puppetMaster;
    public VRPuppet VRpuppet;
    
    public List<Rigidbody> enemyRigidbodies;
    
    public List<HVRGrabbable> puppetGrabbables;
    public List<HVRStabbable> puppetStabbables;
    public List<GrabberHelper> puppetGrabberHelpers;
    
    public Rigidbody mainEnemyRigidbody;
    
    // Enemy References
    public Transform[] spineBones;
    public Transform headBone;
    public Transform eyes;
    public Transform waistLine;
    public Transform rightPalmForward;
    public Transform leftPalmForward;
    public Transform chestForward;

    public Transform forwardPin;
    public Transform AimTransform;
}


public enum EnemyType
{
    None, AgentMelee, AgentShooterPistol, AgentShooterRifle, Ninja, Robot, Biomutant, GiantDoombot, NA, BasicCombatDrone, 
    StretchAgent, StoneAgent, SteelAgent, Monk1, Monk2, TitanMonster, NA2, Raptor, Zombie, FlyingBlaster, NanoCyborgHeavy
}