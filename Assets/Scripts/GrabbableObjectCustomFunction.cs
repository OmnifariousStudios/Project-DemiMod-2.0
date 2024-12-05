using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GrabbableObjectCustomFunction : MonoBehaviour
{
    public List<EventAndActivationType> customEvents;
    
    public List<GroupToggles> groupToggles = new List<GroupToggles>();
    
    public HVRGrabbable Grabbable { get; private set; }
    
    public float minTimeBetweenButtonPresses = 0.5f;
}


public enum ActivationFunction
{
    None, SingleTrigger, DoubleTrigger, SingleBYButton
}

[Serializable]
public struct EventAndActivationType
{
    public UnityEvent eventToFire;
    public ActivationFunction activationFunction;
}

[Serializable]
public struct GroupToggles
{
    public ActivationFunction activationFunction;
    public List<UnityEvent> groupToggleEvents;
}