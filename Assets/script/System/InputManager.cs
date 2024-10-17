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
    public float moveSpeed = 5f; // �Ѥl���ʳt��

    public LayerMask selectableLayers;

    // �Ω�s�x��e���G����l
    private List<GridCell> highlightedCells = new List<GridCell>();

    // �Ω�s�x�襤�Ѥl����l�C��
    private Color originalChessPieceColor;

    // ��e���|
    private List<GridCell> currentPath = new List<GridCell>();
    private int currentPathIndex = 0;
    private bool isMoving = false;

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        aStar = FindObjectOfType<AStarPathfinding>();
        if (gridManager == null)
        {
            Debug.LogError("GridManager ���b���������I");
        }
        if (aStar == null)
        {
            Debug.LogError("AStarPathfinding ���b���������I");
        }

        if (Camera.main == null)
        {
            Debug.LogError("Main Camera ���]�m�Υ����I");
        }

        if (EventSystem.current == null)
        {
            Debug.LogError("EventSystem ���b���������I");
        }
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
            Debug.Log("�o�g�g�u");
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, selectableLayers))
            {
                Debug.Log($"�g�u����: {hit.collider.gameObject.name}");
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
                Debug.Log("�g�u������������");
                DeselectCurrentPiece(); // ���I���ťճB�ɡA������ܨòM�Ű��G��l
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
        // �x�s�襤�Ѥl����l�C��
        Renderer rend = selectedPiece.GetComponent<Renderer>();
        if (rend != null)
        {
            originalChessPieceColor = rend.material.color;
            // ���G�襤�Ѥl
            rend.material.color = Color.yellow;
            Debug.Log($"{selectedPiece.gameObject.name} �Q�襤�ð��G���");
        }
        DisplayAvailableMoves();
    }

    private AStarPathfinding GetAStar()
    {
        return aStar;
    }

    private void SelectCell(GridCell cell, AStarPathfinding aStar)
    {
        if (selectedPiece != null && IsCellInRange(cell))
        {
            // �ϥ� A* �M����|
            Vector2Int start = selectedPiece.gridPosition;
            Vector2Int target = cell.gridPosition;
            var (path, totalCost) = aStar.FindPath(start, target);

            if (path != null && path.Count > 1 && totalCost <= selectedPiece.movementRange)
            {
                currentPath = path;
                currentPathIndex = 1; // ���|���Ĥ@���I�O��e��m
                isMoving = true;
                Debug.Log($"���|�w���A�}�l���ʴѤl�A�`����: {totalCost}");
            }
            else
            {
                Debug.Log("���|�������`�����W�X���ʽd��C");
            }

            // ���ߧY������ܴѤl�A�� selectedPiece �O������
        }
    }

    private void DeselectCurrentPiece()
    {
        if (selectedPiece != null)
        {
            // ��_�Ѥl����l�C��
            Renderer rend = selectedPiece.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = originalChessPieceColor;
                Debug.Log($"{selectedPiece.gameObject.name} �C���_�� {originalChessPieceColor}");
            }
            // ��_�Ҧ��i���ʮ�l���C��
            foreach (var cell in highlightedCells)
            {
                cell.ResetMaterial();
            }
            // �M�Ű��G��l�C��
            highlightedCells.Clear();
            selectedPiece = null;
            selectedCell = null;
        }
    }


    private void DisplayAvailableMoves()
    {
        if (selectedPiece != null)
        {
            var accessibleCells = gridManager.GetAccessibleCells(selectedPiece.gridPosition, selectedPiece.movementRange);
            foreach (var cell in accessibleCells)
            {
                cell.Highlight(); // �ϥ� GridCell �� Highlight ��k
                highlightedCells.Add(cell); // �K�[�찪�G�C��
            }
            Debug.Log($"��� {accessibleCells.Count} �ӥi���ʮ�l");
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
                Debug.LogError("targetCell �O null�I");
                isMoving = false;
                currentPath.Clear();
                return;
            }

            if (selectedPiece == null)
            {
                Debug.LogError("selectedPiece �O null�I");
                isMoving = false;
                currentPath.Clear();
                return;
            }

            if (selectedPiece.transform == null)
            {
                Debug.LogError("selectedPiece.transform �O null�I");
                isMoving = false;
                currentPath.Clear();
                return;
            }

            Vector3 targetPosition = targetCell.transform.position;
            targetPosition.y = selectedPiece.transform.position.y; // �O�d�즳�� Y �b��m

            selectedPiece.transform.position = Vector3.MoveTowards(selectedPiece.transform.position, targetPosition, moveSpeed * Time.deltaTime);
            Debug.Log($"{selectedPiece.gameObject.name} ���b���ʨ� {targetPosition}");

            if (Vector3.Distance(selectedPiece.transform.position, targetPosition) < 0.1f)
            {
                selectedPiece.transform.position = targetPosition;
                currentPathIndex++;

                // ��s�Ѥl�� gridPosition
                selectedPiece.UpdateGridPosition(targetCell.gridPosition);
                Debug.Log($"{selectedPiece.gameObject.name} �� gridPosition ��s�� ({targetCell.gridPosition.x}, {targetCell.gridPosition.y})");

                if (currentPathIndex >= currentPath.Count)
                {
                    isMoving = false;
                    currentPath.Clear();
                    Debug.Log("�Ѥl�w��F�ؼЦ�m�C");
                    DeselectCurrentPiece(); // �b���ʧ����������ܴѤl
                }
            }
        }
        else
        {
            if (!isMoving)
            {
                Debug.Log("isMoving �� false�A���L HandleMovement�C");
            }
            if (currentPath == null)
            {
                Debug.Log("currentPath �O null�A���L HandleMovement�C");
            }
            if (currentPathIndex >= (currentPath != null ? currentPath.Count : 0))
            {
                Debug.Log("currentPathIndex �W�X�d��A���L HandleMovement�C");
            }
        }
    }

    private void HandleCommands()
    {
        if (selectedPiece != null)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // ���է���
                if (selectedPiece is Warrior warrior)
                {
                    var enemy = warrior.DetectEnemy();
                    if (enemy != null)
                    {
                        warrior.Attack(enemy);
                    }
                    else
                    {
                        Debug.Log("�S���i�������ĤH");
                    }
                }
                else
                {
                    Debug.LogWarning($"{selectedPiece.gameObject.name} ���O Warrior�A�L�k����");
                }
            }
        }
    }
}
