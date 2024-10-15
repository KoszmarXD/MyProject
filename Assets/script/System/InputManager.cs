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

    // �Ω�s�x��e���G����l
    private List<GridCell> highlightedCells = new List<GridCell>();

    // �Ω�s�x�襤�Ѥl����l�C��
    private Color originalChessPieceColor;

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null)
        {
            Debug.LogError("GridManager ���b���������I");
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
                        SelectCell(cell);
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
            var availableCells = gridManager.GetAvailableCells(selectedPiece.gridPosition, selectedPiece.movementRange, selectedPiece.isPlayerControlled);
            foreach (var cell in availableCells)
            {
                cell.Highlight(); // �ϥ� GridCell �� Highlight ��k
                highlightedCells.Add(cell); // �K�[�찪�G�C��
            }
            Debug.Log($"��� {availableCells.Count} �ӥi���ʮ�l");
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
            Debug.LogWarning("�S���襤���Ѥl�ΥؼЮ�l�� null�I");
            return;
        }
        // �]�w�Ѥl���@�ɦ�m�A�O�d Y �b����
        Vector3 newPosition = targetCell.transform.position;
        newPosition.y = selectedPiece.transform.position.y; // �O�d�즳�� Y �b��m�A�����
        selectedPiece.transform.position = newPosition;
        // ��s�Ѥl���ѽL��m
        selectedPiece.gridPosition = targetCell.gridPosition;
        Debug.Log($"{selectedPiece.gameObject.name} ���ʨ� ({targetCell.gridPosition.x}, {targetCell.gridPosition.y})");
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
                Debug.LogWarning($"{selectedPiece.gameObject.name} ���O Warrior�A�L�k����");
            }
        }
    }
}
