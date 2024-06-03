using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Demigod Events can be used to trigger UnityEvents when certain conditions are met.
/// </summary>
public class DemigodEvent : MonoBehaviour
{
    public DemigodEventType eventType = DemigodEventType.OnTriggerEnter;

    public LayerMask eventLayerMask;
    
    //public ActivationType activationType = ActivationType.Anything;
    
    public List<UnityEvent> unityEvents = new List<UnityEvent>();
    
    private string mainCameraTag = "MainCamera";
    
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
    
    
    private bool IsViableCollider(Collider other)
    {
        if (eventLayerMask == (eventLayerMask | (1 << other.gameObject.layer)))
        {
            if (other.GetComponent<HVRGrabbableBag>())
                return false;
                
            return other.CompareTag(mainCameraTag);
        }

        return false;
    }

    private bool IsViableCollider(Collision other)
    {
        if (eventLayerMask == (eventLayerMask | (1 << other.gameObject.layer)))
        {
            if (other.collider.GetComponent<HVRGrabbableBag>())
                return false;
            
            return other.collider.CompareTag(mainCameraTag);
        }

        return false;
    }
    
}

public enum ActivationType
{
    Anything, Player, LeftHand, RightHand
}

public enum DemigodEventType
{
    OnTriggerEnter, OnTriggerExit, OnTriggerStay,
    OnCollisionEnter, OnCollisionExit, OnCollisionStay,
}