using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
    public void UpdateGridCellState(GridCell cell, bool isOccupied)
    {
        // 根據傳入的佔據狀態更新指定的格子
        if (isOccupied)
        {
            cell.gameObject.layer = LayerMask.NameToLayer("Default");
            cell.isOccupied = true;
        }
        else
        {
            cell.gameObject.layer = LayerMask.NameToLayer("Selectable");
            cell.isOccupied = false;
        }
    }

    // 初始化棋盤格，讀取場景中所有帶有 GridCell 組件的物體
    private void InitializeGrid()
    {
        // 找到所有場景中的 GridCell 物體
        GridCell[] cells = FindObjectsOfType<GridCell>();
        if (cells.Length == 0) return;
        int maxX = 0, maxZ = 0;

        foreach (var cell in cells)
        {
            maxX = Math.Max(maxX, cell.gridPosition.x);
            maxZ = Math.Max(maxZ, cell.gridPosition.y);
        }

        width = maxX + 1;
        height = maxZ + 1;
        gridCells = new GridCell[width, height];

        foreach (var cell in cells)
        {
            if (cell.gridPosition.x >= 0 && cell.gridPosition.x < width &&
                cell.gridPosition.y >= 0 && cell.gridPosition.y < height)
            {
                gridCells[cell.gridPosition.x, cell.gridPosition.y] = cell;
                ApplyTerrainType(cell);
            }
        }

        chessPieces = new List<ChessPiece>(FindObjectsOfType<ChessPiece>());

        foreach (var piece in chessPieces)
        {
            Vector2Int piecePosition = piece.gridPosition; // 每個棋子有 gridPosition 屬性
            GridCell occupiedCell = GetGridCell(piecePosition); // 獲取棋子所在的格子

            if (occupiedCell != null)
            {
                occupiedCell.isOccupied = true;   // 將格子標記為被佔據
                occupiedCell.UpdateLayer();       // 更新 Layer 為 Default
            }
            else
            {
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
            // 將 GridManager 傳入棋子
            piece.Initialize(this);
        }
    }
    public void MovePiece(GridCell currentCell, GridCell targetCell, ChessPiece piece)
    {
        if (piece.hasMoved)
        {
            Debug.Log($"{piece.gameObject.name} 本回合已移動過");
            return;
        }

        AStarPathfinding pathfinder = FindObjectOfType<AStarPathfinding>();
        if (pathfinder == null)
        {
            Debug.LogError("找不到 AStarPathfinding 組件");
            return;
        }

        // 計算路徑
        var (path, totalCost) = pathfinder.FindPath(currentCell.gridPosition, targetCell.gridPosition);
        if (path == null || path.Count == 0)
        {
            Debug.LogError("無法到達目標位置");
            return;
        }

        // 開始沿著路徑移動
        StartCoroutine(MoveAlongPath(piece, path));

        // 更新格子佔據狀態
        ResetOccupiedCells(piece, currentCell);

        // 更新棋子的位置
        piece.UpdateGridPosition(targetCell.gridPosition, this);

        // 設定棋子為已移動
        piece.hasMoved = true;
    }

    private IEnumerator MoveAlongPath(ChessPiece piece, List<GridCell> path)
    {
        foreach (GridCell cell in path)
        {
            Vector3 targetPosition = cell.transform.position;
            targetPosition.y = piece.transform.position.y; // 保持 Y 軸穩定

            // 平滑移動
            float elapsedTime = 0f;
            float moveDuration = 0.2f; // 每格移動時間
            Vector3 startPosition = piece.transform.position;

            while (elapsedTime < moveDuration)
            {
                piece.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / moveDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // 強制調整到目標格子
            piece.transform.position = targetPosition;
        }
    }

    private void ApplyTerrainType(GridCell cell)
    {
        // 使用枚舉來匹配材質
        switch (cell.terrainType)
        {
            case TerrainTypeEnum.Grass:
            case TerrainTypeEnum.Water:
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
            return gridCells[position.x, position.y];
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
            return accessible;
        }

        GridCell startCell = GetGridCell(start);
        if (startCell == null)
        {
            return accessible;
        }

        // 遍歷所有可行走的格子，並檢查從起點到目標格子的總成本是否在範圍內
        foreach (var cell in gridCells)
        {
            if (cell != null && cell.isWalkable)
            {
                // 使用 A* 尋路
                var (path, totalCost) = aStar.FindPath(start, cell.gridPosition);
                if (path != null && totalCost <= range)
                    accessible.Add(cell);
            }
        }
        return accessible;
    }

    // 新增：獲取棋子的可移動範圍
    public List<GridCell> GetAvailableCells(Vector2Int gridPosition, int movementRange, bool isPlayerControlled)
    {
        List<GridCell> accessibleCells = new List<GridCell>();

        for (int x = -movementRange; x <= movementRange; x++)
        {
            for (int y = -movementRange; y <= movementRange; y++)
            {
                Vector2Int targetPosition = new Vector2Int(gridPosition.x + x, gridPosition.y + y);
                if (Mathf.Abs(x) + Mathf.Abs(y) <= movementRange)
                {
                    GridCell cell = GetGridCell(targetPosition);
                    if (cell != null && cell.isWalkable && !cell.isOccupied)
                    {
                        accessibleCells.Add(cell);
                    }
                }
            }
        }
        return accessibleCells;
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
    // 重置每回合的棋子狀態
    public void ResetTurnState()
    {
        foreach (var piece in chessPieces)
        {
            piece.hasMoved = false;
            piece.hasAttacked = false;
        }
    }
    public bool IsValidGridPosition(Vector2Int position)
    {
        // 檢查是否在棋盤範圍內
        if (position.x >= 0 && position.x < width && position.y >= 0 && position.y < height)
        {
            GridCell cell = GetGridCell(position); // 獲取對應的格子
                                                   // 確保該格子存在，並且是可行走的
            return cell != null && cell.isWalkable && !cell.isOccupied;
        }
        return false; // 若不在範圍內則返回 false
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

            if (cell != null && !cell.isOccupied) // 檢查是否被佔據
            {
                // 若有選擇效果物件，且它不是當前格子上，刪除它
                if (currentSelectionEffect != null && currentSelectionEffect.transform.parent != cell.transform)
                {
                    Destroy(currentSelectionEffect);
                }

                // 若沒有選擇效果物件，生成在當前格子上
                if (currentSelectionEffect == null)
                {
                    currentSelectionEffect = Instantiate(selectionEffectPrefab, cell.transform.position + new Vector3(0, 0.1f, 0), Quaternion.identity, cell.transform);
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
    public void ResetOccupiedCells(ChessPiece movedPiece = null, GridCell previousCell = null)
    {
        // 如果提供了棋子和起始格子，僅更新相關格子
        if (movedPiece != null && previousCell != null)
        {
            // 清除起始格子的狀態
            if (previousCell != null)
            {
                previousCell.isOccupied = false;
                previousCell.gameObject.layer = LayerMask.NameToLayer("Selectable");
            }

            // 設定目標格子的狀態
            GridCell targetCell = GetGridCell(movedPiece.gridPosition);
            if (targetCell != null)
            {
                targetCell.isOccupied = true;
                targetCell.gameObject.layer = LayerMask.NameToLayer("Default");
            }

            return;
        }

        // 如果未提供參數，則重置整個棋盤
        foreach (var cell in gridCells)
        {
            if (cell != null)
            {
                cell.isOccupied = false;
                cell.gameObject.layer = LayerMask.NameToLayer("Selectable");
            }
        }

        // 根據所有棋子的當前位置更新格子狀態
        foreach (var piece in chessPieces)
        {
            GridCell occupiedCell = GetGridCell(piece.gridPosition);
            if (occupiedCell != null)
            {
                occupiedCell.isOccupied = true;
                occupiedCell.gameObject.layer = LayerMask.NameToLayer("Default");
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
}
