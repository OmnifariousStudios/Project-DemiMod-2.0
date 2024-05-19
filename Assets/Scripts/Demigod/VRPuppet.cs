using System.Collections;
using System.Collections.Generic;
using RootMotion.Dynamics;
using UnityEngine;

public class VRPuppet : MonoBehaviour
{
    public PuppetMaster puppetMaster;
    public List<Collider> puppetColliders;
    public List<Rigidbody> puppetRigidbodies;
    
    public GameObject rootObject;
    public EnemyComponentReference enemyComponentReference;
    
    public float initialMuscleSpringAmount;
    public int initialSolverIterationCount;
    public float maxRigidbodyVelocity;
}

