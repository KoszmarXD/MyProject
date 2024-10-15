using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTerrainType", menuName = "Grid/TerrainType")]
public class TerrainType : ScriptableObject
{
    public string terrainName;
    public bool isWalkable;
    public int movementCost;
    public Material terrainMaterial;
}
