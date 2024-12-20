using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Damage Collider Stats")]
public class DamageColliderStats : ScriptableObject
{
    [Tooltip("Unpinning means weakening the enemy ragdoll.")]
    public bool UnpinOnHit = true;
    public float UnpinPuppetAmount = 10.0f;
    
    [Tooltip("Force to apply to the ragdoll puppet when this object hits an enemy.")]
    public float ForceToApplyToPuppet = 10.0f;
    public float MaximumForceToApplyToPuppet = 1000.0f;
    
    [Tooltip("Multiplies the overall forces applied when hitting enemies. Keep this low to avoid physics issues.")]
    public bool AddForceMultiplier = false;
    public float ForceMultiplier = 1.0f;
    
    [Tooltip("Adds extra damage for every collision.")]
    public bool doesAdditionalDamage = false;
    public float additionalDamage = 1.0f;

    [Tooltip("Multiplies the damage dealt to destructible objects.")]
    public float DestructibleDamageMultiplier = 1.0f;
    
    [Tooltip("Forces the enemy to react to the collision. Value can be 0, 1, or 2.")]
    public bool ForceEnemyDamageReaction = false;
    public int ForceEnemyDamageReactionInt = 0;
}
