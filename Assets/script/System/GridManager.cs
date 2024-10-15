using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public TerrainType[] terrainTypes; // 在Inspector中設置不同的地形類型
    public List<ChessPiece> chessPieces; // 所有棋子

    private GridCell[,] gridCells;
    private int width = 0;
    private int height = 0;

    void Start()
    {
        InitializeGrid();
        InitializeChessPieces();
    }

    /// <summary>
    /// 初始化棋盤格，讀取場景中所有帶有 GridCell 組件的物體
    /// </summary>
    private void InitializeGrid()
    {
        // 找到所有場景中的 GridCell 物體
        GridCell[] cells = FindObjectsOfType<GridCell>();

        if (cells.Length == 0)
        {
            Debug.LogError("場景中沒有找到任何 GridCell！");
            return;
        }

        // 假設棋盤是矩形的，計算寬度和高度
        int maxX = 0;
        int maxZ = 0;

        foreach (var cell in cells)
        {
            if (cell.gridPosition.x > maxX) maxX = cell.gridPosition.x;
            if (cell.gridPosition.y > maxZ) maxZ = cell.gridPosition.y;
        }

        width = maxX + 1;
        height = maxZ + 1;

        gridCells = new GridCell[width, height];

        foreach (var cell in cells)
        {
            int x = cell.gridPosition.x;
            int z = cell.gridPosition.y;

            if (x >= 0 && x < width && z >= 0 && z < height)
            {
                gridCells[x, z] = cell;
                // 初始化或更新格子的屬性（根據 TerrainType）
                ApplyTerrainType(cell);
            }
            else
            {
                Debug.LogWarning($"GridCell {cell.name} 的 gridPosition ({x}, {z}) 超出範圍！");
            }
        }

        Debug.Log($"Grid 初始化完成，寬度: {width}, 高度: {height}");
    }
    private void InitializeChessPieces()
    {
        chessPieces = new List<ChessPiece>(FindObjectsOfType<ChessPiece>());
    }
    /// <summary>
    /// 根據 GridCell 的地形類型應用屬性和材質
    /// </summary>
    /// <param name="cell">要應用屬性的 GridCell</param>
    private void ApplyTerrainType(GridCell cell)
    {
        // 使用枚舉來匹配材質
        switch (cell.terrainType)
        {
            case TerrainTypeEnum.Grass:
                cell.SetMaterialBasedOnTerrain();
                break;
            case TerrainTypeEnum.Water:
                cell.SetMaterialBasedOnTerrain();
                break;
            case TerrainTypeEnum.Mountain:
                cell.SetMaterialBasedOnTerrain();
                break;
            default:
                Debug.LogWarning($"未處理的 terrainType: {cell.terrainType}");
                break;
        }
    }

    public List<GridCell> GetAvailableCells(Vector2Int start, int range, bool isPlayerControlled)
    {
        List<GridCell> available = new List<GridCell>();
        // 使用廣度優先搜索或其他算法來計算可移動範圍
        for (int x = start.x - range; x <= start.x + range; x++)
        {
            for (int z = start.y - range; z <= start.y + range; z++)
            {
                if (x >= 0 && x < width && z >= 0 && z < height)
                {
                    GridCell cell = gridCells[x, z];
                    if (cell != null && cell.isWalkable)
                    {
                        available.Add(cell);
                    }
                }
            }
        }
        return available;
    }

    // 獲取所有敵方棋子
    public List<ChessPiece> GetEnemies(bool isPlayerControlled)
    {
        List<ChessPiece> enemies = new List<ChessPiece>();
        foreach (var piece in chessPieces)
        {
            if (piece.isPlayerControlled != isPlayerControlled)
            {
                enemies.Add(piece);
            }
        }
        return enemies;
    }

    // 更新棋子列表（例如棋子死亡時）
    public void RemoveChessPiece(ChessPiece piece)
    {
        if (chessPieces.Contains(piece))
        {
            chessPieces.Remove(piece);
        }
    }
    
    void OnDrawGizmos()
    {
        if (gridCells == null)
            return;

        Gizmos.color = Color.blue;
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GridCell cell = gridCells[x, z];
                if (cell != null)
                {
                    Gizmos.DrawWireCube(cell.transform.position, new Vector3(1, 0.1f, 1));
                }
            }
        }
    }
}
