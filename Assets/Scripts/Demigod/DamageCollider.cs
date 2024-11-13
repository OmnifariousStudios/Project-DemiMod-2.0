using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageCollider : MonoBehaviour
{
    public DamageColliderStats defaultDamageColliderStats;
    
    public Rigidbody thisRigidbody;
    
    public bool isWeapon = false;
    public WeaponType weaponType = WeaponType.None;
    public GameObject weaponInfusion;
    public WeaponAbility weaponAbility;
}


public enum WeaponType
{
    None = 0, 
    Sword = 1, 
    Shield = 2, 
    Kunai = 3, 
    CombatKnife, 
    ClassicKatana, 
    TechKatana, 
    WarHammer, 
    LightHammer,
    HeroBaton, 
    Pistol, 
    DemigodPistol,
    AssaultRifle,
    Bow, 
    DemigodBow,
    Staff,
    Spear,
    Grenade,
    Unused
}