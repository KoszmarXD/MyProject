using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    private GridManager gridManager;
    private ChessPiece selectedPiece;
    private GridCell selectedCell;

    public LayerMask selectableLayers;

    // 用於存儲當前高亮的格子
    private List<GridCell> highlightedCells = new List<GridCell>();

    // 用於存儲選中棋子的原始顏色
    private Color originalChessPieceColor;

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null)
        {
            Debug.LogError("GridManager 未在場景中找到！");
        }

        if (Camera.main == null)
        {
            Debug.LogError("Main Camera 未設置或未找到！");
        }

        if (EventSystem.current == null)
        {
            Debug.LogError("EventSystem 未在場景中找到！");
        }
    }

    void Update()
    {
        HandleSelection();
        HandleCommands();
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
                        SelectCell(cell);
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
        DisplayAvailableMoves();
    }

    private void SelectCell(GridCell cell)
    {
        if (selectedPiece != null && IsCellInRange(cell))
        {
            MoveSelectedPiece(cell);
            DeselectCurrentPiece();
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
            // 恢復所有可移動格子的顏色
            foreach (var cell in highlightedCells)
            {
                cell.ResetMaterial();
            }
            // 清空高亮格子列表
            highlightedCells.Clear();
            selectedPiece = null;
            selectedCell = null;
        }
    }


    private void DisplayAvailableMoves()
    {
        if (selectedPiece != null)
        {
            var availableCells = gridManager.GetAvailableCells(selectedPiece.gridPosition, selectedPiece.movementRange, selectedPiece.isPlayerControlled);
            foreach (var cell in availableCells)
            {
                cell.Highlight(); // 使用 GridCell 的 Highlight 方法
                highlightedCells.Add(cell); // 添加到高亮列表
            }
            Debug.Log($"顯示 {availableCells.Count} 個可移動格子");
        }
    }

    private bool IsCellInRange(GridCell cell)
    {
        var availableCells = gridManager.GetAvailableCells(selectedPiece.gridPosition, selectedPiece.movementRange, selectedPiece.isPlayerControlled);
        return availableCells.Contains(cell);
    }

    private void MoveSelectedPiece(GridCell targetCell)
    {
        if (selectedPiece == null || targetCell == null)
        {
            Debug.LogWarning("沒有選中的棋子或目標格子為 null！");
            return;
        }
        // 設定棋子的世界位置，保留 Y 軸高度
        Vector3 newPosition = targetCell.transform.position;
        newPosition.y = selectedPiece.transform.position.y; // 保留原有的 Y 軸位置，防止掉落
        selectedPiece.transform.position = newPosition;
        // 更新棋子的棋盤位置
        selectedPiece.gridPosition = targetCell.gridPosition;
        Debug.Log($"{selectedPiece.gameObject.name} 移動到 ({targetCell.gridPosition.x}, {targetCell.gridPosition.y})");
    }
    

    private void HandleCommands()
    {
        if (selectedPiece != null)
        {
            if (selectedPiece is Warrior warrior)
            {
                var enemy = warrior.DetectEnemy();
                if (enemy != null)
                {
                    warrior.Attack(enemy);
                }
                
            }
            else
            {
                Debug.LogWarning($"{selectedPiece.gameObject.name} 不是 Warrior，無法攻擊");
            }
        }
    }
}
