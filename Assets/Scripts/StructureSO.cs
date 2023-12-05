using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StructureType
{
    Bridge,
    Turret
}

[CreateAssetMenu(fileName = "Structure", menuName = "ScriptableObjects/Structure")]
public class StructureSO : ScriptableObject
{
    public string StructureName;
    public Sprite StructureIcon;
    public StructureType StructureType;
    public List<ItemCost> Cost;
    public string Description;
    public int totalNumOfRequiredItems;
}
