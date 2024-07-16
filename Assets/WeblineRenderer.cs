using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeblineRenderer : MonoBehaviour
{
    public PlayerAvatar playerAvatar;
    public LineRenderer leftLineRenderer;
    public LineRenderer rightLineRenderer;

    public Transform leftWeblineOrigin;
    public LineRenderer leftWeblineOriginLineRenderer;
    
    public Vector3 leftWeblineTrailPoint;
    public Transform leftWebGrabHandPositionLower;
    public Transform leftWebGrabHandPositionUpper;
    public Vector3 leftWeblineConnectionPoint;
    
    public Transform rightWeblineOrigin;
    public LineRenderer rightWeblineOriginLineRenderer;
    
    public Vector3 rightWeblineTrailPoint;
    public Transform rightWebGrabHandPositionLower;
    public Transform rightWebGrabHandPositionUpper;
    public Vector3 rightWeblineConnectionPoint;

    private void Update()
    {
        SetPoints();
    }

    public void SetPoints()
    {
        if(!playerAvatar || !leftLineRenderer || !rightLineRenderer || !leftWebGrabHandPositionLower || !leftWebGrabHandPositionUpper || !rightWebGrabHandPositionLower || !rightWebGrabHandPositionUpper)
        {
            return;
        }
        
        if (playerAvatar)
        {
            
            leftWebGrabHandPositionLower = playerAvatar.leftWebGrabHandPositionLower;
            leftWebGrabHandPositionUpper = playerAvatar.leftWebGrabHandPositionUpper;
            
            leftWeblineTrailPoint = leftWebGrabHandPositionLower.position + (leftWebGrabHandPositionLower.position - leftWebGrabHandPositionUpper.position).normalized * 0.2f;
            leftWeblineConnectionPoint = leftWebGrabHandPositionUpper.position + (leftWebGrabHandPositionUpper.position - leftWebGrabHandPositionLower.position).normalized * 0.5f;
            
            leftLineRenderer.positionCount = 4;
            
            leftLineRenderer.SetPosition(0, leftWeblineTrailPoint);
            leftLineRenderer.SetPosition(1, leftWebGrabHandPositionLower.position);
            leftLineRenderer.SetPosition(2, leftWebGrabHandPositionUpper.position);
            leftLineRenderer.SetPosition(3, leftWeblineConnectionPoint);
            
            
            
            rightWebGrabHandPositionLower = playerAvatar.rightWebGrabHandPositionLower;
            rightWebGrabHandPositionUpper = playerAvatar.rightWebGrabHandPositionUpper;
            
            rightWeblineTrailPoint = rightWebGrabHandPositionLower.position + (rightWebGrabHandPositionLower.position - rightWebGrabHandPositionUpper.position).normalized * 0.2f;
            rightWeblineConnectionPoint = rightWebGrabHandPositionUpper.position + (rightWebGrabHandPositionUpper.position - rightWebGrabHandPositionLower.position).normalized * 0.5f;
            
            rightLineRenderer.positionCount = 4;
            
            rightLineRenderer.SetPosition(0, rightWeblineTrailPoint);
            rightLineRenderer.SetPosition(1, rightWebGrabHandPositionLower.position);
            rightLineRenderer.SetPosition(2, rightWebGrabHandPositionUpper.position);
            rightLineRenderer.SetPosition(3, rightWeblineConnectionPoint);
        }
        
        if(leftWeblineOrigin && leftWeblineOriginLineRenderer)
        {
            leftWeblineOriginLineRenderer.positionCount = 2;
            leftWeblineOriginLineRenderer.SetPosition(0, leftWeblineOrigin.position);
            leftWeblineOriginLineRenderer.SetPosition(1, leftWeblineOrigin.position + leftWeblineOrigin.forward * 0.5f);
        }
        
        if(rightWeblineOrigin && rightWeblineOriginLineRenderer)
        {
            rightWeblineOriginLineRenderer.positionCount = 2;
            rightWeblineOriginLineRenderer.SetPosition(0, rightWeblineOrigin.position);
            rightWeblineOriginLineRenderer.SetPosition(1, rightWeblineOrigin.position + rightWeblineOrigin.forward * 0.5f);
        }
    }
}
