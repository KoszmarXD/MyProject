using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    private GridManager gridManager;
    private TurnManager turnManager;

    private void Start()
    {
        // 初始化 GridManager 與 TurnManager
        gridManager = FindObjectOfType<GridManager>();
        turnManager = FindObjectOfType<TurnManager>();

        if (gridManager == null)
            Debug.LogError("找不到 GridManager，請確認場景中是否有 GridManager 物件。");

        if (turnManager == null)
            Debug.LogError("找不到 TurnManager，請確認場景中是否有 TurnManager 物件。");
        
    }

    public void TakeAITurn()
    {
        if (gridManager == null || turnManager == null)
        {
            Debug.LogError("GridManager 或 TurnManager 未初始化。");
            return;
        }

        // 獲取所有玩家棋子
        List<ChessPiece> playerPieces = gridManager.GetEnemies(isPlayerControlled: false);
        if (playerPieces == null || playerPieces.Count == 0)
        {
            Debug.LogWarning("未找到玩家棋子。");
            EndAITurn(); // 沒有玩家棋子可行動，直接結束 AI 回合
            return;
        }

        // AI棋子的行動
        foreach (var aiPiece in gridManager.GetEnemies(isPlayerControlled: true))
        {
            if (aiPiece == null || aiPiece.hasMoved)
            {
                Debug.Log($"{aiPiece?.gameObject.name ?? "AI棋子"} 已移動或為空，跳過行動。");
                continue;
            }

            // 找到最近的玩家棋子
            ChessPiece targetPiece = FindNearestPlayerPiece(aiPiece, playerPieces);
            if (targetPiece == null)
            {
                Debug.LogWarning("找不到最近的玩家棋子，跳過此AI棋子。");
                continue;
            }

            // 檢查是否能攻擊到目標棋子
            List<GridCell> attackRange = gridManager.GetAttackRange(aiPiece.gridPosition, aiPiece.attackRange);
            GridCell targetCell = gridManager.GetGridCell(targetPiece.gridPosition);

            if (attackRange.Contains(targetCell))
            {
                // 如果目標在攻擊範圍內，直接攻擊
                Debug.Log($"{aiPiece.gameObject.name} 攻擊 {targetPiece.gameObject.name}。");
                aiPiece.Attack(targetPiece);
                aiPiece.hasMoved = true; // 標記為已行動
            }
            else
            {
                // 如果目標不在攻擊範圍內，嘗試移動到靠近目標的格子
                List<GridCell> targetArea = GetTargetAreaAroundPiece(targetPiece, aiPiece.attackRange);

                if (targetArea == null || targetArea.Count == 0)
                {
                    Debug.LogWarning($"{targetPiece.gameObject.name} 周圍沒有可移動的格子。");
                    continue;
                }

                // 找到最近的可移動目標格子
                GridCell moveCell = FindClosestAccessibleCell(aiPiece.gridPosition, targetArea);
                if (moveCell != null)
                {
                    Debug.Log($"{aiPiece.gameObject.name} 移動到 {moveCell.gridPosition}。");
                    gridManager.MovePiece(gridManager.GetGridCell(aiPiece.gridPosition), moveCell, aiPiece);
                }
                else
                {
                    Debug.LogWarning($"{aiPiece.gameObject.name} 無法找到可行路徑。");
                }
            }
        }

        EndAITurn(); // 所有行動結束後，切換到玩家回合
    }

    // 找到最近的玩家棋子
    private ChessPiece FindNearestPlayerPiece(ChessPiece aiPiece, List<ChessPiece> playerPieces)
    {
        ChessPiece nearestPiece = null;
        float minDistance = float.MaxValue;

        foreach (var playerPiece in playerPieces)
        {
            float distance = Vector2Int.Distance(aiPiece.gridPosition, playerPiece.gridPosition);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestPiece = playerPiece;
            }
        }

        return nearestPiece;
    }

    // 獲取目標棋子周圍的格子，該格子必須在AI的攻擊範圍內且可行走
    private List<GridCell> GetTargetAreaAroundPiece(ChessPiece targetPiece, int attackRange)
    {
        List<GridCell> targetArea = new List<GridCell>();
        List<GridCell> attackRangeCells = gridManager.GetAttackRange(targetPiece.gridPosition, attackRange);

        foreach (var cell in attackRangeCells)
        {
            if (cell.isWalkable && !cell.isOccupied)
            {
                targetArea.Add(cell);
            }
        }

        if (targetArea.Count == 0)
        {
            Debug.LogWarning($"目標棋子 {targetPiece.gameObject.name} 周圍沒有可行走的格子。");
        }

        return targetArea;
    }

    // 找到從起點到目標格子清單中最近的可移動格子
    private GridCell FindClosestAccessibleCell(Vector2Int start, List<GridCell> targetCells)
    {
        AStarPathfinding aStar = FindObjectOfType<AStarPathfinding>();
        if (aStar == null)
        {
            Debug.LogWarning("AStarPathfinding 未初始化。");
            return null;
        }

        GridCell closestCell = null;
        float minCost = float.MaxValue;

        foreach (var cell in targetCells)
        {
            if (!gridManager.IsValidGridPosition(cell.gridPosition)) continue;

            var (path, totalCost) = aStar.FindPath(start, cell.gridPosition);
            if (path != null && totalCost < minCost)
            {
                closestCell = cell;
                minCost = totalCost;
            }
        }

        return closestCell;
    }

    private void EndAITurn()
    {
        Debug.Log("AI回合結束，切換到玩家回合。");
        turnManager.EndTurn();
    }
}
