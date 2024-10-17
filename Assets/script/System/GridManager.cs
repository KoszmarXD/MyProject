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
    public int width = 0;
    public int height = 0;
    public int Width
    {
        get { return width; }
    }

    public int Height
    {
        get { return height; }
    }


    void Start()
    {
        InitializeGrid();
        InitializeChessPieces();
    }

    // 初始化棋盤格，讀取場景中所有帶有 GridCell 組件的物體
    private void InitializeGrid()
    {
        // 找到所有場景中的 GridCell 物體
        GridCell[] cells = FindObjectsOfType<GridCell>();

        if (cells.Length == 0)
        {
            Debug.LogError("場景中沒有找到任何 GridCell！");
            return;
        }

        // 找到最小和最大的 x 和 z 座標
        int minX = int.MaxValue;
        int minZ = int.MaxValue;
        int maxX = int.MinValue;
        int maxZ = int.MinValue;

        foreach (var cell in cells)
        {
            if (cell.transform.position.x < minX) minX = Mathf.RoundToInt(cell.transform.position.x);
            if (cell.transform.position.z < minZ) minZ = Mathf.RoundToInt(cell.transform.position.z);
            if (cell.transform.position.x > maxX) maxX = Mathf.RoundToInt(cell.transform.position.x);
            if (cell.transform.position.z > maxZ) maxZ = Mathf.RoundToInt(cell.transform.position.z);
        }

        // 計算偏移量以確保所有座標為非負數
        int offsetX = minX < 0 ? -minX : 0;
        int offsetZ = minZ < 0 ? -minZ : 0;

        width = maxX + offsetX + 1;
        height = maxZ + offsetZ + 1;

        gridCells = new GridCell[width, height];

        foreach (var cell in cells)
        {
            int x = Mathf.RoundToInt(cell.transform.position.x) + offsetX;
            int z = Mathf.RoundToInt(cell.transform.position.z) + offsetZ;

            if (x >= 0 && x < width && z >= 0 && z < height)
            {
                gridCells[x, z] = cell;
                cell.gridPosition = new Vector2Int(x, z); // 自動分配 gridPosition
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
        Debug.Log($"找到 {chessPieces.Count} 個棋子");
    }

    // 根據 GridCell 的地形類型應用屬性和材質
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

    // 獲取指定位置的 GridCell
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
            Debug.LogWarning($"GridCell 位置 ({position.x}, {position.y}) 超出範圍！寬度: {width}, 高度: {height}");
        }
        return null;
    }

    //獲取指定 GridCell 的鄰居
    public List<GridCell> GetNeighbors(GridCell cell)
    {
        List<GridCell> neighbors = new List<GridCell>();

        Vector2Int pos = cell.gridPosition;

        // 4 方向的鄰居
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1), // 上
            new Vector2Int(1, 0), // 右
            new Vector2Int(0, -1), // 下
            new Vector2Int(-1, 0)  // 左
        };

        foreach (var dir in directions)
        {
            Vector2Int neighborPos = new Vector2Int(pos.x + dir.x, pos.y + dir.y);
            // 檢查鄰居位置是否在有效範圍內
            if (neighborPos.x >= 0 && neighborPos.x < width && neighborPos.y >= 0 && neighborPos.y < height)
            {
                GridCell neighbor = GetGridCell(neighborPos);
                if (neighbor != null && neighbor.isWalkable)
                {
                    neighbors.Add(neighbor);
                    Debug.Log($"取得鄰居 GridCell: {neighbor.gameObject.name} at ({neighbor.gridPosition.x}, {neighbor.gridPosition.y})");
                }
            }
            else
            {
                Debug.LogWarning($"鄰居 GridCell 位置 ({neighborPos.x}, {neighborPos.y}) 超出範圍！");
            }
        }

        return neighbors;
    }

    // 獲取所有在移動範圍內且可達的 GridCell
    public List<GridCell> GetAccessibleCells(Vector2Int start, float maxCost)
    {
        List<GridCell> accessible = new List<GridCell>();
        AStarPathfinding aStar = FindObjectOfType<AStarPathfinding>();
        if (aStar == null)
        {
            Debug.LogError("AStarPathfinding 未在場景中找到！");
            return accessible;
        }

        accessible = aStar.FindAccessibleCells(start, maxCost);

        Debug.Log($"找到 {accessible.Count} 個可移動格子從 ({start.x}, {start.y}) 範圍內");
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
