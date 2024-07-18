using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// Demigod Events can be used to trigger UnityEvents when certain conditions are met.
/// </summary>
public class DemigodEvent : MonoBehaviour
{
    [Tooltip("The type of trigger/collision that will activate the UnityEvent.")]
    public DemigodEventType eventType = DemigodEventType.OnTriggerEnter;

    [Tooltip("What kind of object will activate this event.")]
    public ActivationType activationType = ActivationType.Anything;
    
    [Tooltip("The layer mask that this event will trigger on.")]
    public LayerMask eventLayerMask;
    
    [Tooltip("The UnityEvents that will be triggered.")]
    public List<UnityEvent> unityEvents = new List<UnityEvent>();
    
    private string mainCameraTag = "MainCamera";
    private string PlayerTag = "Player";
    private string handTag = "Hand";
    
    private void OnTriggerEnter(Collider other)
    {
        if (eventType == DemigodEventType.OnTriggerEnter)
        {
            if(IsViableCollider(other))
            {
                foreach (UnityEvent unityEvent in unityEvents)
                {
                    unityEvent.Invoke();
                }
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (eventType == DemigodEventType.OnTriggerExit)
        {
            if(IsViableCollider(other))
            {
                foreach (UnityEvent unityEvent in unityEvents)
                {
                    unityEvent.Invoke();
                }
            }
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (eventType == DemigodEventType.OnTriggerStay)
        {
            if(IsViableCollider(other))
            {
                foreach (UnityEvent unityEvent in unityEvents)
                {
                    unityEvent.Invoke();
                }
            }
        }
    }
    
    private void OnCollisionEnter(Collision other)
    {
        if (eventType == DemigodEventType.OnCollisionEnter)
        {
            if(IsViableCollider(other))
            {
                foreach (UnityEvent unityEvent in unityEvents)
                {
                    unityEvent.Invoke();
                }
            }
        }
    }
    
    private void OnCollisionExit(Collision other)
    {
        if (eventType == DemigodEventType.OnCollisionExit)
        {
            if(IsViableCollider(other))
            {
                foreach (UnityEvent unityEvent in unityEvents)
                {
                    unityEvent.Invoke();
                }
            }
        }
    }
    
    private void OnCollisionStay(Collision other)
    {
        if (eventType == DemigodEventType.OnCollisionStay)
        {
            if(IsViableCollider(other))
            {
                foreach (UnityEvent unityEvent in unityEvents)
                {
                    unityEvent.Invoke();
                }
            }
        }
    }
    

    
    // Check Collision collider
    private bool IsViableCollider(Collision other)
    {
        // First, check the layer. If the layer is not in the layer mask, return false.
        if (eventLayerMask == (eventLayerMask | (1 << other.gameObject.layer)))
        {
            
            // If the collider is a grabbable bag, return false. These should not trigger events.
            if (other.collider.GetComponent<HVRGrabbableBag>())
                return false;

            // Check the activation type.
            switch (activationType)
            {
                case ActivationType.Anything:
                    
                    break;
                
                case ActivationType.Player:
                    
                    if(other.transform.root.name.Contains("Player Rig") == false)
                    {
                        return false;
                    }
                    
                    break;
                
                case ActivationType.PlayerHead:
                    if(other.gameObject.name.Contains("Head") == false)
                    {
                        return false;
                    }
                    
                    break;
                
                case ActivationType.PlayerFeet:
                    
                    break;
                
                case ActivationType.AnyHand:
                    if (other.gameObject.layer != LayerMask.NameToLayer("Hand"))
                    {
                        return false;
                    }
                    break;
                
                case ActivationType.LeftHand:
                    if (other.gameObject.layer != LayerMask.NameToLayer("Hand"))
                    {
                        return false;
                    }
                    
                    if(other.collider.name.Contains("Left") == false)
                    {
                        return false;
                    }
                    break;
                
                case ActivationType.RightHand:
                    if (other.gameObject.layer != LayerMask.NameToLayer("Hand"))
                    {
                        return false;
                    }
                    
                    if(other.collider.name.Contains("Right") == false)
                    {
                        return false;
                    }
                    break;
                    
            }
            
            return true;
        }

        return false;
    }
    
    
    // Check Trigger Collider
    private bool IsViableCollider(Collider other)
    {
        if (eventLayerMask == (eventLayerMask | (1 << other.gameObject.layer)))
        {
            // If the collider is a grabbable bag, return false. These should not trigger events.
            if (other.GetComponent<HVRGrabbableBag>())
                return false;
            
            // Check the activation type.
            switch (activationType)
            {
                case ActivationType.Anything:
                    
                    break;
                
                case ActivationType.Player:
                    
                    if(other.transform.root.name.Contains("Player Rig") == false)
                    {
                        return false;
                    }
                    
                    break;
                
                case ActivationType.PlayerHead:
                    if(other.gameObject.tag.Contains(mainCameraTag) == false)
                    {
                        return false;
                    }
                    
                    break;
                
                case ActivationType.PlayerFeet:
                    
                    break;
                
                case ActivationType.AnyHand:
                    if (other.gameObject.layer != LayerMask.NameToLayer("Hand"))
                    {
                        return false;
                    }
                    break;
                
                case ActivationType.LeftHand:
                    if (other.gameObject.layer != LayerMask.NameToLayer("Hand"))
                    {
                        return false;
                    }
                    
                    if(other.name.Contains("Left") == false)
                    {
                        return false;
                    }
                    break;
                
                case ActivationType.RightHand:
                    if (other.gameObject.layer != LayerMask.NameToLayer("Hand"))
                    {
                        return false;
                    }
                    
                    if(other.name.Contains("Right") == false)
                    {
                        return false;
                    }
                    break;
                    
            }

            return true;
        }

        return false;
    }
    
}

[Serializable]
public enum ActivationType
{
    Anything, Player, PlayerHead, PlayerFeet, AnyHand, LeftHand, RightHand
}

public enum DemigodEventType
{
    OnTriggerEnter, OnTriggerExit, OnTriggerStay,
    OnCollisionEnter, OnCollisionExit, OnCollisionStay,
}