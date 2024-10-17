using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class GridManager : MonoBehaviour
{
    public TerrainType[] terrainTypes; // 在Inspector中設置不同的地形類型
    public List<ChessPiece> chessPieces; // 所有棋子

    private GridCell[,] gridCells;
    private int width = 0;
    private int height = 0;
    private object maxCost;

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

    // 新增方法：獲取指定位置的 GridCell
    public GridCell GetGridCell(Vector2Int position)
    {
        if (position.x >= 0 && position.x < width && position.y >= 0 && position.y < height)
        {
            GridCell cell = gridCells[position.x, position.y];
            if (cell != null)
            {
                Debug.Log($"取得 GridCell: {cell.gameObject.name} at ({position.x}, {position.y})");
                return cell;
            }
            else
            {
                Debug.LogWarning($"GridCell 在 ({position.x}, {position.y}) 為 null！");
            }
        }
        else
        {
            Debug.LogWarning($"GridCell 位置 ({position.x}, {position.y}) 超出範圍！");
        }
        return null;
    }

    // 新增方法：獲取指定 GridCell 的鄰居
    public List<GridCell> GetNeighbors(GridCell cell)
    {
        List<GridCell> neighbors = new List<GridCell>();

        Vector2Int pos = cell.gridPosition;

        // 8 方向的鄰居（可根據需要調整為 4 方向）
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1), // 上
            new Vector2Int(1, 0), // 右
            new Vector2Int(0, -1), // 下
            new Vector2Int(-1, 0), // 左
            /*new Vector2Int(1, 1), // 右上
            new Vector2Int(1, -1), // 右下
            new Vector2Int(-1, -1), // 左下
            new Vector2Int(-1, 1) // 左上*/
        };

        foreach (var dir in directions)
        {
            Vector2Int neighborPos = new Vector2Int(pos.x + dir.x, pos.y + dir.y);
            GridCell neighbor = GetGridCell(neighborPos);
            if (neighbor != null && neighbor.isWalkable)
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }
    // 獲取所有在移動範圍內且可達的 GridCell
    public List<GridCell> GetAccessibleCells(Vector2Int start, int range)
    {
        List<GridCell> accessible = new List<GridCell>();
        AStarPathfinding aStar = FindObjectOfType<AStarPathfinding>();
        if (aStar == null)
        {
            Debug.LogError("AStarPathfinding 未在場景中找到！");
            return accessible;
        }

        GridCell startCell = GetGridCell(start);
        if (startCell == null)
        {
            Debug.LogError("起點格子為 null！");
            return accessible;
        }

        // 遍歷所有可行走的格子，並檢查從起點到目標格子的總成本是否在範圍內
        foreach (var cell in gridCells)
        {
            if (cell != null && cell.isWalkable)
            {
                var (path, totalCost) = aStar.FindPath(start, cell.gridPosition);
                if (path != null && totalCost <= range)
                {
                    accessible.Add(cell);
                }
            }
        }

        return accessible;
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

    internal List<GridCell> GetAvailableCells(Vector2Int gridPosition, int movementRange, bool isPlayerControlled)
    {
        throw new NotImplementedException();
    }
}
