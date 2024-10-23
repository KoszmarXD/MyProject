using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    private GridManager gridManager;
    private ChessPiece selectedPiece;
    private GridCell selectedCell;

    private AStarPathfinding aStar;
    public float moveSpeed = 5f; // 棋子移動速度

    public LayerMask selectableLayers;

    // 用於存儲當前高亮的格子
    private List<GridCell> highlightedCells = new List<GridCell>();

    // 用於存儲選中棋子的原始顏色
    private Color originalChessPieceColor;

    // 當前路徑
    private List<GridCell> currentPath = new List<GridCell>();
    private int currentPathIndex = 0;
    private bool isMoving = false;

    public GameObject moveHighlightPrefab;  // 用於顯示移動範圍的 Prefab
    public GameObject attackHighlightPrefab; // 用於顯示攻擊範圍的 Prefab

    private List<GameObject> activeHighlights = new List<GameObject>(); // 儲存當前生成的高亮物件
    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        aStar = FindObjectOfType<AStarPathfinding>();
    }

    void Update()
    {
        HandleSelection();
        HandleCommands();
        HandleMovement();
    }

    private void HandleSelection()
    {
        if (Input.GetMouseButtonDown(0) && (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.Log("發射射線");
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, selectableLayers))
            {
                Debug.Log($"射線擊中: {hit.collider.gameObject.name}");
                ChessPiece piece = hit.collider.GetComponent<ChessPiece>();
                if (piece != null && piece.isPlayerControlled)
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
                Debug.Log("射線未擊中任何物體");
                DeselectCurrentPiece(); // 當點擊空白處時，取消選擇並清空高亮格子
            }
        }
    }

    private void SelectPiece(ChessPiece piece)
    {
        if (selectedPiece != null)
        {
            DeselectCurrentPiece();
        }

        selectedPiece = piece;
        // 儲存選中棋子的原始顏色
        Renderer rend = selectedPiece.GetComponent<Renderer>();
        if (rend != null)
        {
            originalChessPieceColor = rend.material.color;
            // 高亮選中棋子
            rend.material.color = Color.yellow;
            Debug.Log($"{selectedPiece.gameObject.name} 被選中並高亮顯示");
        }
        DisplayAttackRange(piece);
        DisplayAvailableMoves(piece);
    }

    private AStarPathfinding GetAStar()
    {
        return aStar;
    }

    private void SelectCell(GridCell cell, AStarPathfinding aStar)
    {
        if (selectedPiece != null && IsCellInRange(cell))
        {
            // 使用 A* 尋找路徑
            Vector2Int start = selectedPiece.gridPosition;
            Vector2Int target = cell.gridPosition;
            var (path, totalCost) = aStar.FindPath(start, target);

            if (path != null && path.Count > 1 && totalCost <= selectedPiece.movementRange)
            {
                currentPath = path;
                currentPathIndex = 1; // 路徑的第一個點是當前位置
                isMoving = true;
                Debug.Log($"路徑已找到，開始移動棋子，總成本: {totalCost}");
            }
            else
            {
                Debug.Log("路徑未找到或總成本超出移動範圍。");
            }

            // 不立即取消選擇棋子，讓 selectedPiece 保持有效
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
                Debug.Log($"{selectedPiece.gameObject.name} 顏色恢復為 {originalChessPieceColor}");
            }
            // 刪除所有生成的高亮物件
            foreach (GameObject highlight in activeHighlights)
            {
                Destroy(highlight);
            }
            activeHighlights.Clear();

            selectedPiece = null;
            selectedCell = null;
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
            Debug.Log($"顯示 {accessibleCells.Count} 個可移動格子");
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

            if (targetCell == null)
            {
                Debug.LogError("targetCell 是 null！");
                isMoving = false;
                currentPath.Clear();
                return;
            }

            if (selectedPiece == null)
            {
                Debug.LogError("selectedPiece 是 null！");
                isMoving = false;
                currentPath.Clear();
                return;
            }

            if (selectedPiece.transform == null)
            {
                Debug.LogError("selectedPiece.transform 是 null！");
                isMoving = false;
                currentPath.Clear();
                return;
            }

            Vector3 targetPosition = targetCell.transform.position;
            targetPosition.y = selectedPiece.transform.position.y; // 保留原有的 Y 軸位置

            selectedPiece.transform.position = Vector3.MoveTowards(selectedPiece.transform.position, targetPosition, moveSpeed * Time.deltaTime);
            Debug.Log($"{selectedPiece.gameObject.name} 正在移動到 {targetPosition}");

            if (Vector3.Distance(selectedPiece.transform.position, targetPosition) < 0.1f)
            {
                selectedPiece.transform.position = targetPosition;
                currentPathIndex++;

                // 更新棋子的 gridPosition
                selectedPiece.UpdateGridPosition(targetCell.gridPosition);
                Debug.Log($"{selectedPiece.gameObject.name} 的 gridPosition 更新為 ({targetCell.gridPosition.x}, {targetCell.gridPosition.y})");

                if (currentPathIndex >= currentPath.Count)
                {
                    isMoving = false;
                    currentPath.Clear();
                    Debug.Log("棋子已到達目標位置。");
                    DeselectCurrentPiece(); // 在移動完成後取消選擇棋子
                }
            }
        }
        else
        {
            if (!isMoving)
            {
                Debug.Log("isMoving 為 false，跳過 HandleMovement。");
            }
            if (currentPath == null)
            {
                Debug.Log("currentPath 是 null，跳過 HandleMovement。");
            }
            if (currentPathIndex >= (currentPath != null ? currentPath.Count : 0))
            {
                Debug.Log("currentPathIndex 超出範圍，跳過 HandleMovement。");
            }
        }
    }

    private void HandleCommands()
    {
        if (selectedPiece != null)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // 嘗試攻擊
                if (selectedPiece is Warrior warrior)
                {
                    var enemy = warrior.DetectEnemy();
                    if (enemy != null)
                    {
                        warrior.Attack(enemy);
                    }
                    else
                    {
                        Debug.Log("沒有可攻擊的敵人");
                    }
                }
                else
                {
                    Debug.LogWarning($"{selectedPiece.gameObject.name} 不是 Warrior，無法攻擊");
                }
            }
        }
    }
}
