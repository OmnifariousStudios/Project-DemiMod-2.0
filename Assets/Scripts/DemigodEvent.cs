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
    
    public List<UnityEvent> unityEvents = new List<UnityEvent>();
    
    private void OnTriggerEnter(Collider other)
    {
        if (eventType == DemigodEventType.OnTriggerEnter)
        {
            if (eventLayerMask == (eventLayerMask | (1 << other.gameObject.layer)))
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
            if (eventLayerMask == (eventLayerMask | (1 << other.gameObject.layer)))
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
            if (eventLayerMask == (eventLayerMask | (1 << other.gameObject.layer)))
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
            if (eventLayerMask == (eventLayerMask | (1 << other.gameObject.layer)))
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
            if (eventLayerMask == (eventLayerMask | (1 << other.gameObject.layer)))
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
            if (eventLayerMask == (eventLayerMask | (1 << other.gameObject.layer)))
            {
                foreach (UnityEvent unityEvent in unityEvents)
                {
                    unityEvent.Invoke();
                }
            }
        }
    }
}

public enum DemigodEventType
{
    OnTriggerEnter, OnTriggerExit, OnTriggerStay,
    OnCollisionEnter, OnCollisionExit, OnCollisionStay,
}