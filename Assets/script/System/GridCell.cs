using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TerrainTypeEnum
{
    Grass,
    Water,
    Mountain,
    // 可以添加更多地形類型
}
public class GridCell : MonoBehaviour
{
    [Tooltip("格子在棋盤上的 X 位置")]
    public int xPosition;

    [Tooltip("格子在棋盤上的 Z 位置")]
    public int zPosition;

    // 使用 Vector2Int 來存儲位置
    public Vector2Int gridPosition
    {
        get { return new Vector2Int(xPosition, zPosition); }
        set
        {
            xPosition = value.x;
            zPosition = value.y;
        }
    }

    public ChessPiece currentPiece;

    public bool isOccupied = false; // 記錄格子是否被佔據
    public int selectableLayer = 6; // 可選擇的層，例如第6層是SelectableLayer
    public int defaultLayer = 0;    // Unity中的Default層通常是第0層

    [Tooltip("是否可行走")]
    public bool isWalkable = true;

    [Tooltip("移動成本")]
    public int movementCost = 1;

    [Tooltip("地形類型")]
    public TerrainTypeEnum terrainType = TerrainTypeEnum.Grass;

    [Tooltip("高亮材質")]
    public Material highlightMaterial; // 在 Inspector 中設置
    public bool isHighlighted;
    private Renderer rend;
    private Material originalMaterial;
    internal object fCost;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend == null)
        {
            return;
        }
        SetMaterialBasedOnTerrain();
    }
    void Start()
    {
        isOccupied = false;  // 預設所有格子未被佔據
    }
    public void UpdateLayer()
    {
        gameObject.layer = isOccupied ? defaultLayer : selectableLayer;
    }

    // 根據地形類型設置原始顏色
    public void SetMaterialBasedOnTerrain()
    {
        switch (terrainType)
        {
            case TerrainTypeEnum.Grass:
                originalMaterial = Resources.Load<Material>("Grass"); // 確保你有一個 GrassMaterial 存在於 Resources 文件夾中
                break;
            case TerrainTypeEnum.Water:
                originalMaterial = Resources.Load<Material>("Water"); // 同上
                break;
            case TerrainTypeEnum.Mountain:
                originalMaterial = Resources.Load<Material>("Mountain"); // 同上
                break;
            default:
                originalMaterial = rend.material; // 默認使用當前材質
                break;
        }
        if (originalMaterial != null)
        {
            rend.material = originalMaterial;
        }
        else
        {
        }
    }

    // 高亮顯示格子
    public void HighlightAsAttack()
    {
        isHighlighted = true;
    }
    public void HighlightAsMove()
    {
        if (highlightMaterial != null)
        {
            isHighlighted = true;
        }        
    }
    
    public void SetHighlightColor(Color color)
    {
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.color = color;
    }

    // 重置格子的顏色
    public void ResetMaterial()
    {
        if (originalMaterial != null)
        {
            rend.material = originalMaterial;
            isHighlighted = false;
        }
        else
        {
        }
    }
}
