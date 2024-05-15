using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerAvatar : MonoBehaviour
{
    public Animator animator;

    [Header("Placement for Health and Energy UI")]
    public GameObject healthAndEnergyBar;

    public GameObject healthParentObject;
    public TextMeshProUGUI healthText;
    public Slider healthSlider;
    
    public GameObject energyParentObject;
    public TextMeshProUGUI energyText;
    public Slider energySlider;
    
    [Header("Body Part References")]
    public Transform avatarHead;
    public Transform avatarEyes;
    public Transform leftHand;
    public Transform rightHand;

    public Transform leftPalm;
    public Transform rightPalm;
    
    public Transform leftForearm;
    public Transform rightForearm;

    public Transform leftForearmTwist;
    public Transform rightForearmTwist;

    public TwistSolver leftHandTwistSolver;
    public TwistSolver rightHandTwistSolver;
    
    private Vector3 leftHandPositionOffset = Vector3.zero;
    private Vector3 leftHandRotationOffset = Vector3.zero;

    private Vector3 rightHandPositionOffset = Vector3.zero;
    private Vector3 rightHandRotationOffset = Vector3.zero;

    public Transform leftHandSpawnPointParent;
    public Transform rightHandSpawnPointParent;
    
    
    // Finger References
    public List<Transform> fingerBoneTransforms;
    public List<Transform> fingerTips;
    
    public Transform leftThumbRoot;
    public Transform leftThumbIntermediate;
    public Transform leftThumbEnd;
    public Transform leftThumbTip;

    public Transform leftIndexRoot;
    public Transform leftIndexIntermediate;
    public Transform leftIndexEnd;
    public Transform leftIndexTip;

    public Transform leftMiddleRoot;
    public Transform leftMiddleIntermediate;
    public Transform leftMiddleEnd;
    public Transform leftMiddleTip;

    public Transform leftRingRoot;
    public Transform leftRingIntermediate;
    public Transform leftRingEnd;
    public Transform leftRingTip;

    public Transform leftPinkyRoot;
    public Transform leftPinkyIntermediate;
    public Transform leftPinkyEnd;
    public Transform leftPinkyTip;
    
    public Transform rightThumbRoot;
    public Transform rightThumbIntermediate;
    public Transform rightThumbEnd;
    public Transform rightThumbTip;

    public Transform rightIndexRoot;
    public Transform rightIndexIntermediate;
    public Transform rightIndexEnd;
    public Transform rightIndexTip;

    public Transform rightMiddleRoot;
    public Transform rightMiddleIntermediate;
    public Transform rightMiddleEnd;
    public Transform rightMiddleTip;

    public Transform rightRingRoot;
    public Transform rightRingIntermediate;
    public Transform rightRingEnd;
    public Transform rightRingTip;

    public Transform rightPinkyRoot;
    public Transform rightPinkyIntermediate;
    public Transform rightPinkyEnd;
    public Transform rightPinkyTip;
    
    public TextAsset avatarHandPoseFile;

    [Header("Material References")]
    public List<CustomMaterialSetting> customMaterialSettings = new List<CustomMaterialSetting>();
    
    public List<UnityEvent> OnSlowMotionActivatedEvents = new List<UnityEvent>();
    public List<UnityEvent> OnSlowMotionDeactivatedEvents = new List<UnityEvent>();
    
    public List<UnityEvent> OnHoverActivatedEvents = new List<UnityEvent>();
    public List<UnityEvent> OnHoverDeactivatedEvents = new List<UnityEvent>();
    
    public List<UnityEvent> OnGlidingActivatedEvents = new List<UnityEvent>();
    public List<UnityEvent> OnGlidingDeactivatedEvents = new List<UnityEvent>();
    
    public List<UnityEvent> OnFreeFlowActivatedEvents = new List<UnityEvent>();
    public List<UnityEvent> OnFreeFlowDeactivatedEvents = new List<UnityEvent>();
    
    public List<UnityEvent> OnFlipActivatedEvents = new List<UnityEvent>();
    
    public List<UnityEvent> OnDiveActivatedEvents = new List<UnityEvent>();
    public List<UnityEvent> OnDiveDeactivatedEvents = new List<UnityEvent>();
    
    public List<UnityEvent> OnWallRunActivatedEvents = new List<UnityEvent>();
    public List<UnityEvent> OnWallRunDeactivatedEvents = new List<UnityEvent>();
    
    public List<UnityEvent> OnJumpActivatedEvents = new List<UnityEvent>();
    
    public List<UnityEvent> OnKickActivatedEvents = new List<UnityEvent>();
}





// Class for storing material settings.
[Serializable]
public class CustomMaterialSetting
{
    public Renderer renderer;
    public string rendererNameForUserInterface;
    
    public bool rendererIsEnabled = true;
    
    public string originalShaderName;
    public Shader originalShader;
        
    // The original variables for the material settings. Cached for when the material settings are reset.
    //public string originalMaterialNameForUserInterface;
    public Material originalMaterial;
    public Texture originalMaterialMainTexture;
    public bool originalMaterialUsingTexture = true;
     
        
    // The active variables for the material settings. These are the variables that are actually used by the game.
    public Material activeMaterial;
    public Texture activeMaterialMainTexture;
    public bool activeMaterialUsingTexture = true;
        
        
    public Color color;
    public MaterialPropertyBlockSetting materialPropertyBlockSetting;
}

public class MaterialPropertyBlockSetting
{
    
}