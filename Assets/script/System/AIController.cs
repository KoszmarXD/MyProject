using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    private GridManager gridManager;
    private TurnManager turnManager;

    private void Start()
    {
        // ��l�� GridManager �P TurnManager
        gridManager = FindObjectOfType<GridManager>();
        turnManager = FindObjectOfType<TurnManager>();

        if (gridManager == null)
            Debug.LogError("�䤣�� GridManager�A�нT�{�������O�_�� GridManager ����C");

        if (turnManager == null)
            Debug.LogError("�䤣�� TurnManager�A�нT�{�������O�_�� TurnManager ����C");
        
    }

    public void TakeAITurn()
    {
        if (gridManager == null || turnManager == null)
        {
            Debug.LogError("GridManager �� TurnManager ����l�ơC");
            return;
        }

        // ����Ҧ����a�Ѥl
        List<ChessPiece> playerPieces = gridManager.GetEnemies(isPlayerControlled: false);
        if (playerPieces == null || playerPieces.Count == 0)
        {
            Debug.LogWarning("����쪱�a�Ѥl�C");
            EndAITurn(); // �S�����a�Ѥl�i��ʡA�������� AI �^�X
            return;
        }

        // AI�Ѥl�����
        foreach (var aiPiece in gridManager.GetEnemies(isPlayerControlled: true))
        {
            if (aiPiece == null || aiPiece.hasMoved)
            {
                Debug.Log($"{aiPiece?.gameObject.name ?? "AI�Ѥl"} �w���ʩά��šA���L��ʡC");
                continue;
            }

            // ���̪񪺪��a�Ѥl
            ChessPiece targetPiece = FindNearestPlayerPiece(aiPiece, playerPieces);
            if (targetPiece == null)
            {
                Debug.LogWarning("�䤣��̪񪺪��a�Ѥl�A���L��AI�Ѥl�C");
                continue;
            }

            // �ˬd�O�_�������ؼдѤl
            List<GridCell> attackRange = gridManager.GetAttackRange(aiPiece.gridPosition, aiPiece.attackRange);
            GridCell targetCell = gridManager.GetGridCell(targetPiece.gridPosition);

            if (attackRange.Contains(targetCell))
            {
                // �p�G�ؼЦb�����d�򤺡A��������
                Debug.Log($"{aiPiece.gameObject.name} ���� {targetPiece.gameObject.name}�C");
                aiPiece.Attack(targetPiece);
                aiPiece.hasMoved = true; // �аO���w���
            }
            else
            {
                // �p�G�ؼФ��b�����d�򤺡A���ղ��ʨ�a��ؼЪ���l
                List<GridCell> targetArea = GetTargetAreaAroundPiece(targetPiece, aiPiece.attackRange);

                if (targetArea == null || targetArea.Count == 0)
                {
                    Debug.LogWarning($"{targetPiece.gameObject.name} �P��S���i���ʪ���l�C");
                    continue;
                }

                // ���̪񪺥i���ʥؼЮ�l
                GridCell moveCell = FindClosestAccessibleCell(aiPiece.gridPosition, targetArea);
                if (moveCell != null)
                {
                    Debug.Log($"{aiPiece.gameObject.name} ���ʨ� {moveCell.gridPosition}�C");
                    gridManager.MovePiece(gridManager.GetGridCell(aiPiece.gridPosition), moveCell, aiPiece);
                }
                else
                {
                    Debug.LogWarning($"{aiPiece.gameObject.name} �L�k���i����|�C");
                }
            }
        }

        EndAITurn(); // �Ҧ���ʵ�����A�����쪱�a�^�X
    }

    // ���̪񪺪��a�Ѥl
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

    // ����ؼдѤl�P�򪺮�l�A�Ӯ�l�����bAI�������d�򤺥B�i�樫
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
            Debug.LogWarning($"�ؼдѤl {targetPiece.gameObject.name} �P��S���i�樫����l�C");
        }

        return targetArea;
    }

    // ���q�_�I��ؼЮ�l�M�椤�̪񪺥i���ʮ�l
    private GridCell FindClosestAccessibleCell(Vector2Int start, List<GridCell> targetCells)
    {
        AStarPathfinding aStar = FindObjectOfType<AStarPathfinding>();
        if (aStar == null)
        {
            Debug.LogWarning("AStarPathfinding ����l�ơC");
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
        Debug.Log("AI�^�X�����A�����쪱�a�^�X�C");
        turnManager.EndTurn();
    }
}
