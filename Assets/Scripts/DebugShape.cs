using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script for easily finding & disabling all Debug Shape renderers before finishing weapons.
/// </summary>
public class DebugShape : MonoBehaviour
{
    public List<MeshRenderer> debugShapeMeshRenderers = new List<MeshRenderer>();
    
    
    [ContextMenu("Enable Shapes")]
    public void EnableShapes()
    {
        foreach (var meshRenderer in debugShapeMeshRenderers)
        {
            meshRenderer.enabled = true;
        }
    }
    
    [ContextMenu("Disable Shapes")]
    public void DisableShapes()
    {
        foreach (var meshRenderer in debugShapeMeshRenderers)
        {
            meshRenderer.enabled = false;
        }
    }
    
    
    [ContextMenu("Find Shapes")]
    public void FindShapes()
    {
        var meshRenderers = transform.GetComponentsInChildren<MeshRenderer>(true);
        foreach (var meshRenderer in meshRenderers)
        {
            if (debugShapeMeshRenderers.Contains(meshRenderer) == false)
            {
                debugShapeMeshRenderers.Add(meshRenderer);
            }
        }
    }
}
