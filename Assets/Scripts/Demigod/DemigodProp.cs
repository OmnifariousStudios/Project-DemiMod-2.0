using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Basic Prop class for all Demigod objects.
/// </summary>
public class DemigodProp : MonoBehaviour
{
    public ReplaceablePropType propType = ReplaceablePropType.WoodenCrate1;
}

public enum ReplaceablePropType
{
    Billboard1, 
    BrickWall1, SteelWall1, GlassWall1, 
    SimpleBuilding1, SimpleBuilding2,
    SimpleFloor1,
    WoodenCrate1, WoodenBarrel1, 
    WoodenBench1, StoneBench1,
    StreetLight1,
    StoneColumn1, StoneGargoyle1, StoneLionStatue1,
    TrainingTarget1
}