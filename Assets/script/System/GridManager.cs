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

    public GameObject selectionEffectPrefab; // 指向效果的Prefab
    private GameObject currentSelectionEffect; // 當前顯示的選擇效果物件

    void Start()
    {
        InitializeGrid();
        InitializeChessPieces();
    }

    void Update()
    {
        HandleSelectionEffect();
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
        }
    }
    private void InitializeChessPieces()
    {
        chessPieces = new List<ChessPiece>(FindObjectsOfType<ChessPiece>());
        foreach (var piece in chessPieces)
        {
            GridCell cell = GetGridCell(piece.gridPosition);
            if (cell != null)
            {
                cell.isOccupied = true; // 標記該格子為被佔據
            }
        }
    }
    public void MovePiece(GridCell currentCell, GridCell targetCell, ChessPiece piece)
    {
        // 確保先前的格子不再被佔據
        //GridCell previousCell = GetGridCell(piece.gridPosition);
        if (targetCell.isOccupied)
        {
            Debug.LogWarning("目標格子已被佔據，無法移動。");
            return; // 結束移動操作
        }

        // 更新棋子的 gridPosition 為新格子的位置
        piece.UpdateGridPosition(targetCell.gridPosition);

        if (currentCell != null)  // 確保原位置有效
        {
            
            // 清空原格子的佔據狀態
            currentCell.isOccupied = false;
        }

        // 更新新格子的佔據狀態
        targetCell.isOccupied = true;

        Debug.Log($"{piece.gameObject.name} 移動到新格子 ({targetCell.gridPosition.x}, {targetCell.gridPosition.y})");
    }
    
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
                return cell;
            }
        }
        return null;
    }

    // 新增方法：獲取指定 GridCell 的鄰居
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
            new Vector2Int(-1, 0), // 左
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

    // Get movement range within certain radius
    public List<GridCell> GetMovementRange(Vector2Int start, int range)
    {
        List<GridCell> movementRangeCells = new List<GridCell>();

        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                Vector2Int targetPosition = new Vector2Int(start.x + x, start.y + y);
                if (Mathf.Abs(x) + Mathf.Abs(y) <= range)
                {
                    GridCell cell = GetGridCell(targetPosition);
                    if (cell != null && cell.isWalkable) // Only highlight walkable cells
                    {
                        movementRangeCells.Add(cell);
                    }
                }
            }
        }

        return movementRangeCells;
    }
    public List<GridCell> GetAttackRange(Vector2Int position, int attackRange)
    {
        List<GridCell> attackRangeCells = new List<GridCell>();

        for (int x = -attackRange; x <= attackRange; x++)
        {
            for (int y = -attackRange; y <= attackRange; y++)
            {
                Vector2Int checkPos = new Vector2Int(position.x + x, position.y + y);
                if (Mathf.Abs(x) + Mathf.Abs(y) <= attackRange)
                {
                    GridCell cell = GetGridCell(checkPos);
                    if (cell != null) // 根據需求加入額外條件
                    {
                        attackRangeCells.Add(cell);
                    }
                }
            }
        }

        return attackRangeCells;
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
            // 清除棋子佔據的格子狀態
            GridCell currentCell = GetGridCell(piece.gridPosition);
            if (currentCell != null)
            {
                currentCell.isOccupied = false;
            }

            chessPieces.Remove(piece);
        }
    }

    private void HandleSelectionEffect()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            GridCell cell = hit.collider.GetComponent<GridCell>();

            if (cell != null)
            {
                // 如果有選擇效果物件，且它不是在當前格子上，先刪除
                if (currentSelectionEffect != null && currentSelectionEffect.transform.parent != cell.transform)
                {
                    Destroy(currentSelectionEffect);
                }

                // 如果沒有選擇效果物件，就生成一個在當前格子上
                if (currentSelectionEffect == null)
                {
                    currentSelectionEffect = Instantiate(selectionEffectPrefab, cell.transform.position + new Vector3(0, 0.2f, 0), Quaternion.identity, cell.transform);
                }
            }
        }
        else
        {
            // 滑鼠移出任何格子時，清除選擇效果
            if (currentSelectionEffect != null)
            {
                Destroy(currentSelectionEffect);
            }
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
