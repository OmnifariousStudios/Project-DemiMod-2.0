using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRPuppet : MonoBehaviour
{
    public PuppetMaster puppetMaster;
    public List<Collider> puppetColliders;
    public List<Rigidbody> puppetRigidbodies;
    
    public GameObject rootObject;
    public EnemyComponentReference enemyComponentReference;
}
