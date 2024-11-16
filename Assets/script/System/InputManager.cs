using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    private GridManager gridManager;
    private ChessPiece selectedPiece;
    private AStarPathfinding aStar;
    public float moveSpeed = 5f; // 棋子移動速度

    public LayerMask selectableLayers;

    private Color originalChessPieceColor;

    private List<GridCell> currentPath = new List<GridCell>();
    private int currentPathIndex = 0;
    private bool isMoving = false;

    public GameObject moveHighlightPrefab;  // 用於顯示移動範圍的 Prefab
    public GameObject attackHighlightPrefab; // 用於顯示攻擊範圍的 Prefab

    private List<GameObject> activeHighlights = new List<GameObject>(); // 儲存當前生成的高亮物件
    private bool isPlayerTurn = true;  // 用於確認是否為玩家回合

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        aStar = FindObjectOfType<AStarPathfinding>();
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnTurnChanged += OnTurnChanged;
        }
    }

    void Update()
    {
        // 確認是否為玩家回合，且不能在 AI 回合中操作
        if (TurnManager.Instance.isPlayerTurn)
        {
            HandleSelection();
            HandleCommands();
        }
        HandleMovement();
    }

    private void HandleSelection()
    {
        if (!isPlayerTurn || isMoving) return;  // 在非玩家回合或移動中不允許選擇

        if (Input.GetMouseButtonDown(0) && (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, selectableLayers))
            {
                ChessPiece piece = hit.collider.GetComponent<ChessPiece>();
                if (piece != null && piece.isPlayerControlled && !piece.hasMoved)
                {
                    SelectPiece(piece);
                }
                else
                {
                    GridCell cell = hit.collider.GetComponent<GridCell>();
                    if (cell != null)
                    {
                        SelectCell(cell, GetAStar());
                    }
                }
            }
            else
            {
                DeselectCurrentPiece(); // 當點擊空白處時，取消選擇並清空高亮格子
            }
        }
    }

    private void SelectPiece(ChessPiece piece)
    {
        DeselectCurrentPiece();
        selectedPiece = piece;
        
        Renderer rend = selectedPiece.GetComponent<Renderer>();
        if (rend != null)
        {
            originalChessPieceColor = rend.material.color;
            rend.material.color = Color.yellow;
        }
        DisplayAttackRange(piece);
        DisplayAvailableMoves(piece);
        selectedPiece.IsSelected = true;
    }

    private AStarPathfinding GetAStar()
    {
        return aStar;
    }

    private void SelectCell(GridCell cell, AStarPathfinding aStar)
    {
        if (selectedPiece != null && IsCellInRange(cell))
        {
            Vector2Int start = selectedPiece.gridPosition;
            Vector2Int target = cell.gridPosition;
            var (path, totalCost) = aStar.FindPath(start, target);

            if (path != null && path.Count > 1 && totalCost <= selectedPiece.movementRange)
            {
                currentPath = path;
                currentPathIndex = 1;
                isMoving = true;
            }
        }
    }

    private void DeselectCurrentPiece()
    {
        if (selectedPiece != null)
        {
            // 恢復棋子的原始顏色
            Renderer rend = selectedPiece.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = originalChessPieceColor;
            }
            // 刪除所有生成的高亮物件
            foreach (GameObject highlight in activeHighlights)
            {
                Destroy(highlight);
            }
            activeHighlights.Clear();
            selectedPiece.IsSelected = false;
            selectedPiece = null;
        }
    }

    private void DisplayAvailableMoves(ChessPiece piece)
    {
        if (selectedPiece != null)
        {
            var accessibleCells = gridManager.GetAccessibleCells(selectedPiece.gridPosition, selectedPiece.movementRange);
            foreach (var cell in accessibleCells)
            {
                // 調整生成高亮物件的位置，讓其稍微高於棋盤
                Vector3 highlightPosition = cell.transform.position;
                highlightPosition.y += 0.05f; // 調整 Y 軸位置，將其抬高一點

                GameObject highlight = Instantiate(moveHighlightPrefab, highlightPosition, Quaternion.identity);
                activeHighlights.Add(highlight);
            }
        }
    }

    private void DisplayAttackRange(ChessPiece piece)
    {
        HashSet<GridCell> totalAttackRangeCells = new HashSet<GridCell>();
        List<GridCell> moveRangeCells = gridManager.GetAccessibleCells(piece.gridPosition, piece.movementRange);
        List<GridCell> attackRangeCells = gridManager.GetAttackRange(piece.gridPosition, piece.attackRange);
        foreach (GridCell moveCell in moveRangeCells)
        {
            List<GridCell> attackRangeFromMoveCell = gridManager.GetAttackRange(moveCell.gridPosition, piece.attackRange);

            foreach (GridCell attackCell in attackRangeFromMoveCell)
            {
                totalAttackRangeCells.Add(attackCell);
            }
        }
        // 確保攻擊範圍覆蓋到整個移動範圍
        foreach (GridCell attackCell in totalAttackRangeCells)
        {
            // 調整生成高亮物件的位置，讓其稍微高於棋盤
            Vector3 highlightPosition = attackCell.transform.position;
            highlightPosition.y += 0.05f; // 調整 Y 軸位置，將其抬高一點

            GameObject highlight = Instantiate(attackHighlightPrefab, highlightPosition, Quaternion.identity);
            activeHighlights.Add(highlight);
        }

    }
    private bool IsCellInRange(GridCell cell)
    {
        var accessibleCells = gridManager.GetAccessibleCells(selectedPiece.gridPosition, selectedPiece.movementRange);
        return accessibleCells.Contains(cell);
    }

    private void HandleMovement()
    {
        if (isMoving && currentPath != null && currentPathIndex < currentPath.Count)
        {
            GridCell targetCell = currentPath[currentPathIndex];

            if (selectedPiece == null)
            {
                isMoving = false;
                currentPath.Clear();
                return;
            }

            // 目標位置，保留原本高度
            Vector3 targetPosition = targetCell.transform.position;
            targetPosition.y = 1f;     // 保持 Y 軸不變

            // 逐步移動到目標位置
            Vector3 currentPosition = selectedPiece.transform.position;
            currentPosition.y = 1f; // 確保當前位置的 Y 軸也固定

            selectedPiece.transform.position = Vector3.MoveTowards(
                currentPosition,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            // 確認是否抵達目標格子
            if (Vector3.Distance(selectedPiece.transform.position, targetPosition) < 0.01f)
            {
                // 更新棋子的位置到目標格
                selectedPiece.transform.position = new Vector3(targetPosition.x, 1f, targetPosition.z); // 強制修正

                currentPathIndex++;

                // 如果到達路徑的最後一個格子
                if (currentPathIndex >= currentPath.Count)
                {
                    // 確保格子狀態正確更新
                    GridCell previousCell = gridManager.GetGridCell(selectedPiece.gridPosition);
                    GridCell targetGridCell = targetCell;

                    // 更新棋子邏輯座標
                    selectedPiece.gridPosition = targetGridCell.gridPosition;

                    // 使用優化的狀態更新
                    gridManager.ResetOccupiedCells(selectedPiece, previousCell);

                    isMoving = false;
                    selectedPiece.hasMoved = true;
                    currentPath.Clear();
                    DeselectCurrentPiece(); // 移動完成後取消選擇
                }
            }
        }
    }

    private void HandleCommands()
    {
        if (selectedPiece != null && Input.GetKeyDown(KeyCode.Space) && isPlayerTurn && !isMoving)
        {
            if (selectedPiece is Warrior warrior)
            {
                var enemy = warrior.DetectEnemy();
                if (enemy != null)
                {
                    warrior.Attack(enemy);
                    DeselectCurrentPiece();
                }
            }
        }
    }

    private void OnTurnChanged(bool isPlayerTurn)
    {
        this.isPlayerTurn = isPlayerTurn;
        if (!isPlayerTurn)
            DeselectCurrentPiece();      // 清空所有選擇
    }
    private void EndTurn()
    {
        if (selectedPiece != null)
        {
            selectedPiece.hasMoved = true;
            selectedPiece.IsSelected = false;
        }
        isPlayerTurn = false;
        TurnManager.Instance.NotifyTurnChanged(); // 觸發回合變更通知
    }

    private void OnDestroy()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnTurnChanged -= OnTurnChanged;
        }
    }
}
