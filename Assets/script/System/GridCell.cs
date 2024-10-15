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

    [Tooltip("是否可行走")]
    public bool isWalkable = true;

    [Tooltip("移動成本")]
    public int movementCost = 1;

    [Tooltip("地形類型")]
    public TerrainTypeEnum terrainType = TerrainTypeEnum.Grass;

    [Tooltip("高亮材質")]
    public Material highlightMaterial; // 在 Inspector 中設置

    private Renderer rend;
    private Material originalMaterial;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.LogError($"{gameObject.name} 缺少 Renderer 組件！");
            return;
        }
        SetMaterialBasedOnTerrain();
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
            Debug.Log($"{gameObject.name} 設置材質為 {originalMaterial.name}");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} 未找到對應的原始材質！");
        }
    }

    // 高亮顯示格子
    public void Highlight()
    {
        if (highlightMaterial != null)
        {
            rend.material = highlightMaterial;
            Debug.Log($"{gameObject.name} 被高亮顯示");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} 沒有設置 Highlight Material！");
        }
    }

    // 重置格子的顏色
    public void ResetMaterial()
    {
        if (originalMaterial != null)
        {
            rend.material = originalMaterial;
            Debug.Log($"{gameObject.name} 材質重置為 {originalMaterial.name}");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} 原始材質為 null，無法重置！");
        }
    }
    
}
